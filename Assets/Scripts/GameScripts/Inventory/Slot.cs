using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
namespace FantasyRPG.InventorySystem {
    public class Slot : MonoBehaviour, IPointerClickHandler {

        private Stack<ItemScript> items;
        private CanvasGroup canvasGroup;
        private bool clickable = true;
        public ItemType canContain;
        public Text stackText;
        public Sprite slotEmpty;
        public Sprite slotHighlight;

        public Stack<ItemScript> Items {
            get { return items; }
            set { items = value; }
        }

        public bool IsEmpty {
            get { return Items.Count == 0; }
        }

        public bool IsAvailable {
            get { return CurrentItem.Item.MaxSize > Items.Count; }
        }

        public ItemScript CurrentItem {
            get { return Items.Peek(); }
        }

        public bool Clickable {
            get { return clickable; }
            set { clickable = value; }
        }

        void Awake() {
            Items = new Stack<ItemScript>();
        }

        void Start() {
            RectTransform slotRect = GetComponent<RectTransform>();
            RectTransform txtRect = stackText.GetComponent<RectTransform>();
            int txtScaleFactor = (int)(slotRect.sizeDelta.x * 0.6);
            stackText.resizeTextMaxSize = txtScaleFactor;
            stackText.resizeTextMinSize = txtScaleFactor;

            txtRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slotRect.sizeDelta.x);
            txtRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slotRect.sizeDelta.y);
            if (transform.parent != null) {
                canvasGroup = transform.parent.GetComponent<CanvasGroup>();
                EventTrigger trigger = GetComponentInParent<EventTrigger>();
                if (trigger != null) {
                    EventTrigger.Entry entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.PointerEnter;
                    entry.callback.AddListener((eventData) => { transform.parent.GetComponent<Inventory>().ShowToolTip(gameObject); });
                    trigger.triggers.Add(entry);
                }
                /*
                UNUSED NEWER CODE 
                Transform p = transform.parent;
                while (canvasGroup == null && p != null) {
                    canvasGroup = p.GetComponent<CanvasGroup>();
                    p = p.parent;
                }
                */

            }
        }

        public void AddItem(ItemScript item) {
            if (IsEmpty) {
                transform.parent.GetComponent<Inventory>().EmptySlots--;
            }
            Items.Push(item);
            if (Items.Count > 1) {
                stackText.text = Items.Count.ToString();
            }
            ChangeSprite(item.spriteNeutral, item.spriteHighlighted);
        }

        public void AddItems(Stack<ItemScript> items) {
            this.Items = new Stack<ItemScript>(items);
            stackText.text = items.Count > 1 ? items.Count.ToString() : string.Empty;
            ChangeSprite(CurrentItem.spriteNeutral, CurrentItem.spriteHighlighted);
        }
        private void ChangeSprite(Sprite neutral, Sprite highlight) {
            GetComponent<Image>().sprite = neutral;
            SpriteState st = new SpriteState();
            st.highlightedSprite = highlight;
            st.pressedSprite = neutral;
            GetComponent<Button>().spriteState = st;
        }

        private void UseItem() {
            if (!IsEmpty) {
                if (transform.parent.GetComponent<Inventory>() is VendorInventory) {
                    if (CurrentItem.Item.BuyPrice <= PlayerHandleItem.Instance.Gold && PlayerHandleItem.Instance.inventory.AddItem(CurrentItem)) {
                        PlayerHandleItem.Instance.Gold -= CurrentItem.Item.BuyPrice;
                    }
                } else if (VendorInventory.Instance.IsOpen) {
                    PlayerHandleItem.Instance.Gold += CurrentItem.Item.SellPrice;
                    RemoveItem();
                } else if (clickable) {
                    Items.Peek().Use(this);
                    stackText.text = Items.Count > 1 ? Items.Count.ToString() : string.Empty;
                    if (IsEmpty) {
                        ChangeSprite(slotEmpty, slotHighlight);
                        transform.parent.GetComponent<Inventory>().EmptySlots++;
                    }
                }
            }
        }

        public Stack<ItemScript> RemoveItems(int amount) {
            Stack<ItemScript> tmp = new Stack<ItemScript>();
            for (int i = 0; i < amount; i++) {
                tmp.Push(items.Pop());
            }
            stackText.text = items.Count > 1 ? items.Count.ToString() : string.Empty;
            return tmp;
        }

        public ItemScript RemoveItem() {
            if (!IsEmpty) {
                ItemScript tmp = items.Pop();
                stackText.text = items.Count > 1 ? items.Count.ToString() : string.Empty;
                if (IsEmpty) {
                    ClearSlot();
                }
                return tmp;
            }
            return null;
        }

        public void ClearSlot() {
            items.Clear();
            ChangeSprite(slotEmpty, slotHighlight);
            stackText.text = string.Empty;
            if (transform.parent != null) {
                transform.parent.GetComponent<Inventory>().EmptySlots++;
            }

        }

        public void OnPointerClick(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Right && !GameObject.Find("Hover") && canvasGroup != null && canvasGroup.alpha > 0) { //Right
                UseItem();
            } else if (eventData.button == PointerEventData.InputButton.Left && Input.GetKey(KeyCode.LeftShift) && !IsEmpty && !GameObject.Find("Hover") && transform.parent.GetComponent<Inventory>().IsOpen) {
                Vector2 position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(InventoryManager.Instance.canvas.transform as RectTransform, Input.mousePosition, InventoryManager.Instance.canvas.worldCamera, out position);
                InventoryManager.Instance.selectStackSize.SetActive(true);
                InventoryManager.Instance.selectStackSize.transform.position = InventoryManager.Instance.canvas.transform.TransformPoint(position);
                InventoryManager.Instance.SetStackInfo(items.Count);
            }
        }

        public static void SwapItems(Slot from, Slot to) {
            if (to != null && from != null) {
                bool calcStats = from.transform.parent == CharacterPanel.Instance.transform || to.transform.parent == CharacterPanel.Instance.transform;
                if (canSwap(from, to)) {
                    Stack<ItemScript> tmpTo = new Stack<ItemScript>(to.Items);
                    to.AddItems(from.Items);
                    if (tmpTo.Count == 0) {
                        to.transform.parent.GetComponent<Inventory>().EmptySlots--;
                        from.ClearSlot();
                    } else {
                        from.AddItems(tmpTo);
                    }
                }
                if (calcStats) {
                    CharacterPanel.Instance.CalcStats();
                }
            }
        }
        private static bool canSwap(Slot from, Slot to) {
            ItemType fromType = from.CurrentItem.Item.ItemType;
            if (to.canContain == from.canContain) {
                return true;
            }
            if (fromType != ItemType.OFFHAND && to.canContain == fromType) {
                return true;
            }
            if (to.canContain == ItemType.GENERIC && (to.IsEmpty || to.CurrentItem.Item.ItemType == fromType)) {
                return true;
            }
            if (fromType == ItemType.MAINHAND && to.canContain == ItemType.GENERICWEAPON) {
                return true;
            }
            if (fromType == ItemType.TWOHAND && to.canContain == ItemType.GENERICWEAPON && CharacterPanel.Instance.OffHandSlot.IsEmpty) {
                return true;
            }
            if (fromType == ItemType.OFFHAND && (to.IsEmpty || to.CurrentItem.Item.ItemType == ItemType.OFFHAND) && (CharacterPanel.Instance.MainHandSlot.IsEmpty || CharacterPanel.Instance.MainHandSlot.CurrentItem.Item.ItemType != ItemType.TWOHAND)) {
                return true;
            }
            return false;
        }
    }
}