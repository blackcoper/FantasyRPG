using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace FantasyRPG.InventorySystem {
    public class PlayerHandleItem : MonoBehaviour {
        private static PlayerHandleItem instance;

        public Inventory inventory;
        public Inventory charPanel;
        public Text helperText;
        public Text statsText;
        public int baseStrength;
        public int baseAgility;
        public int baseIntellect;
        public int baseVitality;

        private Inventory chest;
        [SerializeField]
        private CharacterStats health;
        [SerializeField]
        private CharacterStats mana;
        private int strength;
        private int agility;
        private int intellect;
        private int vitality;
        private int gold;
        [SerializeField]
        private Text goldText;
        public static PlayerHandleItem Instance {
            get {
                if (instance == null) {
                    instance = GameObject.FindObjectOfType<PlayerHandleItem>();
                }
                return instance;
            }
        }

        public int Gold {
            get { return gold; }
            set {
                goldText.text = "Gold: <color=orange>" + value + "</color>";
                gold = value;
            }
        }

        void Awake() {
            health.Initialize();
            mana.Initialize();
        }

        void Start() {
            gold = 0;
            SetStats(0, 0, 0, 0);
        }

        void Update() {
            if (Input.GetKey(KeyCode.Alpha1)) {
                health.CurrentVal -= 50;
            }
            if (Input.GetKey(KeyCode.Alpha2)) {
                health.CurrentVal += 50;
            }
            if (Input.GetKey(KeyCode.Alpha3)) {
                mana.CurrentVal -= 50;
            }
            if (Input.GetKey(KeyCode.Alpha4)) {
                mana.CurrentVal += 50;
            }
            if (Input.GetKeyDown(KeyCode.B)) {
                inventory.Open();
            }
            if (Input.GetKeyDown(KeyCode.B)) {
                inventory.Open();
            }
            if (Input.GetKeyDown(KeyCode.E)) {
                if (chest != null) {
                    if (chest.canvasGroup.alpha == 0 || chest.canvasGroup.alpha == 1) {
                        chest.Open();
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.C)) {
                if (charPanel != null) {
                    charPanel.Open();
                }
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.tag == "Item") {
                int randomType = UnityEngine.Random.Range(0, 3);
                GameObject tmp = Instantiate(InventoryManager.Instance.itemObject);
                int randomItem;
                tmp.AddComponent<ItemScript>();
                ItemScript newItem = tmp.GetComponent<ItemScript>();
                switch (randomType) {
                    case 0:
                        randomItem = UnityEngine.Random.Range(0, InventoryManager.Instance.ItemContainer.Consumables.Count);
                        newItem.Item = InventoryManager.Instance.ItemContainer.Consumables[randomItem];
                        break;
                    case 1:
                        randomItem = UnityEngine.Random.Range(0, InventoryManager.Instance.ItemContainer.Weapons.Count);
                        newItem.Item = InventoryManager.Instance.ItemContainer.Weapons[randomItem];
                        break;
                    case 2:
                        randomItem = UnityEngine.Random.Range(0, InventoryManager.Instance.ItemContainer.Equipment.Count);
                        newItem.Item = InventoryManager.Instance.ItemContainer.Equipment[randomItem];
                        break;
                }
                inventory.AddItem(newItem);
                Destroy(tmp);
            }
            if (other.tag == "Chest" || other.tag == "Vendor") {
                helperText.gameObject.SetActive(true);
                chest = other.GetComponent<InventoryLink>().linkedInventory;
            }
            if (other.tag == "CraftingBench") {
                helperText.gameObject.SetActive(true);
                chest = other.GetComponent<CraftingBenchScript>().craftingBench;
            }
            if (other.tag == "Material") {
                for (int i = 0; i < 5; i++) {
                    for (int x = 0; x < 3; x++) {
                        GameObject tmp = Instantiate(InventoryManager.Instance.itemObject) as GameObject;
                        tmp.AddComponent<ItemScript>();
                        ItemScript newMaterial = tmp.GetComponent<ItemScript>();
                        newMaterial.Item = InventoryManager.Instance.ItemContainer.Materials[x];
                        inventory.AddItem(newMaterial);
                        Destroy(tmp);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.tag == "Chest" || other.tag == "CraftingBench" || other.tag == "Vendor") {
                helperText.gameObject.SetActive(false);
                if (chest != null) {
                    if (chest.IsOpen) {
                        chest.Open();
                    }
                    chest = null;
                }
            }
        }

        private void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.tag == "Item") {
                if (inventory.AddItem(collision.gameObject.GetComponent<ItemScript>())) {
                    Destroy(collision.gameObject);
                }
            }
        }

        public void SetStats(int strength, int agility, int intellect, int vitality) {
            this.strength = strength + baseStrength;
            this.agility = agility + baseAgility;
            this.intellect = intellect + baseIntellect;
            this.vitality = vitality + baseVitality;

            statsText.text = string.Format("Strength: {0}\nAgility: {1}\nIntellect: {2}\nVitality: {3}", this.strength, this.agility, this.intellect, this.vitality);
        }
    }
}