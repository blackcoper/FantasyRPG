using UnityEngine;
using System.Collections;
namespace FantasyRPG.InventorySystem {
    public class Equipment : Item {
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Intellect { get; set; }
        //public int Stamina { get; set; }
        public int Vitality { get; set; }

        public Equipment() { }

        public Equipment(string itemName, string description, ItemType itemType, Quality quality, string spriteNeutral, string spriteHighlighted, int maxSize, int strength, int agility, int intellect, int vitality) : base(itemName, description, itemType, quality, spriteNeutral, spriteHighlighted, maxSize) {
            this.Strength = strength;
            this.Agility = agility;
            this.Intellect = intellect;
            this.Vitality = vitality;
        }

        public override void Use(Slot slot, ItemScript item) {
            CharacterPanel.Instance.EquipItem(slot, item);
        }

        public override string GetTooltip(Inventory inv) {
            string stats = string.Empty;
            if (Strength > 0) {
                stats += "\n+" + Strength.ToString() + " Strength";
            }
            if (Agility > 0) {
                stats += "\n+" + Agility.ToString() + " Agility";
            }
            if (Intellect > 0) {
                stats += "\n+" + Intellect.ToString() + " Intellect";
            }
            if (Vitality > 0) {
                stats += "\n+" + Vitality.ToString() + " Vitality";
            }
            string itemTip = base.GetTooltip(inv);
            if (inv is VendorInventory && !(this is Weapon)) {
                return string.Format("{0}<size=14>{1}</size>\n<color=orange>Price: {2}</color>", itemTip, stats, BuyPrice);
            } else if (VendorInventory.Instance.IsOpen && !(this is Weapon)) {
                return string.Format("{0}<size=14>{1}</size>\n<color=orange>Price: {2}</color>", itemTip, stats, SellPrice);
            } else {
                return string.Format("{0}<size=14>{1}</size>", itemTip, stats);
            }
        }
    }
}
