using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace FantasyRPG.InventorySystem {
    public class ItemContainer {
        private List<Item> weapons = new List<Item>();
        private List<Item> equipment = new List<Item>();
        private List<Item> consumables = new List<Item>();
        private List<Item> materials = new List<Item>();

        public List<Item> Weapons {
            get { return weapons; }
            set { weapons = value; }
        }

        public List<Item> Equipment {
            get { return equipment; }
            set { equipment = value; }
        }

        public List<Item> Consumables {
            get { return consumables; }
            set { consumables = value; }
        }

        public List<Item> Materials {
            get { return materials; }
            set { materials = value; }
        }

        public ItemContainer() { }
    }
}