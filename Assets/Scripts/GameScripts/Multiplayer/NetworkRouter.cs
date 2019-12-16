using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkRouter : NetworkBehaviour {

    [Header("MonoBehaviors")]
    public SimpleRpgCamera rpgCamera;
    public AudioListener audioListener;
    void Awake() {
        audioListener = GetComponent<AudioListener>();

    }
    void Start() {
        if (isLocalPlayer) {
            audioListener.enabled = true;
            rpgCamera = Camera.main.GetComponent<SimpleRpgCamera>();
            rpgCamera.target = transform;

        } else {
            audioListener.enabled = false;
        }
    }
}
