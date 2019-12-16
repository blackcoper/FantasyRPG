using UnityEngine;
using System.Collections;
using System;
namespace FantasyRPG.InventorySystem {
    public class HotkeyPanel : Inventory {

        public Slot[] hotkeySlots;

        private static HotkeyPanel instance;

        public static HotkeyPanel Instance {
            get {
                if (instance == null) {
                    instance = GameObject.FindObjectOfType<HotkeyPanel>();
                }
                return instance;
            }
        }

        void Awake() {
            hotkeySlots = transform.GetComponentsInChildren<Slot>();
        }

        public override void CreateLayout() {
            inventoryRect = transform.GetComponent<RectTransform>();
        }
        public override void SaveInventory() {
            string content = string.Empty;
            for (int i = 0; i < hotkeySlots.Length; i++) {
                if (!hotkeySlots[i].IsEmpty) {
                    content += i + "{0}" + hotkeySlots[i].Items.Peek().Item.ItemName + "{1}";
                }
            }
            PlayerPrefs.SetString("HotkeyPanel", content);
            PlayerPrefs.Save();
        }
        public override void LoadInventory() {
            foreach (Slot slot in hotkeySlots) {
                slot.ClearSlot();
            }
            string content = PlayerPrefs.GetString("HotkeyPanel");
            string[] splitContent = content.Split(new string[] { "{1}" }, StringSplitOptions.None);
            for (int i = 0; i < splitContent.Length - 1; i++) {
                string[] splitValues = splitContent[i].Split(new string[] { "{0}" }, StringSplitOptions.None);
                int index = Int32.Parse(splitValues[0]);
                string itemName = splitValues[1];
                GameObject loadedItem = Instantiate(InventoryManager.Instance.itemObject);
                loadedItem.AddComponent<ItemScript>();
                if (index == 9 || index == 10) {
                    loadedItem.GetComponent<ItemScript>().Item = InventoryManager.Instance.ItemContainer.Weapons.Find(x => x.ItemName == itemName);
                } else {
                    loadedItem.GetComponent<ItemScript>().Item = InventoryManager.Instance.ItemContainer.Equipment.Find(x => x.ItemName == itemName);
                }
                hotkeySlots[index].AddItem(loadedItem.GetComponent<ItemScript>());
                Destroy(loadedItem);
            }
        }
    }
}