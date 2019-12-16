using UnityEngine;
using System.Collections;
using System;
namespace FantasyRPG.InventorySystem {
    public class Consumeable : Item {
        public int Health { get; set; }
        public int Mana { get; set; }

        public Consumeable() {

        }
        public Consumeable(string itemName, string description, ItemType itemType, Quality quality, string spriteNeutral, string spriteHighlighted, int maxSize, int health, int mana) : base(itemName, description, itemType, quality, spriteNeutral, spriteHighlighted, maxSize) {
            this.Health = health;
            this.Mana = mana;
        }
        public override void Use(Slot slot, ItemScript item) {
            Debug.Log("Used " + ItemName);
            slot.RemoveItem();
        }

        public override string GetTooltip(Inventory inv) {
            string stats = string.Empty;

            if (Health > 0) {
                stats += "\nRestores " + Health.ToString() + " Health";
            }
            if (Mana > 0) {
                stats += "\nRestores " + Mana.ToString() + " Mana";
            }
            string itemTip = base.GetTooltip(inv);
            if (inv is VendorInventory) {
                return string.Format("{0}<size=14>{1}</size>\n<color=orange>Price: {2}</color>", itemTip, stats, BuyPrice);
            } else if (VendorInventory.Instance.IsOpen) {
                return string.Format("{0}<size=14>{1}</size>\n<color=orange>Price: {2}</color>", itemTip, stats, SellPrice);
            } else {
                return string.Format("{0}<size=14>{1}</size>", itemTip, stats);
            }
        }
    }
}