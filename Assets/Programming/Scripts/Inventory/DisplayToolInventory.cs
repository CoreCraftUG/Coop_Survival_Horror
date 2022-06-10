using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace CoreCraft.Inventory
{
    public class DisplayToolInventory : MonoBehaviour
    {
        public InventoryBase Inventory;
        public int XSpace;
        public int YStart;
        public int XStart;
        public int NRColumns;
        public int YSpace;
        Dictionary<InventorySlot, GameObject> itemsDisplayed = new Dictionary<InventorySlot, GameObject>();
        // Start is called before the first frame update
        void Start()
        {
            ClearDisplay();
            CreateDisplay();
        }

        // Update is called once per frame
        void Update()
        {
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
            for(int i = 0; i < Inventory.ItemList.Count; i++)
            {
                var obj = Instantiate(Inventory.ItemList[i].Item.Visual, Vector3.zero, Quaternion.identity, transform);
                obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
                obj.GetComponentInChildren<TextMeshProUGUI>().text = Inventory.ItemList[i].Amount.ToString("n0");
                itemsDisplayed.Add(Inventory.ItemList[i], obj);
            }
        }

        public Vector3 GetPosition(int listPos)
        {
            return new Vector3(XStart + (XSpace * (listPos % NRColumns)),YStart + (-YSpace * (listPos / NRColumns)), 0f);
        }
        public void UpdateDisplay()
        {
            for(int i = 0; i < Inventory.ItemList.Count; i++)
            {
                if (itemsDisplayed.ContainsKey(Inventory.ItemList[i]))
                {
                    itemsDisplayed[Inventory.ItemList[i]].GetComponentInChildren<TextMeshProUGUI>().text = Inventory.ItemList[i].Amount.ToString("n0");
                }
                else
                {
                    var obj = Instantiate(Inventory.ItemList[i].Item.Visual, Vector3.zero, Quaternion.identity, transform);
                    obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
                    obj.GetComponentInChildren<TextMeshProUGUI>().text = Inventory.ItemList[i].Amount.ToString("n0");
                    itemsDisplayed.Add(Inventory.ItemList[i], obj);
                }
            }
        }
    }
}
