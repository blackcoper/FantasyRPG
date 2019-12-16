using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
namespace FantasyRPG.InventorySystem {
    public class InventoryManager : MonoBehaviour {

        #region Field
        private static InventoryManager instance;

        public static InventoryManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<InventoryManager>();
                }
                return instance;
            }
        }

        public GameObject slotPrefab;
        public GameObject iconPrefab;
        public GameObject itemObject;
        public GameObject dropItem;
        public GameObject tooltipObject;
        public GameObject selectStackSize;
        public Text sizeTextObject;
        public Text visualTextObject;
        public Canvas canvas;
        public Text stackText;
        public EventSystem eventSystem;

        private Slot from, to;
        private GameObject clicked;
        private GameObject hoverObject;
        private int splitAmount;
        private int maxStackCount;
        private Slot movingSlot;
        private ItemContainer itemContainer = new ItemContainer();
        #endregion

        #region Propertoes
        public Slot From {
            get { return from; }
            set { from = value; }
        }

        public Slot To {
            get { return to; }
            set { to = value; }
        }

        public GameObject Clicked {
            get { return clicked; }
            set { clicked = value; }
        }

        public int SplitAmount {
            get { return splitAmount; }
            set { splitAmount = value; }
        }

        public Slot MovingSlot {
            get { return movingSlot; }
            set { movingSlot = value; }
        }

        public GameObject HoverObject {
            get { return hoverObject; }
            set { hoverObject = value; }
        }

        public int MaxStackCount {
            get { return maxStackCount; }
            set { maxStackCount = value; }
        }

        public ItemContainer ItemContainer {
            get { return itemContainer; }
            set { itemContainer = value; }
        }
        #endregion

        public void Awake() {
            /*
            ===============================
            USING StreamingAssets/Items.xml
            ===============================
            */
            Type[] itemTypes = { typeof(Equipment), typeof(Weapon), typeof(Consumeable), typeof(Material) };
            XmlSerializer serializer = new XmlSerializer(typeof(ItemContainer), itemTypes);
            TextReader textReader = new StreamReader(Application.streamingAssetsPath + "/Items.xml");
            ItemContainer = serializer.Deserialize(textReader) as ItemContainer;
            textReader.Close();
            CraftingBench.Instance.CreateBlueprints();
            /*
            ================================
            USING Resources/Items.xml
            ================================
            XmlDocument doc = new XmlDocument();
            TextAsset myXmlAsset = Resources.Load<TextAsset>("Items");
            doc.LoadXml(myXmlAsset.text);
            Type[] itemTypes = { typeof(Equipment), typeof(Weapon), typeof(Consumeable), typeof(Material)};
            XmlSerializer serializer = new XmlSerializer(typeof(ItemContainer), itemTypes);
            TextReader textReader = new StreamReader(doc.InnerXml);
            ItemContainer = serializer.Deserialize(textReader) as ItemContainer;
            textReader.Close();
            */

        }

        public void SetStackInfo(int maxStackCount) {
            selectStackSize.SetActive(true);
            tooltipObject.SetActive(false);
            SplitAmount = 0;
            this.MaxStackCount = maxStackCount;
            stackText.text = SplitAmount.ToString();
        }
        public void Save() {
            GameObject[] inventories = GameObject.FindGameObjectsWithTag("Inventory");
            GameObject[] chests = GameObject.FindGameObjectsWithTag("Chest");
            foreach (GameObject inventory in inventories) {
                inventory.GetComponent<Inventory>().SaveInventory();
            }
            foreach (GameObject chest in chests) {
                chest.GetComponent<InventoryLink>().SaveInventory();
            }
        }
        public void Load() {
            GameObject[] inventories = GameObject.FindGameObjectsWithTag("Inventory");
            GameObject[] chests = GameObject.FindGameObjectsWithTag("Chest");
            foreach (GameObject inventory in inventories) {
                inventory.GetComponent<Inventory>().LoadInventory();
            }
            foreach (GameObject chest in chests) {
                chest.GetComponent<InventoryLink>().LoadInventory();
            }
        }
    }
}