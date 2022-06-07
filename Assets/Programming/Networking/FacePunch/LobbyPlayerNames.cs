using System.Collections;
using System.Collections.Generic;
using CoreCraft.Core;
using Steamworks;
using TMPro;
using UnityEngine;

namespace CoreCraft
{
    public class LobbyPlayerNames : Singleton<LobbyPlayerNames>
    {
        [SerializeField] private GameObject _namePanel;
        [SerializeField] private Transform _targetContent;

        public void CreateNamePanel(string name)
        {
            GameObject obj = Instantiate(_namePanel, _targetContent);

            obj.GetComponentInChildren<TMP_Text>().text = name;
        }

        public void ClearContent()
        {
            for (int i = _targetContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_targetContent.GetChild(i).gameObject);
            }
        }
    }
}
