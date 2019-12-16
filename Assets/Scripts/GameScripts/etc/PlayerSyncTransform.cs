using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[NetworkSettings(channel =0, sendInterval =0.1f)]
public class Player_SyncTransform : NetworkBehaviour {

    [SyncVar(hook ="SyncPositionValues")] private Vector3 syncPos;
    [SyncVar] private Quaternion syncRot;

    private float lerpRate;
    [SerializeField]
    private float normalLerpRate = 16;
    [SerializeField]
    private float fasterLerpRate = 27;

    private Vector3 lastPos;
    private Quaternion lastRot;
    private float thresholdPos = 0.5f; //0.5f;
    private float thresholdRot = 5; //5;

    private NetworkClient nClient;
    private float latency;
    private Text latencyText;

    private List<Vector3> syncPosList = new List<Vector3>();
    [SerializeField] private bool useHistoricalLerping = false;
    [SerializeField] private float closeEnough = 0.11f;

    void Start () {
        if (isLocalPlayer)
        {
            GetComponent<AudioListener>().enabled = true;
            GetComponent<PlayerMovement>().enabled = true;
            UnityStandardAssets.Utility.FollowTarget followTarget = GameObject.FindGameObjectWithTag("MainCamera").AddComponent<UnityStandardAssets.Utility.FollowTarget>();
            followTarget.target = transform;
        }
        nClient = GameObject.Find("NetworkManager").GetComponent<NetworkManager>().client;
        latencyText = GameObject.Find("Latency Text").GetComponent<Text>();
        lerpRate = normalLerpRate;
    }

    void Update () {
        LerpTransform();
        ShowLatency();
    }

    void FixedUpdate () {
        TransmitTranform();
	}

    void LerpTransform() {
        if (!isLocalPlayer) {
            if (useHistoricalLerping) {
                HistoricalLerping();
            }
            else {
                OrdinaryLerping();
            }
        }
    }

    [Command]
    void CmdProvideTransformToServer (Vector3 pos, Quaternion rot) {
        syncPos = pos;
        syncRot = rot;
    }

    [ClientCallback]
    void TransmitTranform() {
        if (isLocalPlayer && (Vector3.Distance(transform.position,lastPos)> thresholdPos || Quaternion.Angle(transform.rotation,lastRot)>thresholdRot )) {
            CmdProvideTransformToServer(transform.position, transform.rotation);
            lastPos = transform.position;
            lastRot = transform.rotation;
        }
    }

    [Client]
    void SyncPositionValues(Vector3 latestPos) {
        syncPos = latestPos;
        syncPosList.Add(syncPos);
    }

    void HistoricalLerping() {
        if(syncPosList.Count > 0) {
            transform.position = Vector3.Lerp(transform.position, syncPosList[0], Time.deltaTime * lerpRate);
            if(Vector3.Distance(transform.position, syncPosList[0]) < closeEnough)
            {
                syncPosList.RemoveAt(0);
            }
            if(syncPosList.Count > 10) {
                lerpRate = fasterLerpRate;
            } else{
                lerpRate = normalLerpRate;
            }
            Debug.Log(lerpRate+"  , "+syncPosList.Count);
        }
    }

    void OrdinaryLerping() {
        transform.position = Vector3.Lerp(transform.position, syncPos, Time.deltaTime * lerpRate);
        transform.rotation = Quaternion.Lerp(transform.rotation, syncRot, Time.deltaTime * lerpRate);
    }

    void ShowLatency() {
        if (isLocalPlayer) {
            latency = nClient.GetRTT();
            latencyText.text = latency.ToString();
        }
    }
}
