using UnityEngine;
using System.Collections;
namespace FantasyRPG.InventorySystem {
    public enum ItemType { CONSUMEABLE, MAINHAND, TWOHAND, OFFHAND, HEAD, NECK, CHEST, RING, LEGS, BRACERS, BOOTS, TRINKET, SHOULDERS, BELT, GENERIC, GENERICWEAPON, MATERIAL };
    public enum Quality { COMMON, UNCOMMON, RARE, EPIC, LEGENDARY, ARTIFACT };

    public class ItemScript : MonoBehaviour {
        public Sprite spriteNeutral;
        public Sprite spriteHighlighted;

        private Item item;

        public Item Item {
            get { return item; }
            set {
                if (value != null) {
                    item = value;
                    spriteNeutral = Resources.Load<Sprite>(value.SpriteNeutral);
                    spriteHighlighted = Resources.Load<Sprite>(value.SpriteHighlighted);
                }
            }
        }

        public void Use(Slot slot) {
            item.Use(slot, this);
        }

        public string GetTooltip(Inventory inv) {
            return item.GetTooltip(inv);
        }

        //public void SetStats(ItemScript item) {
        //    this.type = item.type;
        //    this.quality = item.quality;
        //    this.spriteNeutral = item.spriteNeutral;
        //    this.spriteHighlighted = item.spriteHighlighted;
        //    this.maxSize = item.maxSize;
        //    this.strength = item.strength;
        //    this.intellect = item.intellect;
        //    this.vitality = item.vitality;
        //    this.stamina = item.stamina;
        //    this.itemName = item.itemName;
        //    this.description = item.description;
        //    switch (type) {
        //        //case ItemType.MANA:
        //        //    GetComponent<Renderer>().material.color = Color.blue;
        //        //    break;
        //        //case ItemType.HEALTH:
        //        //    GetComponent<Renderer>().material.color = Color.red;
        //        //    break;
        //        //case ItemType.WEAPON:
        //        //    GetComponent<Renderer>().material.color = Color.green;
        //        //    break;
        //    }
        //}
    }
}