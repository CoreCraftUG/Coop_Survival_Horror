using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft
{
    public class ItemOutline : MonoBehaviour
    {
        [SerializeField] private Material outlineMaterial;

        [SerializeField] private float outlineScaleFactor;

        [SerializeField] private Color outlineColor;

        public Renderer OutlineRenderer;

        [SerializeField] private Collider _thisCollision;


        private void Start()
        {
            OutlineRenderer = CreateOutline(outlineMaterial, outlineScaleFactor, outlineColor);
            OutlineRenderer.enabled = false;
        }

        Renderer CreateOutline(Material outlineMat, float scaleFactor, Color color)

        {
            GameObject outlineObject = Instantiate(this.gameObject, transform.position, transform.rotation, transform);
            Renderer rend = outlineObject.GetComponent<Renderer>();

            rend.material = outlineMat;

            rend.material.SetColor("_OutlineColor", color);

            rend.material.SetFloat("_Scale", scaleFactor);

            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
  
            outlineObject.GetComponent<ItemOutline>()._thisCollision = _thisCollision;
            outlineObject.GetComponent<ItemOutline>().enabled = false;
            //outlineObject.GetComponent<Collider>().enabled = false;
            rend.enabled = false;
            return rend;
        }
    }
}
