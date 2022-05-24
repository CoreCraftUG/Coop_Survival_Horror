using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TypeFinder : MonoBehaviour
{
    public static List<string> GetAllScriptableObjects<T>() where T : ScriptableObject
    {
        string[] searchFolders = new string[1];
        searchFolders[0] = "Assets";
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, searchFolders);
        List<string> returnList = new List<string>();
        for (int i = 0; i < guids.Length; i++)          
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            returnList.Add(path);
        }
        return returnList;
    }

    public static List<string> GetAllScenes<T>() where T : SceneAsset
    {
        string[] searchFolders = new string[1];
        searchFolders[0] = "Assets";
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, searchFolders);
        List<string> returnList = new List<string>();
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            returnList.Add(path);
        }
        return returnList;
    }
}
