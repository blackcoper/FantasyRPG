using UnityEngine;
using System.Collections;
namespace FantasyRPG.InventorySystem {
    public class Weapon : Equipment {

        public float AttackSpeed { get; set; }

        public Weapon() { }

        public Weapon(string itemName, string description, ItemType itemType, Quality quality, string spriteNeutral, string spriteHighlighted, int maxSize, int strength, int agility, int intellect, int vitality, float attackSpeed) : base(itemName, description, itemType, quality, spriteNeutral, spriteHighlighted, maxSize, strength, agility, intellect, vitality) {
            this.AttackSpeed = attackSpeed;
        }

        public override void Use(Slot slot, ItemScript item) {
            CharacterPanel.Instance.EquipItem(slot, item);
        }

        public override string GetTooltip(Inventory inv) {

            string itemTip = base.GetTooltip(inv);
            if (inv is VendorInventory) {
                return string.Format("{0}<size=14>\nAttack Speed : {1}</size>\n<color=orange>Price: {2}</color>", itemTip, AttackSpeed, BuyPrice);
            } else if (VendorInventory.Instance.IsOpen) {
                return string.Format("{0}<size=14>\nAttack Speed : {1}</size>\n<color=orange>Price: {2}</color>", itemTip, AttackSpeed, SellPrice);
            } else {
                return string.Format("{0}<size=14>\nAttack Speed : {1}</size>", itemTip, AttackSpeed);
            }

        }
    }
}