using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class KeyBindScript : MonoBehaviour {

    public Text MoveForwardText, StrafeLeftText, MoveBackwarText, StrafeRightText, TurnLeftText, JumpText, TurnRightText;
    public Text Hotkey1Text, Hotkey2Text, Hotkey3Text, Hotkey4Text, Hotkey5Text, Hotkey6Text, Hotkey7Text, Hotkey8Text, Hotkey9Text, Hotkey10Text;
    public Text CharacterText, InventoryText, InteractText;
    public CanvasGroup canvasGroup;
    public float fadeTime;

    private Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();
    private GameObject currentKey;
    private Color32 normal = new Color32(151, 126, 20, 255);
    private Color32 selected = new Color32(47, 193, 108, 255);
    private bool fadingIn = false;
    private bool fadingOut = false;
    //private bool isOpen = false;

    private void Initialize () {
        // MOVEMENT
        keys.Add("MoveForward", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-MoveForward", "W")));
        keys.Add("StrafeLeft", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-StrafeLeft", "A")));
        keys.Add("MoveBackward", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-MoveBackward", "S")));
        keys.Add("StrafeRight", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-StrafeRight", "D")));
        keys.Add("TurnLeft", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-TurnLeft", "Q")));
        keys.Add("Jump", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-Jump", "Space")));
        keys.Add("TurnRight", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-TurnRight", "E")));

        MoveForwardText.text = keys["MoveForward"].ToString();
        StrafeLeftText.text = keys["StrafeLeft"].ToString();
        MoveBackwarText.text = keys["MoveBackward"].ToString();
        StrafeRightText.text = keys["StrafeRight"].ToString();
        TurnLeftText.text = keys["TurnLeft"].ToString();
        JumpText.text = keys["Jump"].ToString();
        TurnRightText.text = keys["TurnRight"].ToString();

        // HOTKEY
        keys.Add("Hotkey1", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-Hotkey1", "Alpha1")));
        keys.Add("Hotkey2", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-Hotkey2", "Alpha2")));
        keys.Add("Hotkey3", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-Hotkey3", "Alpha3")));
        keys.Add("Hotkey4", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-Hotkey4", "Alpha4")));
        keys.Add("Hotkey5", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-Hotkey5", "Alpha5")));
        keys.Add("Hotkey6", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-Hotkey6", "Alpha6")));
        keys.Add("Hotkey7", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-Hotkey7", "Alpha7")));
        keys.Add("Hotkey8", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-Hotkey8", "Alpha8")));
        keys.Add("Hotkey9", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-Hotkey9", "Alpha9")));
        keys.Add("Hotkey10", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-Hotkey10", "Alpha0")));
        Hotkey1Text.text = keys["Hotkey1"].ToString();
        Hotkey2Text.text = keys["Hotkey2"].ToString();
        Hotkey3Text.text = keys["Hotkey3"].ToString();
        Hotkey4Text.text = keys["Hotkey4"].ToString();
        Hotkey5Text.text = keys["Hotkey5"].ToString();
        Hotkey6Text.text = keys["Hotkey6"].ToString();
        Hotkey7Text.text = keys["Hotkey7"].ToString();
        Hotkey8Text.text = keys["Hotkey8"].ToString();
        Hotkey9Text.text = keys["Hotkey9"].ToString();
        Hotkey10Text.text = keys["Hotkey10"].ToString();

        // ETC
        keys.Add("CharacterKey", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-CharacterKey", "C")));
        keys.Add("InventoryKey", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-InventoryKey", "B")));
        keys.Add("InteractKey", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("KeyBind-InteractKey", "F")));
        CharacterText.text = keys["CharacterKey"].ToString();
        InventoryText.text = keys["InventoryKey"].ToString();
        InteractText.text = keys["InteractKey"].ToString();
    }

    void Start() {
        Initialize();
    }

    void Update() {
        if(currentKey!=null && Input.GetKeyDown(KeyCode.Escape)) {
            currentKey.GetComponent<Image>().color = normal;
            currentKey = null;
        }
        //if (Input.GetKeyDown(keys["Up"]){
        //}
    }

    void OnGUI() {
        if(currentKey != null) {
            Event e = Event.current;
            if (e.isKey) {
                keys[currentKey.name] = e.keyCode;
                currentKey.transform.GetComponentInChildren<Text>().text = e.keyCode.ToString();
                currentKey.GetComponent<Image>().color = normal;
                currentKey = null;
            }
        }
    }
    public void ChangeKey(GameObject clicked) {
        if (currentKey != null) {
            currentKey.GetComponent<Image>().color = normal;
        }
        currentKey = clicked;
        currentKey.GetComponent<Image>().color = selected;
    }

    public void SaveKeys() {
        foreach (var key in keys) {
            PlayerPrefs.SetString("KeyBind-"+key.Key, key.Value.ToString());
        }
        PlayerPrefs.Save();
    }

    public void ResetKeys() {
        foreach (var key in keys) {
            if(PlayerPrefs.HasKey("KeyBind-" + key.Key)) {
                PlayerPrefs.DeleteKey("KeyBind-" + key.Key);
            }
        }
        keys = new Dictionary<string, KeyCode>();
        Initialize();
    }

    public void Open() {
        if (canvasGroup.alpha == 0) {
            StartCoroutine("FadeIn");
            //isOpen = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void Close() {
        if (canvasGroup.alpha > 0) {
            StartCoroutine("FadeOut");
            //isOpen = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private IEnumerator FadeOut() {
        if (!fadingOut) {
            fadingOut = true;
            fadingIn = false;
            StopCoroutine("FadeIn");
            float startAlpha = canvasGroup.alpha;
            float rate = 1.0f / fadeTime;
            float progress = 0.0f;
            while (progress < 1.0) {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, progress);
                progress += rate * Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 0;
            fadingOut = false;
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
