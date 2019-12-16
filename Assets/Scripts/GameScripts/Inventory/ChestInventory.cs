using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
namespace FantasyRPG.InventorySystem {
    public class ChestInventory : Inventory {

        private List<Stack<ItemScript>> chestItems;
        private int chestSlots;

        public override void CreateLayout() {
            allSlots = new List<GameObject>();
            for (int i = 0; i < slots; i++) {
                GameObject newSlot = Instantiate(InventoryManager.Instance.slotPrefab);
                newSlot.name = "Slot";
                newSlot.transform.SetParent(this.transform);
                allSlots.Add(newSlot);
                newSlot.GetComponent<Button>().onClick.AddListener(delegate { MoveItem(newSlot); });
                newSlot.SetActive(false);
            }
            hoverYOffset = slotSize * 0.1f;
        }

        public void UpdateLayout(List<Stack<ItemScript>> items, int rows, int slots) {
            this.chestItems = items;
            this.chestSlots = slots;
            inventoryWidth = (slots / rows) * (slotSize + slotPaddingLeft);
            inventoryHeight = rows * (slotSize + slotPaddingTop);
            inventoryRect = GetComponent<RectTransform>();
            inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, inventoryWidth + slotPaddingLeft + marginLeft);
            inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, inventoryHeight + slotPaddingTop + marginTop);
            int columns = slots / rows;
            int index = 0;
            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < columns; x++) {
                    GameObject newSlot = allSlots[index];
                    RectTransform slotRect = newSlot.GetComponent<RectTransform>();
                    newSlot.transform.SetParent(this.transform.parent);
                    slotRect.localPosition = inventoryRect.localPosition + new Vector3(slotPaddingLeft * (x + 1) + (slotSize * x) + marginLeft, -slotPaddingTop * (y + 1) - (slotSize * y) - marginTop);
                    slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slotSize * InventoryManager.Instance.canvas.scaleFactor);
                    slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slotSize * InventoryManager.Instance.canvas.scaleFactor);
                    newSlot.transform.SetParent(this.transform);

                    if (items.Count != 0 && items.Count >= index && items[index].Count > 0) {
                        newSlot.GetComponent<Slot>().AddItems(items[index]);
                    }
                    index++;
                }
            }
        }
        public override void Open() {
            base.Open();
            if (IsOpen) {
                MoveItemsFromChest();
            }
        }

        public void MoveItemsToChest() {
            chestItems.Clear();
            for (int i = 0; i < chestSlots; i++) {
                Slot tmpSlot = allSlots[i].GetComponent<Slot>();
                if (!tmpSlot.IsEmpty) {
                    chestItems.Add(new Stack<ItemScript>(tmpSlot.Items));
                    if (!IsOpen) {
                        tmpSlot.ClearSlot();
                    }
                } else {
                    chestItems.Add(new Stack<ItemScript>());
                }
                if (!IsOpen) {
                    allSlots[i].SetActive(false);
                }
            }
        }
        public void MoveItemsFromChest() {
            for (int i = 0; i < chestSlots; i++) {
                if (chestItems.Count != 0 && chestItems.Count >= i && chestItems[i] != null && chestItems[i].Count > 0) {
                    GameObject newSlot = allSlots[i];
                    newSlot.GetComponent<Slot>().AddItems(chestItems[i]);
                }
            }
            for (int i = 0; i < chestSlots; i++) {
                allSlots[i].SetActive(true);
            }
        }

        protected override IEnumerator FadeOut() {
            yield return StartCoroutine(base.FadeOut());
            MoveItemsToChest();
        }

        public override void SaveInventory() { }
        public override void LoadInventory() {
            foreach (GameObject slot in allSlots) {
                slot.GetComponent<Slot>().ClearSlot();
            }
        }
    }
}