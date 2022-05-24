#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Snappables {
    public class SnapPoint : MonoBehaviour {
		[HideInInspector] public List<SnapPoint> linkedPoints = new List<SnapPoint>();
		[Tooltip("Disables point linking on this point, allowing multiple objects to be placed on it")]
		public bool overrideLink = false;

		private void Reset() {
			//Tag / name
			gameObject.tag = "EditorOnly";
			gameObject.name = "SnapPoint";

			//Sphere Collider for Physics.OverlapSphere
			SphereCollider existingCol = gameObject.GetComponent<SphereCollider>();
			if(existingCol != null)
				DestroyImmediate(existingCol); //Delete existing sphere collider if resetting again

			SphereCollider col = gameObject.AddComponent<SphereCollider>();
			col.isTrigger = true;
			col.radius = 0.1f;

			linkedPoints.Clear();
		}

		private void OnDrawGizmosSelected() {
			if(Selection.activeGameObject != gameObject)
				return;

			Color cache = Gizmos.color;
			Gizmos.color = Color.blue;
			Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.forward));
			Handles.Label(transform.position + transform.TransformDirection(Vector3.forward), "SnapPoint Forwards");
			Gizmos.color = cache;
		}
	}
}
#endif