using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
namespace FantasyRPG.InventorySystem {
    public class CraftingBench : Inventory {
        private static CraftingBench instance;
        public GameObject prefabButton;
        private GameObject previewSlot;
        private Dictionary<string, Item> craftingItems = new Dictionary<string, Item>();

        public static CraftingBench Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CraftingBench>();
                }
                return instance;
            }
        }

        public override void CreateLayout() {
            base.CreateLayout();
            inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, inventoryHeight + slotSize + slotPaddingTop * 2 + marginTop);
            if (!GameObject.Find("CraftButton")) {
                GameObject craftBtn;
                craftBtn = Instantiate(prefabButton);
                RectTransform btnRect = craftBtn.GetComponent<RectTransform>();
                craftBtn.name = "CraftButton";
                craftBtn.transform.SetParent(this.transform.parent);
                btnRect.localPosition = inventoryRect.localPosition + new Vector3(slotPaddingLeft + marginLeft, -slotPaddingTop * 4 - (slotSize * 3) - marginTop);
                btnRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ((slotSize * 2) + (slotPaddingLeft * 2)) * InventoryManager.Instance.canvas.scaleFactor);
                btnRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slotSize * InventoryManager.Instance.canvas.scaleFactor);
                craftBtn.transform.SetParent(this.transform);
                craftBtn.GetComponent<Button>().onClick.AddListener(CraftItem);
            }
            if (!GameObject.Find("PreviewSlot")) {
                previewSlot = Instantiate(InventoryManager.Instance.slotPrefab);
                RectTransform slotRect = previewSlot.GetComponent<RectTransform>();
                previewSlot.name = "PreviewSlot";
                previewSlot.transform.SetParent(this.transform.parent);
                slotRect.localPosition = inventoryRect.localPosition + new Vector3((slotPaddingLeft * 3) + (slotSize * 2) + marginLeft, -slotPaddingTop * 4 - (slotSize * 3) - marginTop);
                slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slotSize * InventoryManager.Instance.canvas.scaleFactor);
                slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slotSize * InventoryManager.Instance.canvas.scaleFactor);
                previewSlot.transform.SetParent(this.transform);
                previewSlot.GetComponent<Slot>().Clickable = false;
            }
        }

        public void CreateBlueprints() {
            craftingItems.Add("EMPTY-Iron-EMPTY-EMPTY-Iron-EMPTY-EMPTY-Wood-EMPTY-", InventoryManager.Instance.ItemContainer.Weapons.Find(x => x.ItemName == "Shortsword"));
            craftingItems.Add("EMPTY-EMPTY-EMPTY-Stone-Stone-Stone-Stone-EMPTY-Stone-", InventoryManager.Instance.ItemContainer.Equipment.Find(x => x.ItemName == "Rusty Helmet"));
        }

        public void CraftItem() {
            string output = string.Empty;
            foreach (GameObject slot in allSlots) {
                Slot tmp = slot.GetComponent<Slot>();
                if (tmp.IsEmpty) {
                    output += "EMPTY-";
                } else {
                    output += tmp.CurrentItem.Item.ItemName + "-";
                }
            }
            if (craftingItems.ContainsKey(output)) {
                GameObject tmpObj = Instantiate(InventoryManager.Instance.itemObject);
                tmpObj.AddComponent<ItemScript>();
                ItemScript craftedItem = tmpObj.GetComponent<ItemScript>();
                Item tmpItem;
                craftingItems.TryGetValue(output, out tmpItem);
                if (tmpItem != null) {
                    craftedItem.Item = tmpItem;
                    if (PlayerHandleItem.Instance.inventory.AddItem(craftedItem)) {
                        foreach (GameObject slot in allSlots) {
                            slot.GetComponent<Slot>().RemoveItem();
                        }
                    }
                    Destroy(tmpObj);
                }
            }
            updatePreview();
        }
        public override void MoveItem(GameObject clicked) {
            base.MoveItem(clicked);
            updatePreview();
        }

        public void updatePreview() {
            string output = string.Empty;
            previewSlot.GetComponent<Slot>().ClearSlot();
            foreach (GameObject slot in allSlots) {
                Slot tmp = slot.GetComponent<Slot>();
                if (tmp.IsEmpty) {
                    output += "EMPTY-";
                } else {
                    output += tmp.CurrentItem.Item.ItemName + "-";
                }
            }
            if (craftingItems.ContainsKey(output)) {
                GameObject tmpObj = Instantiate(InventoryManager.Instance.itemObject);
                tmpObj.AddComponent<ItemScript>();
                ItemScript craftedItem = tmpObj.GetComponent<ItemScript>();
                Item tmpItem;
                craftingItems.TryGetValue(output, out tmpItem);
                if (tmpItem != null) {
                    craftedItem.Item = tmpItem;
                    previewSlot.GetComponent<Slot>().AddItem(craftedItem);
                    Destroy(tmpObj);
                }
            }
        }
        public override void LoadInventory() {
            base.LoadInventory();
            updatePreview();
        }
        public override void Open() {
            base.Open();
            foreach (GameObject slot in allSlots) {
                Slot tmpSlot = slot.GetComponent<Slot>();
                int count = tmpSlot.Items.Count;
                for (int i = 0; i < count; i++) {
                    ItemScript tmpItem = tmpSlot.RemoveItem();
                    if (!PlayerHandleItem.Instance.inventory.AddItem(tmpItem)) {
                        float angle = UnityEngine.Random.Range(0.0f, Mathf.PI * 2);
                        Vector3 v = new Vector3(Mathf.Sin(angle), -0.5f, Mathf.Cos(angle));
                        v *= 1.5f;
                        GameObject tmpDrp = GameObject.Instantiate(InventoryManager.Instance.dropItem, playerRef.transform.position - v, Quaternion.identity) as GameObject;
                        tmpDrp.AddComponent<ItemScript>();
                        tmpDrp.GetComponent<ItemScript>().Item = tmpItem.Item;
                    }
                }
            }
            updatePreview();
        }
    }
}