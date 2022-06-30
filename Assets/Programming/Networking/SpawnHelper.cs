using System.Collections;
using System.Collections.Generic;
using CoreCraft.Networking;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CoreCraft
{
    public class SpawnHelper : MonoBehaviour
    {
        [SerializeField] private Vector3 _spawnPosition = Vector3.zero;

        public void Spawn()
        {
            PlayerSpawnManager.Instance.SpawnPlayerCharacterServerRpc((ulong)0,_spawnPosition,Quaternion.identity);
        }
    }
}
