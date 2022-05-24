using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class SettingsHelper : MonoBehaviour
{
    private const string AtoD = ".0;.1ST;.600;.602;.ABW;.ACL;.AFP;.AMI;.ANS;.ASC;.AWW;.CCF;.CSV;.CWK;.DBK;.DITA;.DOC;.DOCM;.DOCX;.DOT;.DOTX;.DWD;";
    private const string EtoN = ".EGT;.EPUB;.EZW;.FDX;.FTM;.FTX;.GDOC;.HTML;.HWP;.HWPML;.LOG;.LWP;.MBP;.MD;.ME;.MCW;.MOBI;.NB;.NBP;.NEIS;.NT;.NQ;";
    private const string OtoT = ".ODM;.ODOC;.ODS;.ODT;.OSHEET;.OTT;.OMM;.PAGES;.PAP;.PDAX;.PDF;.QUOX;.RTF;.RPT;.SDW;.SE;.STW;.SXW;.TEX;.INFO;.TROFF;.TXT;";
    private const string UtoZ = ".UOF;.UOML;.VIA;.WPD;.WPS;.WPT;.WRD;.WRF;.WRI;.XHTML;.XHT;.XLS;.XLSX;.XML;.XPS";
    public const string docExtensions = AtoD + EtoN + OtoT + UtoZ;
    public const string pluginExtensions = ".C;.DISABLED;.DLL;.H;.JAR;.JSON;.META";
    public const string cleanerPath = "Assets/PleebieJeebies/AssetCleaner";

    [MenuItem("Tools/Asset Cleaner/Load Default Settings", false, 201)]
    private static void ResetSettings()
    {
        InitPrefs();
        SettingsWindow.LoadSettings();
        EditorUtility.DisplayDialog("Asset Cleaner", "Default Settings Loaded", "OK");
    }

    public static string GetProjectKeyName(string keyname)
    {
        string returnString = keyname + "_" + Application.dataPath;
        return returnString;
    }

    public static List<string> DecodeString(string data)
    {
        List<string> returnData = new List<string>();
        if (data != "")
        {
            string[] dataArray = data.Split(';', ';');
            for (int i = 0; i < dataArray.Length; i++)
            {
                returnData.Add(dataArray[i]);
            }
        }
        return returnData;
    }

    public static string EncodeList(List<string> data)
    {
        string returnString = "";
        for (int i = 0; i < data.Count; i++)
        {
            returnString += data[i];
            if (i != data.Count - 1) returnString += ";";
        }
        return returnString;
    }

    public static void InitPrefs()
    {
        Debug.Log("Setting Asset Cleaner Preferences to Defaults.");
        //Set Debug Options From EditorPrefs
        EditorPrefs.SetBool("debugSceneAndAssetSearch", false);
        EditorPrefs.SetBool("debugUsedAssetScan", false);
        EditorPrefs.SetBool("debugTreeViewConstruction", false);
        EditorPrefs.SetBool("debugAssetFilesDelete", false);
        EditorPrefs.SetBool("debugEmptyFoldersDelete", false);
        //Set Exclusion Options From EditorPrefs
        EditorPrefs.SetBool("excludeScripts", true);
        EditorPrefs.SetBool("excludeDocuments", true);
        EditorPrefs.SetBool("excludeCustomFileExtensions", true);
        EditorPrefs.SetBool("excludeMaterials", false);
        EditorPrefs.SetBool("excludeScriptables", false);
        //Default Document File Extensions
        EditorPrefs.SetString("documentExtensions", docExtensions);
        EditorPrefs.SetString("customExclusions", pluginExtensions);
        string key = GetProjectKeyName("excludedFolders");
        EditorPrefs.SetString(key, cleanerPath);
        //clear empty folders
        EditorPrefs.SetBool("deleteEmptyFolders", true);
        //popup warning on launch
        EditorPrefs.SetBool("noPopupOnLaunch", false);
        //Set Initalized Key
        EditorPrefs.SetBool("assetCleanerPrefsInitialized", true);
    }

    public static void SaveSettings()
    {
        //debug options
        EditorPrefs.SetBool("debugSceneAndAssetSearch", SettingsWindow.DebugSceneAndAssetSearch);
        EditorPrefs.SetBool("debugUsedAssetScan", SettingsWindow.DebugUsedAssetScan);
        EditorPrefs.SetBool("debugTreeViewConstruction", SettingsWindow.DebugTreeViewConstruction);
        EditorPrefs.SetBool("debugAssetFilesDelete", SettingsWindow.DebugAssetFilesDelete);
        EditorPrefs.SetBool("debugEmptyFoldersDelete", SettingsWindow.DebugEmptyFoldersDelete);
        //option to clear empty folders
        EditorPrefs.SetBool("deleteEmptyFolders", SettingsWindow.DeleteEmptyFolders);
        //exclusion options
        EditorPrefs.SetBool("excludeScripts", SettingsWindow.ExcludeScripts);
        EditorPrefs.SetBool("excludeDocuments", SettingsWindow.ExcludeDocuments);
        EditorPrefs.SetBool("excludeCustomFileExtensions", SettingsWindow.ExcludeCustomFileExtensions);
        EditorPrefs.SetBool("excludeMaterials", SettingsWindow.ExcludeMaterials);
        EditorPrefs.SetBool("excludeScriptables", SettingsWindow.ExcludeScriptables);
        string data = EncodeList(SettingsWindow.DocumentExtensions);
        EditorPrefs.SetString("documentExtensions", data);
        data = EncodeList(SettingsWindow.CustomExclusions);
        EditorPrefs.SetString("customExclusions", data);
        data = EncodeList(SettingsWindow.ExcludedFolders);
        string key = GetProjectKeyName("excludedFolders");
        EditorPrefs.SetString(key, data);
        EditorPrefs.SetBool("noPopupOnLaunch", SettingsWindow.NoPopupOnLaunch);
        if (AssetCleaner.useDebugging) Debug.Log("Asset Cleaner Settings Saved.");
    }

    /// <summary>
    /// check for banned characters in file extensions
    /// </summary>
    /// <param name="extension"></param>
    /// <returns></returns>
    public static bool CheckForValidExtension(string extension)
    {
        if (extension.Contains("<")) return false;
        if (extension.Contains(">")) return false;
        if (extension.Contains(":")) return false;
        if (extension.Contains("\"")) return false;
        if (extension.Contains("/")) return false;
        if (extension.Contains("\\")) return false;
        if (extension.Contains("|")) return false;
        if (extension.Contains("?")) return false;
        if (extension.Contains("*")) return false;
        return true;
    }

    public static List<TreeViewItem> BuildTreeViewItemsFromList(List<string> data)
    {
        List<TreeViewItem> allItems = new List<TreeViewItem>();
        //get Depth
        int indexDepth = 0;
        for (int i = 0; i < data.Count; i++)
        {
            //get displayName
            string displayName = data[i];
            //create new TreeViewItem using i for ID, currentDepeth for depth and name string for displayName
            TreeViewItem newItem = new TreeViewItem(i, indexDepth, displayName);
            //add new TreeViewItem to allItems List
            allItems.Add(newItem);
        }
        // Return List<TreeViewItem>
        return allItems;
    }
}
