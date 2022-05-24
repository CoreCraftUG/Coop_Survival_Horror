using UnityEngine;

namespace CoreCraft.Programming.GameSettings
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Settings")]
    public class SO_Settings : ScriptableObject
    {
        [SerializeField] private SO_DefaultSettingsFile _defaultSettingsFile;
        public string SaveSettingsFileName = "settings.ini";

        [Header("Settings File JSON Data")] 
        public SettingsFile SettingsData = new SettingsFile();

        public bool LoadSaveSettingsDataFromDisk()
        {
            if (SaveAndLoadSettings.LoadFromFile(SaveSettingsFileName, out var json))
            {
                SettingsData.LoadFromJson(json);
                return true;
            }

            return false;
        }

        public void SaveDataToDisk()
        {
            if (SaveAndLoadSettings.WriteToFile(SaveSettingsFileName, SettingsData.ToJson()))
            {
                Debug.Log("Settings saved successfully: " + SaveSettingsFileName);
            }
        }

        public void SetNewSettingsData()
        {
            SaveAndLoadSettings.WriteToFile(SaveSettingsFileName, "");

            // Game Settings
            SettingsData.MouseSensivity = _defaultSettingsFile.SaveData.MouseSensivity;
            SettingsData.MouseInvertX = _defaultSettingsFile.SaveData.MouseInvertX;
            SettingsData.MouseInvertY = _defaultSettingsFile.SaveData.MouseInvertY;

            // Video Settings
            SettingsData.Resolution = _defaultSettingsFile.SaveData.Resolution;
            SettingsData.WindowMode = _defaultSettingsFile.SaveData.WindowMode;
            SettingsData.Brightness = _defaultSettingsFile.SaveData.Brightness;
            SettingsData.RenderDistance = _defaultSettingsFile.SaveData.RenderDistance;
            SettingsData.VSync = _defaultSettingsFile.SaveData.VSync;
            SettingsData.AntiAliasing = _defaultSettingsFile.SaveData.AntiAliasing;
            SettingsData.ShadowQuality = _defaultSettingsFile.SaveData.ShadowQuality;
            SettingsData.SoftShadows = _defaultSettingsFile.SaveData.SoftShadows;
            SettingsData.TextureQuality = _defaultSettingsFile.SaveData.TextureQuality;
            SettingsData.AnisotropicTextures = _defaultSettingsFile.SaveData.AnisotropicTextures;
            SettingsData.SoftParticles = _defaultSettingsFile.SaveData.SoftParticles;
            SettingsData.RealtimeReflectionProbes = _defaultSettingsFile.SaveData.RealtimeReflectionProbes;
            SettingsData.BillboardsFacingCameraPositions = _defaultSettingsFile.SaveData.BillboardsFacingCameraPositions;
            SettingsData.SkinWeights = _defaultSettingsFile.SaveData.SkinWeights;
            SettingsData.LODBias = _defaultSettingsFile.SaveData.LODBias;
            SettingsData.ParticleRaycastBudget = _defaultSettingsFile.SaveData.ParticleRaycastBudget;
            SettingsData.FieldOfView = _defaultSettingsFile.SaveData.FieldOfView;
            SettingsData.SSAO = _defaultSettingsFile.SaveData.SSAO;

            SaveDataToDisk();
        }
    }
}