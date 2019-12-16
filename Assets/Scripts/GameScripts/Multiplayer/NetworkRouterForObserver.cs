using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
//[RequireComponent(typeof(Animator))]
public class NetworkRouterForObserver : NetworkBehaviour {

    [Header("NetworkBehaviors")]
    public NetworkObserver n_observer;
    //public NetworkObserverInventory n_inventory;

    [Header("MonoBehaviors")]
    public AudioListener audioListener;
    public PlayerMovement playerMovement;
    public CharacterController characterController;
    public Rigidbody n_rigidbody;
    public ClearSight clearSight;
    public SmoothFollow smoothFollow;
    void Awake() {
        //n_observer = GetComponent<NetworkObserver>();
        //n_inventory = GetComponentInParent<NetworkObserverInventory>();
        audioListener = GetComponent<AudioListener>();
        //playerMovement = GetComponent<PlayerMovement>();
        //characterController = GetComponent<CharacterController>();
        n_rigidbody = GetComponent<Rigidbody>();
        clearSight = GetComponent<ClearSight>();
        
    }
	void Start () {
        if (isLocalPlayer)
        {
            audioListener.enabled = true;
            //playerMovement.enabled = true;
            clearSight.enabled = true;
            /*
            smoothFollow =  Camera.main.GetComponent<SmoothFollow>();
            smoothFollow.enabled = true;
            smoothFollow.target = transform;
            */
            UnityStandardAssets.Utility.FollowTarget followTarget = GameObject.FindGameObjectWithTag("MainCamera").AddComponent<UnityStandardAssets.Utility.FollowTarget>();
            followTarget.target = transform;

        }
    }
	
}
 