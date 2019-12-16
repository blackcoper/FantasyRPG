using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
namespace FantasyRPG.InventorySystem {
    public class InventoryLink : MonoBehaviour {
        public ChestInventory linkedInventory;
        public int rows, slots;
        private List<Stack<ItemScript>> allSlots;
        private bool active = false;
        void Start() {
            allSlots = new List<Stack<ItemScript>>(slots);
        }

        private void OnTriggerEnter(Collider other) {
            if (other.tag == "Player") {
                if (linkedInventory.FadingOut) {
                    linkedInventory.InstantClose = true;
                    linkedInventory.MoveItemsToChest();
                }
                active = true;
                linkedInventory.UpdateLayout(allSlots, rows, slots);
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.tag == "Player") {
                active = false;
            }
        }

        public void SaveInventory() {
            string content = string.Empty;
            for (int i = 0; i < allSlots.Count; i++) {
                if (allSlots[i] != null && allSlots[i].Count > 0) {
                    content += i + "{0}" + allSlots[i].Peek().Item.ItemName + "{0}" + allSlots[i].Count.ToString() + "{1}";
                }
            }
            PlayerPrefs.SetString(gameObject.name + "content", content);
            PlayerPrefs.Save();
        }

        public virtual void LoadInventory() {
            string content = PlayerPrefs.GetString(gameObject.name + "content");
            allSlots = new List<Stack<ItemScript>>();
            for (int i = 0; i < slots; i++) {
                allSlots.Add(new Stack<ItemScript>());
            }
            if (content != string.Empty) {
                string[] splitContent = content.Split(new string[] { "{1}" }, StringSplitOptions.None);
                for (int x = 0; x < splitContent.Length - 1; x++) {
                    string[] splitValues = splitContent[x].Split(new string[] { "{0}" }, StringSplitOptions.None);
                    int index = Int32.Parse(splitValues[0]);
                    string itemName = splitValues[1];
                    int amount = Int32.Parse(splitValues[2]);
                    Item tmp = null;
                    for (int i = 0; i < amount; i++) {
                        GameObject loadedItem = Instantiate(InventoryManager.Instance.itemObject);
                        if (tmp == null) {
                            tmp = InventoryManager.Instance.ItemContainer.Consumables.Find(item => item.ItemName == itemName);
                        }
                        if (tmp == null) {
                            tmp = InventoryManager.Instance.ItemContainer.Equipment.Find(item => item.ItemName == itemName);
                        }
                        if (tmp == null) {
                            tmp = InventoryManager.Instance.ItemContainer.Weapons.Find(item => item.ItemName == itemName);
                        }
                        if (tmp == null) {
                            tmp = InventoryManager.Instance.ItemContainer.Materials.Find(item => item.ItemName == itemName);
                        }
                        loadedItem.AddComponent<ItemScript>();
                        loadedItem.GetComponent<ItemScript>().Item = tmp;
                        allSlots[index].Push(loadedItem.GetComponent<ItemScript>());
                        Destroy(loadedItem);
                    }
                }
            }
            if (active) {
                linkedInventory.UpdateLayout(allSlots, rows, slots);
            }
        }
    }
}