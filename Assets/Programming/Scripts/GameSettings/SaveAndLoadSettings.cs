using System;
using UnityEngine;
using System.IO;

namespace CoreCraft.Programming.GameSettings
{
    public static class SaveAndLoadSettings
    {
        public static bool WriteToFile(string fileName, string fileContents)
        {
            var filePath = Path.Combine(Application.persistentDataPath, fileName);

            try
            {
                File.WriteAllText(filePath, fileContents);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to {filePath} with exception {e}");
                return false;
            }
        }

        public static bool LoadFromFile(string fileName, out string result)
        {
            var filePath = Path.Combine(Application.persistentDataPath, fileName);

            if (!File.Exists(filePath))
            {
                result = "";
                return false;
            }

            try
            {
                result = File.ReadAllText(filePath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read from {filePath} with exception {e}");
                result = "";
                return false;
            }
        }
    }
}