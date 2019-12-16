using UnityEngine;
using System.Collections;
using System;
using System.Xml.Serialization;
using System.IO;
namespace FantasyRPG.InventorySystem {
    public enum Category { EQUIPMENT, WEAPON, CONSUMABLE }

    public class ItemManager : MonoBehaviour {

        public ItemType itemType;
        public Quality quality;
        public Category category;
        public string spriteNeutral;
        public string spriteHighlighted;
        public string itemName;
        public string description;
        public int maxSize;
        public int strength;
        public int agility;
        public int intellect;
        public int vitality;
        public float attackSpeed;
        public int health;
        public int mana;

        public void CreateItem() {
            ItemContainer itemContainer = new ItemContainer();
            Type[] itemTypes = { typeof(Equipment), typeof(Weapon), typeof(Consumeable) };
            XmlSerializer serializer = new XmlSerializer(typeof(ItemContainer), itemTypes);
            FileStream fs = new FileStream(Path.Combine(Application.streamingAssetsPath, "Items.xml"), FileMode.Open);

            itemContainer = serializer.Deserialize(fs) as ItemContainer;

            serializer.Serialize(fs, itemContainer);

            fs.Close();

            switch (category) {
                case Category.EQUIPMENT:
                    itemContainer.Equipment.Add(new Equipment(itemName, description, itemType, quality, spriteNeutral, spriteHighlighted, maxSize, strength, agility, intellect, vitality));
                    break;
                case Category.WEAPON:
                    itemContainer.Weapons.Add(new Weapon(itemName, description, itemType, quality, spriteNeutral, spriteHighlighted, maxSize, strength, agility, intellect, vitality, attackSpeed));
                    break;
                case Category.CONSUMABLE:
                    itemContainer.Consumables.Add(new Consumeable(itemName, description, itemType, quality, spriteNeutral, spriteHighlighted, maxSize, health, mana));
                    break;
            }

            fs = new FileStream(Path.Combine(Application.streamingAssetsPath, "Items.xml"), FileMode.Create);
            serializer.Serialize(fs, itemContainer);
            fs.Close();
        }
        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }
}