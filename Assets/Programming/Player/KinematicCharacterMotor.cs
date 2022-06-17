using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace CoreCraft.Character
{
    #region Structs and Enums

    public enum RigidbodyInteractionType
    {
        None,
        Kinematic,
        SimulatedDynamic
    }

    public enum StepHandlingMethod
    {
        None,
        Standard,
        Extra
    }

    public enum MovementSweepState
    {
        Initial,
        AfterFirstHit,
        FoundBlockingCrease,
        FoundBlockingCorner,
    }

    /// <summary>
    /// Represents the entire state of a character motor that is pertinent for simulation.
    /// Use this to save state or revert to past state
    /// </summary>
    [System.Serializable]
    public struct KinematicCharacterMotorState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 BaseVelocity;

        public bool MustUnground;
        public float MustUngroundTime;
        public bool LastMovementIterationFoundAnyGround;
        public CharacterTransientGroundingReport GroundingStatus;

        public Rigidbody AttachedRigidbody;
        public Vector3 AttachedRigidbodyVelocity;
    }

    /// <summary>
    /// Describes an overlap between the character capsule and another collider
    /// </summary>
    public struct OverlapResult
    {
        public Vector3 Normal;
        public Collider Collider;

        public OverlapResult(Vector3 normal, Collider collider)
        {
            Normal = normal;
            Collider = collider;
        }
    }

    /// <summary>
    /// Contains all the information for the motor's grounding status
    /// </summary>
    public struct CharacterGroundingReport
    {
        public bool FoundAnyGround;
        public bool IsStableOnGround;
        public bool SnappingPrevented;
        public Vector3 GroundNormal;
        public Vector3 InnerGroundNormal;
        public Vector3 OuterGroundNormal;

        public Collider GroundCollider;
        public Vector3 GroundPoint;

        public void CopyFrom(CharacterTransientGroundingReport transientGroundingReport)
        {
            FoundAnyGround = transientGroundingReport.FoundAnyGround;
            IsStableOnGround = transientGroundingReport.IsStableOnGround;
            SnappingPrevented = transientGroundingReport.SnappingPrevented;
            GroundNormal = transientGroundingReport.GroundNormal;
            InnerGroundNormal = transientGroundingReport.InnerGroundNormal;
            OuterGroundNormal = transientGroundingReport.OuterGroundNormal;

            GroundCollider = null;
            GroundPoint = Vector3.zero;
        }
    }

    /// <summary>
    /// Contains the simulation-relevant information for the motor's grounding status
    /// </summary>
    public struct CharacterTransientGroundingReport
    {
        public bool FoundAnyGround;
        public bool IsStableOnGround;
        public bool SnappingPrevented;
        public Vector3 GroundNormal;
        public Vector3 InnerGroundNormal;
        public Vector3 OuterGroundNormal;

        public void CopyFrom(CharacterGroundingReport groundingReport)
        {
            FoundAnyGround = groundingReport.FoundAnyGround;
            IsStableOnGround = groundingReport.IsStableOnGround;
            SnappingPrevented = groundingReport.SnappingPrevented;
            GroundNormal = groundingReport.GroundNormal;
            InnerGroundNormal = groundingReport.InnerGroundNormal;
            OuterGroundNormal = groundingReport.OuterGroundNormal;
        }
    }

    /// <summary>
    /// Contains all the information from a hit stability evaluation
    /// </summary>
    public struct HitStabilityReport
    {
        public bool IsStable;

        public bool FoundInnerNormal;
        public Vector3 InnerNormal;
        public bool FoundOuterNormal;
        public Vector3 OuterNormal;

        public bool ValidStepDetected;
        public Collider SteppedCollider;

        public bool LedgeDetected;
        public bool IsOnEmptySideOfLedge;
        public float DistanceFromLedge;
        public bool IsMovingTowardsEmptySideOfLedge;
        public Vector3 LedgeGroundNormal;
        public Vector3 LedgeRightDirection;
        public Vector3 LedgeFacingDirection;
    }

    /// <summary>
    /// Contains the information of hit rigidbodies during the movement phase, so they can be processed afterwards
    /// </summary>
    public struct RigidbodyProjectionHit
    {
        public Rigidbody Rigidbody;
        public Vector3 HitPoint;
        public Vector3 EffectiveHitNormal;
        public Vector3 HitVelocity;
        public bool StableOnHit;
    }

    #endregion

    [RequireComponent(typeof(CapsuleCollider))]
    public class KinematicCharacterMotor : NetworkBehaviour
    {
        // Components
        [Header("Components")]

        [ReadOnly]
        public CapsuleCollider Capsule;


        // Capsule Settings
        [Space,Header("Capsule Settings")]

        [SerializeField,Tooltip("Radius of the Character Capsule")]
        private float CapsuleRadius = 0.5f;

        [SerializeField,Tooltip("Height of the Character Capsule")]
        private float CapsuleHeight = 2f;

        [SerializeField,Tooltip("Height of the Character Capsule")]
        private float CapsuleYOffset = 1f;

        [SerializeField,Tooltip("Physics material of the Character Capsule (Does not affect character movement. Only affects things colliding with it)")]
        private PhysicMaterial CapsulePhysicsMaterial;


        // Grounding settings
        [Space,Header("Grounding settings")]

        [SerializeField, Tooltip("Increases the range of ground detection, to allow snapping to ground at very high speeds")]
        public float GroundDetectionExtraDistance = 0f;  

        [SerializeField, Range(0f, 89f),Tooltip("Maximum slope angle on which the character can be stable")]
        public float MaxStableSlopeAngle = 60f;

        [SerializeField, Tooltip("Which layers can the character be considered stable on")]
        public LayerMask StableGroundLayers = -1; 

        [SerializeField, Tooltip("Notifies the Character Controller when discrete collisions are detected")]
        public bool DiscreteCollisionEvents = false;


        // Step settings
        [Space, Header("Step settings")]

        [SerializeField, Tooltip("Handles properly detecting grounding status on steps, but has a performance cost.")]
        public StepHandlingMethod StepHandling = StepHandlingMethod.Standard;

        [SerializeField, Tooltip("Maximum height of a step which the character can climb")]
        public float MaxStepHeight = 0.5f;

        [SerializeField, Tooltip("Can the character step up obstacles even if it is not currently stable?")]
        public bool AllowSteppingWithoutStableGrounding = false;

        [SerializeField, Tooltip("Minimum length of a step that the character can step on (used in Extra stepping method). Use this to let the character step on steps that are smaller that its radius")]
        public float MinRequiredStepDepth = 0.1f;


        // Ledge settings
        [Space, Header("Ledge settings")]

        [SerializeField, Tooltip("Handles properly detecting ledge information and grounding status, but has a performance cost.")]
        public bool LedgeAndDenivelationHandling = true;
        
        [SerializeField, Tooltip("The distance from the capsule central axis at which the character can stand on a ledge and still be stable")]
        public float MaxStableDistanceFromLedge = 0.5f;

        [SerializeField, Tooltip("Prevents snapping to ground on ledges beyond a certain velocity")]
        public float MaxVelocityForLedgeSnap = 0f;

        [SerializeField, Tooltip("The maximun downward slope angle change that the character can be subjected to and still be snapping to the ground")]
        [Range(1f, 180f)]
        public float MaxStableDenivelationAngle = 180f;


        // Rigidbody interaction settings
        [Space,Header("Rigidbody interaction settings")]

        [SerializeField, Tooltip("Handles properly being pushed by and standing on PhysicsMovers or dynamic rigidbodies. Also handles pushing dynamic rigidbodies")]
        public bool InteractiveRigidbodyHandling = true;

        [SerializeField, Tooltip("How the character interacts with non-kinematic rigidbodies. \"Kinematic\" mode means the character pushes the rigidbodies with infinite force (as a kinematic body would). \"SimulatedDynamic\" pushes the rigidbodies with a simulated mass value.")]
        public RigidbodyInteractionType RigidbodyInteractionType;

        [SerializeField, Tooltip("Mass used for pushing bodies")]
        public float SimulatedCharacterMass = 1f;

        [SerializeField, Tooltip("Determines if the character preserves moving platform velocities when de-grounding from them")]
        public bool PreserveAttachedRigidbodyMomentum = true;


        // Constraints settings
        [Space, Header("Constraints settings")]

        [SerializeField, Tooltip("Determines if the character's movement uses the planar constraint")]
        public bool HasPlanarConstraint = false;

        [SerializeField, Tooltip("Defines the plane that the character's movement is constrained on, if HasMovementConstraintPlane is active")]
        public Vector3 PlanarConstraintAxis = Vector3.forward;


        // Other settings
        [Space, Header("Other settings")]

        [SerializeField, Tooltip("How many times can we sweep for movement per update")]
        public int MaxMovementIterations = 5;

        [SerializeField, Tooltip("How many times can we check for decollision per update")]
        public int MaxDecollisionIterations = 1;

        [SerializeField, Tooltip("Checks for overlaps before casting movement, making sure all collisions are detected even when already intersecting geometry (has a performance cost, but provides safety against tunneling through colliders)")]
        public bool CheckMovementInitialOverlaps = true;

        [SerializeField, Tooltip("Sets the velocity to zero if exceed max movement iterations")]
        public bool KillVelocityWhenExceedMaxMovementIterations = true;

        [SerializeField, Tooltip("Sets the remaining movement to zero if exceed max movement iterations")]
        public bool KillRemainingMovementWhenExceedMaxMovementIterations = true;

        
        [System.NonSerialized]
        public CharacterGroundingReport GroundingStatus = new CharacterGroundingReport();

        [System.NonSerialized]
        public CharacterTransientGroundingReport LastGroundingStatus = new CharacterTransientGroundingReport();

        [System.NonSerialized]
        public LayerMask CollidableLayers = -1;

        
        public Transform Transform { get { return _transform; } }
        private Transform _transform;

        public Vector3 TransientPosition { get { return _transientPosition; } }
        private Vector3 _transientPosition;

        public Vector3 CharacterUp { get { return _characterUp; } }
        private Vector3 _characterUp;

        public Vector3 CharacterForward { get { return _characterForward; } }
        private Vector3 _characterForward;

        public Vector3 CharacterRight { get { return _characterRight; } }
        private Vector3 _characterRight;

        public Vector3 InitialSimulationPosition { get { return _initialSimulationPosition; } }
        private Vector3 _initialSimulationPosition;

        public Quaternion InitialSimulationRotation { get { return _initialSimulationRotation; } }
        private Quaternion _initialSimulationRotation;

        public Rigidbody AttachedRigidbody { get { return _attachedRigidbody; } }
        private Rigidbody _attachedRigidbody;

        public Vector3 CharacterTransformToCapsuleCenter { get { return _characterTransformToCapsuleCenter; } }
        private Vector3 _characterTransformToCapsuleCenter;

        public Vector3 CharacterTransformToCapsuleBottom { get { return _characterTransformToCapsuleBottom; } }
        private Vector3 _characterTransformToCapsuleBottom;

        public Vector3 CharacterTransformToCapsuleTop { get { return _characterTransformToCapsuleTop; } }
        private Vector3 _characterTransformToCapsuleTop;

        public Vector3 CharacterTransformToCapsuleBottomHemi { get { return _characterTransformToCapsuleBottomHemi; } }
        private Vector3 _characterTransformToCapsuleBottomHemi;

        public Vector3 CharacterTransformToCapsuleTopHemi { get { return _characterTransformToCapsuleTopHemi; } }
        private Vector3 _characterTransformToCapsuleTopHemi;

        public Vector3 AttachedRigidbodyVelocity { get { return _attachedRigidbodyVelocity; } }
        private Vector3 _attachedRigidbodyVelocity;

        public int OverlapsCount { get { return _overlapsCount; } }
        private int _overlapsCount;

        // public OverlapResult[] Overlaps { get { return _overlaps; } }
        // private OverlapResult[] _overlaps = new OverlapResult[MaxRigidbodyOverlapsCount];
    }
}