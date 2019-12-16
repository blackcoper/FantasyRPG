using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
namespace FantasyRPG.InventorySystem {
    public class Inventory : MonoBehaviour {

        public CanvasGroup canvasGroup;
        public int rows;
        public int slots;
        public float marginLeft, marginTop;
        public float slotPaddingLeft, slotPaddingTop;
        public float slotSize;
        public float fadeTime;
        public static bool mouseInside = false;


        private int emptySlots;
        private bool isOpen;
        private bool fadingIn;
        private bool fadingOut;
        private bool instantClose = false;
        private Vector2 dragOffset;
        protected float hoverYOffset;
        protected RectTransform inventoryRect;
        protected float inventoryWidth, inventoryHeight;
        protected List<GameObject> allSlots;

        protected static GameObject playerRef;
        public int EmptySlots {
            get { return emptySlots; }
            set { emptySlots = value; }
        }

        public bool IsOpen {
            get { return isOpen; }
            set { isOpen = value; }
        }

        public bool FadingOut {
            get { return fadingOut; }
        }

        public bool InstantClose {
            get { return instantClose; }
            set { instantClose = value; }
        }

        protected virtual void Start() {
            isOpen = false;
            playerRef = GameObject.FindGameObjectWithTag("Player");
            CreateLayout();
            InventoryManager.Instance.MovingSlot = GameObject.Find("MovingSlot").GetComponent<Slot>();
            InventoryManager.Instance.selectStackSize.transform.SetAsLastSibling();
        }

        void Update() {
            if (Input.GetMouseButtonUp(0)) {
                if (!mouseInside && InventoryManager.Instance.From != null) {
                    InventoryManager.Instance.From.GetComponent<Image>().color = Color.white;
                    foreach (ItemScript item in InventoryManager.Instance.From.Items) {
                        float angle = UnityEngine.Random.Range(0, Mathf.PI * 2); //
                        Vector3 v = new Vector3(Mathf.Sin(angle), -0.5f, Mathf.Cos(angle));
                        v *= 1.5f;
                        GameObject tmpDrp = GameObject.Instantiate(InventoryManager.Instance.dropItem, playerRef.transform.position - v, Quaternion.identity) as GameObject; //

                        tmpDrp.AddComponent<ItemScript>();
                        tmpDrp.GetComponent<ItemScript>().Item = item.Item;
                    }
                    InventoryManager.Instance.From.ClearSlot();
                    if (InventoryManager.Instance.From.transform.parent == CharacterPanel.Instance.transform) {
                        CharacterPanel.Instance.CalcStats();
                    }
                    Destroy(GameObject.Find("Hover"));
                    InventoryManager.Instance.To = null;
                    InventoryManager.Instance.From = null;
                } else if (!InventoryManager.Instance.eventSystem.IsPointerOverGameObject(-1) && !InventoryManager.Instance.MovingSlot.IsEmpty) {
                    foreach (ItemScript item in InventoryManager.Instance.MovingSlot.Items) {
                        float angle = UnityEngine.Random.Range(0.0f, Mathf.PI * 2);
                        Vector3 v = new Vector3(Mathf.Sin(angle), -0.5f, Mathf.Cos(angle));
                        v *= 1.5f;
                        GameObject tmpDrp = GameObject.Instantiate(InventoryManager.Instance.dropItem, playerRef.transform.position - v, Quaternion.identity) as GameObject;

                        tmpDrp.AddComponent<ItemScript>();
                        tmpDrp.GetComponent<ItemScript>().Item = item.Item;
                    }
                    InventoryManager.Instance.MovingSlot.ClearSlot();
                    Destroy(GameObject.Find("Hover"));
                }
            }
            if (InventoryManager.Instance.HoverObject != null) {
                Vector2 position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(InventoryManager.Instance.canvas.transform as RectTransform, Input.mousePosition, InventoryManager.Instance.canvas.worldCamera, out position);
                position.Set(position.x, position.y - hoverYOffset);
                InventoryManager.Instance.HoverObject.transform.position = InventoryManager.Instance.canvas.transform.TransformPoint(position);
            }
            //if (Input.GetKeyDown(KeyCode.R)) {
            //    PlayerPrefs.DeleteAll();
            //}
        }

        public void OnPointerDown() {
            Vector2 mousePos = Input.mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(InventoryManager.Instance.canvas.transform as RectTransform, new Vector3(Input.mousePosition.x, Input.mousePosition.y), InventoryManager.Instance.canvas.worldCamera, out mousePos);
            dragOffset = (Vector2)inventoryRect.localPosition - mousePos;
        }

        public void OnPointerUp() {
            dragOffset = Vector2.zero;
        }

        public void OnDrag(Transform position) {
            if (isOpen) {
                MoveInventory(position);
            }
        }

        public void PointerExit() {
            mouseInside = false;
        }

        public void PointerEnter() {
            if (canvasGroup != null) {
                if (canvasGroup.alpha > 0) {
                    mouseInside = true;
                }
            }
        }

        public virtual void Open() {
            if (canvasGroup.alpha > 0) {
                StartCoroutine("FadeOut");
                PutItemBack();
                HideToolTip();
                isOpen = false;
                canvasGroup.blocksRaycasts = false;
            } else {
                StartCoroutine("FadeIn");
                isOpen = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public virtual void ShowToolTip(GameObject slot) {
            Slot tmpSlot = slot.GetComponent<Slot>();
            if (slot.GetComponentInParent<Inventory>().isOpen && !tmpSlot.IsEmpty && InventoryManager.Instance.HoverObject == null && !InventoryManager.Instance.selectStackSize.activeSelf) {
                InventoryManager.Instance.visualTextObject.text = tmpSlot.CurrentItem.GetTooltip(this);
                InventoryManager.Instance.sizeTextObject.text = InventoryManager.Instance.visualTextObject.text;
                InventoryManager.Instance.tooltipObject.SetActive(true);
                float xPos = slot.transform.position.x + slotPaddingLeft;
                float yPos = slot.transform.position.y - slot.GetComponent<RectTransform>().sizeDelta.y - slotPaddingTop;
                InventoryManager.Instance.tooltipObject.transform.position = new Vector2(xPos, yPos);
            }
        }

        public void HideToolTip() {
            InventoryManager.Instance.tooltipObject.SetActive(false);
        }

        public virtual void SaveInventory() {
            string content = string.Empty;
            for (int i = 0; i < allSlots.Count; i++) {
                Slot tmp = allSlots[i].GetComponent<Slot>();
                if (!tmp.IsEmpty) {
                    content += i + "{0}" + tmp.CurrentItem.Item.ItemName.ToString() + "{0}" + tmp.Items.Count.ToString() + "{1}";
                }
            }
            PlayerPrefs.SetString(gameObject.name + "content", content);
            PlayerPrefs.SetInt(gameObject.name + "slots", slots);
            PlayerPrefs.SetInt(gameObject.name + "rows", rows);
            PlayerPrefs.SetFloat(gameObject.name + "marginLeft", marginLeft);
            PlayerPrefs.SetFloat(gameObject.name + "marginTop", marginTop);
            PlayerPrefs.SetFloat(gameObject.name + "slotPaddingLeft", slotPaddingLeft);
            PlayerPrefs.SetFloat(gameObject.name + "slotPaddingTop", slotPaddingTop);
            PlayerPrefs.SetFloat(gameObject.name + "slotSize", slotSize);
            PlayerPrefs.SetFloat(gameObject.name + "xPos", inventoryRect.position.x);
            PlayerPrefs.SetFloat(gameObject.name + "yPos", inventoryRect.position.y);
            PlayerPrefs.Save();
        }

        public virtual void LoadInventory() {
            string content = PlayerPrefs.GetString(gameObject.name + "content");
            slots = PlayerPrefs.GetInt(gameObject.name + "slots");
            rows = PlayerPrefs.GetInt(gameObject.name + "rows");
            marginLeft = PlayerPrefs.GetFloat(gameObject.name + "marginLeft");
            marginTop = PlayerPrefs.GetFloat(gameObject.name + "marginTop");
            slotPaddingLeft = PlayerPrefs.GetFloat(gameObject.name + "slotPaddingLeft");
            slotPaddingTop = PlayerPrefs.GetFloat(gameObject.name + "slotPaddingTop");
            slotSize = PlayerPrefs.GetFloat(gameObject.name + "slotSize");

            inventoryRect.position = new Vector3(PlayerPrefs.GetFloat(gameObject.name + "xPos"), PlayerPrefs.GetFloat(gameObject.name + "yPos"), inventoryRect.position.z);

            CreateLayout();

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
                    allSlots[index].GetComponent<Slot>().AddItem(loadedItem.GetComponent<ItemScript>());
                    Destroy(loadedItem);
                }
            }
        }

        public virtual void CreateLayout() {
            if (allSlots != null) {
                foreach (GameObject go in allSlots) {
                    Destroy(go);
                }
            }
            allSlots = new List<GameObject>();
            hoverYOffset = slotSize * 0.1f;
            EmptySlots = slots;
            inventoryWidth = (slots / rows) * (slotSize + slotPaddingLeft) + slotPaddingLeft;
            inventoryHeight = rows * (slotSize + slotPaddingTop) + slotPaddingTop;
            inventoryRect = GetComponent<RectTransform>();
            inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, inventoryWidth + marginLeft);
            inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, inventoryHeight + marginTop);

            int columns = slots / rows;
            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < columns; x++) {
                    GameObject newSlot = Instantiate(InventoryManager.Instance.slotPrefab) as GameObject;
                    RectTransform slotRect = newSlot.GetComponent<RectTransform>();
                    newSlot.name = "Slot";
                    newSlot.transform.SetParent(this.transform.parent);
                    slotRect.localPosition = inventoryRect.localPosition + new Vector3((slotPaddingLeft * (x + 1) + (slotSize * x)) + marginLeft, (-slotPaddingTop * (y + 1) - (slotSize * y)) - marginTop);
                    slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slotSize * InventoryManager.Instance.canvas.scaleFactor);
                    slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slotSize * InventoryManager.Instance.canvas.scaleFactor);
                    newSlot.transform.SetParent(this.transform);
                    allSlots.Add(newSlot);
                    newSlot.GetComponent<Button>().onClick.AddListener(delegate { MoveItem(newSlot); });
                }
            }
        }

        public bool AddItem(ItemScript item) {
            if (item.Item.MaxSize == 1) {
                return PlaceEmpty(item);
            } else {
                foreach (GameObject slot in allSlots) {
                    Slot tmp = slot.GetComponent<Slot>();
                    if (!tmp.IsEmpty) {
                        if (tmp.CurrentItem.Item.ItemName == item.Item.ItemName && tmp.IsAvailable) {
                            if (!InventoryManager.Instance.MovingSlot.IsEmpty && InventoryManager.Instance.Clicked.GetComponent<Slot>() == tmp.GetComponent<Slot>()) {
                                continue;
                            } else {
                                tmp.AddItem(item);
                                return true;
                            }
                        }
                    }
                }
                if (EmptySlots > 0) {
                    return PlaceEmpty(item);
                }
            }
            return false;
        }

        private void MoveInventory(Transform _transform) {
            Vector2 screenPos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(InventoryManager.Instance.canvas.transform as RectTransform, new Vector3(Input.mousePosition.x + dragOffset.x * InventoryManager.Instance.canvas.scaleFactor, Input.mousePosition.y + dragOffset.y * InventoryManager.Instance.canvas.scaleFactor), InventoryManager.Instance.canvas.worldCamera, out screenPos);
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(InventoryManager.Instance.canvas.transform as RectTransform, new Vector3(Input.mousePosition.x - (inventoryRect.sizeDelta.x / 2 * InventoryManager.Instance.canvas.scaleFactor), Input.mousePosition.y + (inventoryRect.sizeDelta.y / 2 * InventoryManager.Instance.canvas.scaleFactor)), InventoryManager.Instance.canvas.worldCamera, out mousePos);
            _transform.position = InventoryManager.Instance.canvas.transform.TransformPoint(screenPos);
        }

        private bool PlaceEmpty(ItemScript item) {
            if (EmptySlots > 0) {
                foreach (GameObject slot in allSlots) {
                    Slot tmp = slot.GetComponent<Slot>();
                    if (tmp.IsEmpty) {
                        tmp.AddItem(item);
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void MoveItem(GameObject clicked) {
            if (isOpen) {
                CanvasGroup cg = clicked.transform.parent.GetComponent<CanvasGroup>();
                if (cg != null && cg.alpha > 0 || clicked.transform.parent.parent.GetComponent<CanvasGroup>().alpha > 0) {
                    InventoryManager.Instance.Clicked = clicked;
                    if (!InventoryManager.Instance.MovingSlot.IsEmpty) {
                        Slot tmp = clicked.GetComponent<Slot>();
                        if (tmp.IsEmpty) {
                            tmp.AddItems(InventoryManager.Instance.MovingSlot.Items);
                            InventoryManager.Instance.MovingSlot.Items.Clear();
                            Destroy(GameObject.Find("Hover"));
                        } else if (!tmp.IsEmpty && InventoryManager.Instance.MovingSlot.CurrentItem.Item.ItemName == tmp.CurrentItem.Item.ItemName && tmp.IsAvailable) {
                            MergeStacks(InventoryManager.Instance.MovingSlot, tmp);
                        }
                    } else if (InventoryManager.Instance.From == null && clicked.transform.parent.GetComponent<Inventory>().isOpen && !Input.GetKey(KeyCode.LeftShift)) {
                        if (!clicked.GetComponent<Slot>().IsEmpty && !GameObject.Find("Hover")) {
                            InventoryManager.Instance.From = clicked.GetComponent<Slot>();
                            InventoryManager.Instance.From.GetComponent<Image>().color = Color.gray;
                            CreateHoverIcon();
                        }
                    } else if (InventoryManager.Instance.To == null && !Input.GetKey(KeyCode.LeftShift)) {
                        InventoryManager.Instance.To = clicked.GetComponent<Slot>();
                        Destroy(GameObject.Find("Hover"));
                    }
                    if (InventoryManager.Instance.To != null && InventoryManager.Instance.From != null) {
                        if (!InventoryManager.Instance.To.IsEmpty && InventoryManager.Instance.From.CurrentItem.Item.ItemName == InventoryManager.Instance.To.CurrentItem.Item.ItemName && InventoryManager.Instance.To.IsAvailable) {
                            MergeStacks(InventoryManager.Instance.From, InventoryManager.Instance.To);
                        } else {
                            Slot.SwapItems(InventoryManager.Instance.From, InventoryManager.Instance.To);
                        }
                        InventoryManager.Instance.From.GetComponent<Image>().color = Color.white;
                        InventoryManager.Instance.From = null;
                        InventoryManager.Instance.To = null;
                        Destroy(GameObject.Find("Hover"));
                    }
                }
                if (CraftingBench.Instance.isOpen) {
                    CraftingBench.Instance.updatePreview();
                }
            }
        }
        protected virtual IEnumerator FadeOut() {
            if (!FadingOut) {
                fadingOut = true;
                fadingIn = false;
                StopCoroutine("FadeIn");
                float startAlpha = canvasGroup.alpha;
                float rate = 1.0f / fadeTime;
                float progress = 0.0f;
                while (progress < 1.0) {
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, progress);
                    progress += rate * Time.deltaTime;
                    if (InstantClose) {
                        break;
                    }
                    yield return null;
                }
                canvasGroup.alpha = 0;
                InstantClose = false;
                fadingOut = false;
            }
        }

        private void CreateHoverIcon() {
            InventoryManager.Instance.HoverObject = Instantiate(InventoryManager.Instance.iconPrefab) as GameObject;
            InventoryManager.Instance.HoverObject.GetComponent<Image>().sprite = InventoryManager.Instance.Clicked.GetComponent<Image>().sprite;
            InventoryManager.Instance.HoverObject.GetComponent<Image>().raycastTarget = false;
            InventoryManager.Instance.HoverObject.transform.GetChild(0).GetComponent<Text>().raycastTarget = false;
            InventoryManager.Instance.HoverObject.name = "Hover";
            RectTransform hoverTransform = InventoryManager.Instance.HoverObject.GetComponent<RectTransform>();
            RectTransform clickedTransform = InventoryManager.Instance.Clicked.GetComponent<RectTransform>();

            hoverTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, clickedTransform.sizeDelta.x);
            hoverTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, clickedTransform.sizeDelta.y);

            InventoryManager.Instance.HoverObject.transform.SetParent(GameObject.Find("Canvas").transform, true);
            InventoryManager.Instance.HoverObject.transform.localScale = InventoryManager.Instance.Clicked.gameObject.transform.localScale;
            InventoryManager.Instance.HoverObject.transform.GetChild(0).GetComponent<Text>().text = InventoryManager.Instance.MovingSlot.Items.Count > 1 ? InventoryManager.Instance.MovingSlot.Items.Count.ToString() : string.Empty;

        }

        private void PutItemBack() {
            if (InventoryManager.Instance.From != null) {
                Destroy(GameObject.Find("Hover"));
                InventoryManager.Instance.From.GetComponent<Image>().color = Color.white;
                InventoryManager.Instance.From = null;
            } else if (!InventoryManager.Instance.MovingSlot.IsEmpty) {
                Destroy(GameObject.Find("Hover"));
                foreach (ItemScript item in InventoryManager.Instance.MovingSlot.Items) {
                    InventoryManager.Instance.Clicked.GetComponent<Slot>().AddItem(item);
                }
                InventoryManager.Instance.MovingSlot.ClearSlot();
            }
            InventoryManager.Instance.selectStackSize.SetActive(false);
        }

        public void SplitStack() {
            InventoryManager.Instance.selectStackSize.SetActive(false);
            if (InventoryManager.Instance.SplitAmount == InventoryManager.Instance.MaxStackCount) {
                MoveItem(InventoryManager.Instance.Clicked);
            } else if (InventoryManager.Instance.SplitAmount > 0) {
                InventoryManager.Instance.MovingSlot.Items = InventoryManager.Instance.Clicked.GetComponent<Slot>().RemoveItems(InventoryManager.Instance.SplitAmount);
                CreateHoverIcon();
            }
        }

        public void ChangeStackText(int i) {
            InventoryManager.Instance.SplitAmount += i;
            if (InventoryManager.Instance.SplitAmount < 0) {
                InventoryManager.Instance.SplitAmount = 0;
            }
            if (InventoryManager.Instance.SplitAmount > InventoryManager.Instance.MaxStackCount) {
                InventoryManager.Instance.SplitAmount = InventoryManager.Instance.MaxStackCount;
            }
            InventoryManager.Instance.stackText.text = InventoryManager.Instance.SplitAmount.ToString();
        }

        public void MergeStacks(Slot source, Slot destination) {
            int max = destination.CurrentItem.Item.MaxSize - destination.Items.Count;
            int count = source.Items.Count < max ? source.Items.Count : max;
            for (int i = 0; i < count; i++) {
                destination.AddItem(source.RemoveItem());
                InventoryManager.Instance.HoverObject.transform.GetChild(0).GetComponent<Text>().text = InventoryManager.Instance.MovingSlot.Items.Count.ToString();
            }
            if (source.Items.Count == 0) {
                source.ClearSlot();
                Destroy(GameObject.Find("Hover"));
            }
        }

        private IEnumerator FadeIn() {
            if (!fadingIn) {
                fadingOut = false;
                fadingIn = true;
                StopCoroutine("FadeOut");
                float startAlpha = canvasGroup.alpha;
                float rate = 1.0f / fadeTime;
                float progress = 0.0f;
                while (progress < 1.0) {
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, 1, progress);
                    progress += rate * Time.deltaTime;
                    yield return null;
                }
                canvasGroup.alpha = 1;
                fadingIn = false;
            }
        }
    }
}
