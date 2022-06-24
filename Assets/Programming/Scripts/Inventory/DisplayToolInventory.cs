using System;
using System.Collections;
using System.Collections.Generic;
using CoreCraft.Character;
using UnityEngine;
using TMPro;
using Unity.Netcode;


namespace CoreCraft.Inventory
{
    enum EInventoryType
    {
        Tool,
        Part
    }

    public class DisplayToolInventory : NetworkBehaviour
    {
        [SerializeField] private EInventoryType _inventoryType;
        public InventoryBase Inventory;
        public int XSpace;
        public int YStart;
        public int XStart;
        public int NRColumns;
        public int YSpace;
        Dictionary<InventorySlot, GameObject> itemsDisplayed = new Dictionary<InventorySlot, GameObject>();

        private bool _inventoryReady;
        // Start is called before the first frame update
        void Awake()
        {
            // InventorySetUpServerRpc(NetworkManager.Singleton.LocalClientId);
            
            EventManager.Instance.InventoryReady.AddListener(AddInventory);
        }

        private void AddInventory(ulong clientId)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId || NetworkManager.Singleton.LocalClient.PlayerObject == null)
                return;

            switch (_inventoryType)
            {
                case EInventoryType.Tool:
                    Inventory = NetworkManager.Singleton.LocalClient.PlayerObject.transform.gameObject.GetComponent<ItemPickUp>().ToolInventory;
                    break;
                case EInventoryType.Part:
                    Inventory = NetworkManager.Singleton.LocalClient.PlayerObject.transform.gameObject.GetComponent<ItemPickUp>().PartsInventory;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _inventoryReady = true;
            ClearDisplay();
            CreateDisplay();
        }

        /*[ServerRpc]
        private void InventorySetUpServerRpc(ulong clientId)
        {
            InventorySetUpClientRpc(NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.NetworkObjectId,clientId);
        }

        [ClientRpc]
        private void InventorySetUpClientRpc(ulong playerObjectNetworkId, ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                switch (_inventoryType)
                {
                    case EInventoryType.Tool:
                        Inventory = playerObj.transform.gameObject.GetComponent<ItemPickUp>().ToolInventory;
                        break;
                    case EInventoryType.Part:
                        Inventory = playerObj.transform.gameObject.GetComponent<ItemPickUp>().PartsInventory;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }*/

        // Update is called once per frame
        void FixedUpdate()
        {
            if(_inventoryReady)
                UpdateDisplay();
        }

        public void ClearDisplay()
        {
            itemsDisplayed.Clear();
            foreach(Transform x in this.transform)
            {
                GameObject.Destroy(x.gameObject);
            }

        }

        public void CreateDisplay()
        {
            for(int i = 0; i < Inventory.ItemClientList.Count; i++)
            {
                var obj = Instantiate(Inventory.ItemClientList[i].Item.Visual, Vector3.zero, Quaternion.identity, transform);
                obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
                obj.GetComponentInChildren<TextMeshProUGUI>().text = Inventory.ItemClientList[i].Amount.ToString("n0");
                itemsDisplayed.Add(Inventory.ItemClientList[i], obj);
            }
        }

        public Vector3 GetPosition(int listPos)
        {
            return new Vector3(XStart + (XSpace * (listPos % NRColumns)),YStart + (-YSpace * (listPos / NRColumns)), 0f);
        }
        public void UpdateDisplay()
        {
            for(int i = 0; i < Inventory.ItemClientList.Count; i++)
            {
                if (itemsDisplayed.ContainsKey(Inventory.ItemClientList[i]))
                {
                    itemsDisplayed[Inventory.ItemClientList[i]].GetComponentInChildren<TextMeshProUGUI>().text = Inventory.ItemClientList[i].Amount.ToString("n0");
                }
                else
                {
                    var obj = Instantiate(Inventory.ItemClientList[i].Item.Visual, Vector3.zero, Quaternion.identity, transform);
                    obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
                    obj.GetComponentInChildren<TextMeshProUGUI>().text = Inventory.ItemClientList[i].Amount.ToString("n0");
                    itemsDisplayed.Add(Inventory.ItemClientList[i], obj);
                    Debug.Log($"Client: {NetworkManager.Singleton.LocalClientId} displayed Item: {itemsDisplayed[Inventory.ItemClientList[i]]}");
                }
            }
        }
    }
}
