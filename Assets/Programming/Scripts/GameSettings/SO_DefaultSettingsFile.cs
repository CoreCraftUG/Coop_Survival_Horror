using System;
using CoreCraft.Programming.HelpersAndTools;
using UnityEngine;

namespace CoreCraft.Programming.GameSettings
{
    [CreateAssetMenu(menuName = "ScriptableObjects/DefaultSettings")]
    public class SO_DefaultSettingsFile : ScriptableObject
    {
        [Header("Save File JSON Data")]
        public DefaultSettingsFile SaveData = new DefaultSettingsFile();
    }

    [Serializable]
    public class DefaultSettingsFile
    {
        // Game Settings
        public float MouseSensivity;
        public bool MouseInvertX;
        public bool MouseInvertY;

        // Video Settings
        [Attribute_ReadOnly] public int Resolution;
        [Attribute_ReadOnly] public int WindowMode;
        public float Brightness;
        public float RenderDistance;
        public bool VSync;
        public int AntiAliasing;
        public int ShadowQuality;
        public bool SoftShadows;
        public int TextureQuality;
        public int AnisotropicTextures;
        public bool SoftParticles;
        public bool RealtimeReflectionProbes;
        public bool BillboardsFacingCameraPositions;
        public int SkinWeights;
        public int LODBias;
        public int ParticleRaycastBudget;
        public float FieldOfView;
        public bool SSAO;
    }
}