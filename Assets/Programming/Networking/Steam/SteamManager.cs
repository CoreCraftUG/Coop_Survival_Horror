using System;
using Steamworks;
using UnityEngine;

using Logger = CoreCraft.Core.Logger;
using NM = Unity.Netcode.NetworkManager;

namespace CoreCraft.Networking.Steam
{
    public class SteamManager : MonoBehaviour
    {
        [SerializeField] private uint appId;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            try
            {
                if (SteamClient.IsValid)
                    PlayerPrefs.SetString("PlayerName", $"{SteamClient.Name}");

                Logger.Instance.Log("Steam is running",ELogType.Debug);
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e.Message,ELogType.Error);
                throw;
            }
        }

        private void OnApplicationQuit()
        {
            try
            {
                SteamLobbyManager.CurrentLobby?.Leave();

                if (!NM.Singleton)
                    return;

                NM.Singleton.Shutdown();
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e.Message, ELogType.Error);
                throw;
            }
        }
    }
}