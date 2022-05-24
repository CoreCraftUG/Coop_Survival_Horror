using UnityEngine;

namespace CoreCraft.Programming.HelpersAndTools
{
    public class InitializeScriptableObjectGO : MonoBehaviour
    {
        public SO_GameObject SOGameObject;
        public GameObject AssignWithThisGO;

        private void Awake()
        {
            SOGameObject.GameObject = AssignWithThisGO;
        }
    }
}