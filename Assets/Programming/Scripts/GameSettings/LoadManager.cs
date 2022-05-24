using System;
using CoreCraft.Programming.HelpersAndTools;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CoreCraft.Programming.GameSettings
{
    public class LoadManager : MonoBehaviour
    {
        [SerializeField] private VolumeProfile _postProcessVolume;

        [Header("SO_Settings")] [SerializeField]
        private SO_Settings _sOSettings;

        [SerializeField] private SO_GameObject _sOCamera;

        [Header("Settings Components")] [SerializeField]
        private VideoGameSettings _gameMenu;

        [SerializeField] private VideoSettings _videoMenu;

        private FullScreenMode _screenMode;

        private void Start()
        {
            if (_sOSettings != null && !_sOSettings.LoadSaveSettingsDataFromDisk())
            {
                _sOSettings.SetNewSettingsData();
            }

            VideoGameSettings();
            VideoSettings();
        }

        #region Game Settings

        private void VideoGameSettings()
        {
            _gameMenu.SetValues();
        }

        #endregion

        #region Video Settings

        private void VideoSettings()
        {
            _videoMenu.SetValues();

            SetResolutionAndWindowMode(_sOSettings.SettingsData.Resolution, _sOSettings.SettingsData.WindowMode);
            if (_postProcessVolume.TryGet<ColorAdjustments>(out ColorAdjustments colorAdjustments))
                colorAdjustments.postExposure.Override(_sOSettings.SettingsData.Brightness);
            _sOCamera.GameObject.GetComponent<Camera>().farClipPlane = _sOSettings.SettingsData.RenderDistance;

            _videoMenu.VerticalSync(_sOSettings.SettingsData.VSync);
            _videoMenu.AntiAliasing(_sOSettings.SettingsData.AntiAliasing);
            _videoMenu.ShadowQuality(_sOSettings.SettingsData.ShadowQuality);
            _videoMenu.SoftShadows(_sOSettings.SettingsData.SoftShadows);
            _videoMenu.TextureQuality(_sOSettings.SettingsData.TextureQuality);
            _videoMenu.AnisotropicTextures(_sOSettings.SettingsData.AnisotropicTextures);
            _videoMenu.SoftParticles(_sOSettings.SettingsData.SoftParticles);
            _videoMenu.RealtimeReflectionProbes(_sOSettings.SettingsData.RealtimeReflectionProbes);
            _videoMenu.BillboardsFacingCameraPositions(_sOSettings.SettingsData.BillboardsFacingCameraPositions);
            _videoMenu.SkinWeights(_sOSettings.SettingsData.SkinWeights);
            _videoMenu.LODBias(_sOSettings.SettingsData.LODBias);
            _videoMenu.ParticleRaycastBudget(_sOSettings.SettingsData.ParticleRaycastBudget);

            _sOCamera.GameObject.GetComponent<Camera>().fieldOfView = _sOSettings.SettingsData.FieldOfView;

            _videoMenu.SSAO(_sOSettings.SettingsData.SSAO);
        }

        private void SetResolutionAndWindowMode(int resIndex, int windowModeIndex)
        {
            Resolution[] resolutions = Screen.resolutions;
            Array.Reverse(resolutions);

            switch (windowModeIndex)
            {
                case 0:
                    _screenMode = FullScreenMode.ExclusiveFullScreen;
                    break;
                case 1:
                    _screenMode = FullScreenMode.Windowed;
                    break;
                case 2:
                    _screenMode = FullScreenMode.FullScreenWindow;
                    break;
            }

            Screen.SetResolution(resolutions[resIndex].width, resolutions[resIndex].height, _screenMode);
        }

        #endregion
    }
}