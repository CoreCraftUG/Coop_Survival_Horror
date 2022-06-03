using System;
using Steamworks;
using UnityEngine;
using Logger = CoreCraft.Core.Logger;

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
                Steamworks.SteamClient.Init(appId, true);
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
                Steamworks.SteamClient.Shutdown();
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e.Message, ELogType.Error);
                throw;
            }
        }
    }
}