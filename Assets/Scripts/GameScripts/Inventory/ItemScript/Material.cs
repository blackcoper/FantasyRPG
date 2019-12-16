using UnityEngine;
using System.Collections;
namespace FantasyRPG.InventorySystem {
    public class Material : Item {

        public Material() { }

        public Material(string itemName, string description, ItemType itemType, Quality quality, string spriteNeutral, string spriteHighlight, int maxSize) : base(itemName, description, itemType, quality, spriteNeutral, spriteHighlight, maxSize) {

        }

        public override void Use(Slot slot, ItemScript item) { }

        public override string GetTooltip(Inventory inv) {
            string itemTip = base.GetTooltip(inv);
            if (inv is VendorInventory) {
                return string.Format("{0}\n<color=orange>Price: {1}</color>", itemTip, BuyPrice);
            } else if (VendorInventory.Instance.IsOpen) {
                return string.Format("{0}\n<color=orange>Price: {1}</color>", itemTip, SellPrice);
            } else {
                return itemTip;
            }
        }
    }
}