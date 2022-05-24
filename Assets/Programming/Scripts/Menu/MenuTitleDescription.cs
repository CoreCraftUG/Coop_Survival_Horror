using UnityEngine;

namespace CoreCraft.Programming.Menu
{
    public class MenuTitleDescription : MonoBehaviour
    {
        private string _title;
        [SerializeField] public string Title { get => _title; set => _title = value; }
    }
}