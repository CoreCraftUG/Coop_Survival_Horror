using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreCraft.Inventory;
using UnityEngine.InputSystem;

namespace CoreCraft
{
    public class JulianTestPlayer : MonoBehaviour
    {
        public InventoryTools ToolInventory;
        public InventoryParts PartsInventory;
        [SerializeField]private float _itemRange;
        private RaycastHit _itemHit;
        public float MaxOxygen;
        public float CurrentOxygen;
        private ItemOutline itemOutline;

        public void Awake()
        {
            ToolInventory.ItemList.Clear();
            PartsInventory.ItemList.Clear();
        }
        public void FixedUpdate()
        {
            if (Physics.Raycast(this.transform.position, this.transform.forward, out RaycastHit hitInfo, _itemRange))
            {
                
                if (hitInfo.transform.tag == "Item")
                {
                   
                    if (_itemHit.transform != null && _itemHit.transform != hitInfo.transform)
                        if (_itemHit.transform.GetComponentInChildren<ItemOutline>() != null)
                            if (_itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled == true)
                                _itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled = false;

                    _itemHit = hitInfo;
                   
                    if (_itemHit.transform.GetComponentInChildren<ItemOutline>())
                        _itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled = true;
                }
                else
                {
                    if(_itemHit.transform != null && _itemHit.transform.GetComponentInChildren<ItemOutline>() != null)
                        _itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled = false;
                }
            }
            else if (_itemHit.transform != null && _itemHit.transform.GetComponentInChildren<ItemOutline>() != null)
                if (_itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled == true)
                    _itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled = false;
            Debug.DrawRay(transform.position, transform.forward * _itemRange, Color.green, 0.1f);
        }
        public void Interact(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                if (_itemHit.transform != null && _itemHit.transform.GetComponentInChildren<ItemOutline>() != null)
                    if (_itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled == true)
                    {
                        if (_itemHit.transform.GetComponent<TestItem>() != null)
                        {
                            var item = _itemHit.transform.GetComponent<TestItem>();
                            if (item)
                            {

                                if (item.Item.ItemType == EItemType.Part)
                                {
                                    PartsInventory.AddItem(item.Item, (item.Item as ItemsPartBase).PartCount);
                                    Destroy(_itemHit.transform.gameObject);
                                }
                                if (item.Item.ItemType == EItemType.Tool)
                                {
                                    ToolInventory.AddItem(item.Item, 1);
                                    Destroy(_itemHit.transform.gameObject);
                                }
                            }
                        }
                    }
            }
        }
    }
}
