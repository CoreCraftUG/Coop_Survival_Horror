using System;
using TMPro;
using UnityEngine;

public enum ELogType
{
    Error,
    Debug,
    Info
}


namespace CoreCraft.Core
{
    public class Logger : Singleton<Logger>
    {
        [SerializeField] private TMP_Text _loggerText;

        private void Update()
        {

        }
        
        public void Log(string logText, ELogType logType)
        {
            switch (logType)
            {
                case ELogType.Error:
                    ErrorLog(logText);
                    break;
                case ELogType.Debug:
                    DebugLog(logText);
                    break;
                case ELogType.Info:
                    InfoLog(logText);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }

        private void InfoLog(string logText)
        {
            _loggerText.text += $"<color=#FFFFFF>{logText}\n</color>";
        }

        private void DebugLog(string logText)
        {
            _loggerText.text += $"<color=#0000FF>{logText}\n</color>";
        }

        private void ErrorLog(string logText)
        {
            _loggerText.text += $"<color=#FF0000>{logText}\n</color>";
        }
    }
}