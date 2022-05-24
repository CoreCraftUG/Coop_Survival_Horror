using System;
using UnityEngine;

namespace CoreCraft.Programming.GameSettings
{
    [Serializable]
    public class SettingsFile
    {
        // Game Settings
        public float MouseSensivity;
        public bool MouseInvertX;
        public bool MouseInvertY;

        // Video Settings
        public int Resolution;
        public int WindowMode;
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
        
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public void LoadFromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}