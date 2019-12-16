using UnityEngine;
using System.Collections;
namespace FantasyRPG.InventorySystem {
    public class VendorInventory : ChestInventory {
        private static VendorInventory instance;

        public static VendorInventory Instance {
            get {
                if (instance == null) {
                    instance = GameObject.FindObjectOfType<VendorInventory>();
                }
                return instance;
            }
        }

        protected override void Start() {
            EmptySlots = slots;
            base.Start();
            GiveItem("Lesser Healing Potion");
            GiveItem("Healing Potion");
            GiveItem("Lesser Mana Potion");
            GiveItem("Mana Potion");
        }
        protected void GiveItem(string itemName) {
            GameObject tmp = Instantiate(InventoryManager.Instance.itemObject);
            tmp.AddComponent<ItemScript>();
            ItemScript newItem = tmp.GetComponent<ItemScript>();
            if (InventoryManager.Instance.ItemContainer.Consumables.Exists(x => x.ItemName == itemName)) {
                newItem.Item = InventoryManager.Instance.ItemContainer.Consumables.Find(x => x.ItemName == itemName);
            } else if (InventoryManager.Instance.ItemContainer.Weapons.Exists(x => x.ItemName == itemName)) {
                newItem.Item = InventoryManager.Instance.ItemContainer.Weapons.Find(x => x.ItemName == itemName);
            } else if (InventoryManager.Instance.ItemContainer.Equipment.Exists(x => x.ItemName == itemName)) {
                newItem.Item = InventoryManager.Instance.ItemContainer.Equipment.Find(x => x.ItemName == itemName);
            } else if (InventoryManager.Instance.ItemContainer.Materials.Exists(x => x.ItemName == itemName)) {
                newItem.Item = InventoryManager.Instance.ItemContainer.Materials.Find(x => x.ItemName == itemName);
            }
            if (newItem != null) {
                AddItem(newItem);
            }
            Destroy(tmp);
        }
        public override void MoveItem(GameObject clicked) { }
    }
}