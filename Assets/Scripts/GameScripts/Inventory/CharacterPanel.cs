using UnityEngine;
using System.Collections;
using System;
namespace FantasyRPG.InventorySystem {
    public class CharacterPanel : Inventory {

        public Slot[] equipmentSlots;

        private static CharacterPanel instance;

        public static CharacterPanel Instance {
            get {
                if (instance == null) {
                    instance = GameObject.FindObjectOfType<CharacterPanel>();
                }
                return instance;
            }
        }

        public Slot MainHandSlot {
            get { return equipmentSlots[9]; }
        }

        public Slot OffHandSlot {
            get { return equipmentSlots[10]; }
        }

        void Awake() {
            equipmentSlots = transform.GetComponentsInChildren<Slot>();
        }

        public override void CreateLayout() {
            inventoryRect = transform.parent.GetComponent<RectTransform>();
        }

        public void EquipItem(Slot slot, ItemScript item) {
            ItemType swapType = item.Item.ItemType;
            //if (swapType == ItemType.MAINHAND || swapType == ItemType.TWOHAND && OffHandSlot.IsEmpty) {
            //    Slot.SwapItems(slot, MainHandSlot);
            //} else { 
            if (swapType == ItemType.MAINHAND || swapType == ItemType.TWOHAND) {
                swapType = ItemType.GENERICWEAPON;
            }
            Slot.SwapItems(slot, Array.Find(equipmentSlots, x => x.canContain == swapType));
            //}
        }

        public override void ShowToolTip(GameObject slot) {
            Slot tmpSlot = slot.GetComponent<Slot>();
            if (slot.GetComponentInParent<Inventory>().IsOpen && !tmpSlot.IsEmpty && InventoryManager.Instance.HoverObject == null && !InventoryManager.Instance.selectStackSize.activeSelf) {
                InventoryManager.Instance.visualTextObject.text = tmpSlot.CurrentItem.GetTooltip(this);
                InventoryManager.Instance.sizeTextObject.text = InventoryManager.Instance.visualTextObject.text;
                InventoryManager.Instance.tooltipObject.SetActive(true);
                InventoryManager.Instance.tooltipObject.transform.position = slot.transform.position;
            }
        }

        public void CalcStats() {
            int strength = 0;
            int agility = 0;
            int intellect = 0;
            int vitality = 0;

            foreach (Slot slot in equipmentSlots) {
                if (!slot.IsEmpty) {
                    Equipment e = slot.CurrentItem.Item as Equipment;
                    strength += e.Strength;
                    agility += e.Agility;
                    intellect += e.Intellect;
                    vitality += e.Vitality;
                }
            }
            PlayerHandleItem.Instance.SetStats(strength, agility, intellect, vitality);
        }

        public override void SaveInventory() {
            string content = string.Empty;
            for (int i = 0; i < equipmentSlots.Length; i++) {
                if (!equipmentSlots[i].IsEmpty) {
                    content += i + "{0}" + equipmentSlots[i].Items.Peek().Item.ItemName + "{1}";
                }
            }
            PlayerPrefs.SetString("CharPanel", content);
            PlayerPrefs.Save();
        }
        public override void LoadInventory() {
            foreach (Slot slot in equipmentSlots) {
                slot.ClearSlot();
            }
            string content = PlayerPrefs.GetString("CharPanel");
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
                equipmentSlots[index].AddItem(loadedItem.GetComponent<ItemScript>());
                Destroy(loadedItem);
                CalcStats();
            }
        }
    }
}