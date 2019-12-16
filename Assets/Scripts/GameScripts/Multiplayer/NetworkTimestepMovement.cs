﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

/// <summary>
/// QoS channels:
/// channel #0: Reliable Sequenced
/// channel #1: Unreliable Sequenced
/// </summary>
[NetworkSettings(channel = 1, sendInterval = 0.05f)]
public class NetworkTimestepMovement : NetworkBehaviour {

    public struct Inputs {
        public float forward;
        public float sides;
        public float yaw;
        public float vertical;
        public float pitch;
        public bool sprint;
        public bool crouch;
        public float timeStamp;
    }

    public struct SyncInputs {
        public sbyte forward;
        public sbyte sides;
        public float yaw;
        public sbyte vertical;
        public float pitch;
        public bool sprint;
        public bool crouch;
        public float timeStamp;
    }

    public struct Results {
        public Quaternion rotation;
        public Vector3 position;
        public bool sprinting;
        public bool crouching;
        public float timeStamp;
    }

    public struct SyncResults {
        public ushort yaw;
        public ushort pitch;
        public Vector3 position;
        public bool sprinting;
        public bool crouching;
        public float timeStamp;
    }

    [HideInInspector]
    public NetworkRouterForObserver router;

    private Inputs m_inputs;
    [SyncVar(hook = "RecieveResults")]
    private SyncResults syncResults;

    private Results m_results;

    public CursorLockMode m_CursorLockMode;
    public bool m_controlsLocked = false;
    public float m_mouseSense = 100;

    private List<Inputs> m_inputsList = new List<Inputs>();
    private List<Results> m_resultsList = new List<Results>();

    private bool m_playData = false;

    private float m_dataStep = 0;
    private float m_lastTimeStamp = 0;

    private bool m_jumping = false;
    private Vector3 m_startPosition;
    private Quaternion m_startRotation;

    private float m_step = 0;

    public Transform m_observer;

    public void Awake() {
        m_observer = transform;
        router = GetComponent<NetworkRouterForObserver>();
    }

    void Start() {
        gameObject.name = "Network!" + netId + "!";
        //Debug.Log(GetObjectDebugInfo());
    }

    void Update() {
        if (isLocalPlayer) {
            if (!m_controlsLocked) {
                GetInputs(ref m_inputs);
            }
        }
    }

    void FixedUpdate() {
        if (isLocalPlayer) {
            m_inputs.timeStamp = Time.time;
            //Client side prediction for non-authoritative client or plane movement and rotation for listen server/host
            Vector3 lastPosition = m_results.position;
            Quaternion lastRotation = m_results.rotation;
            bool lastCrouch = m_results.crouching;
            m_results.rotation = Rotate(m_inputs, m_results);
            m_results.crouching = Crouch(m_inputs, m_results);
            m_results.sprinting = Sprint(m_inputs, m_results);
            m_results.position = Move(m_inputs, m_results);
            if (hasAuthority) {
                //Listen server/host part
                //Sending results to other clients(state sync)
                if (m_dataStep >= GetNetworkSendInterval()) {
                    if (Vector3.Distance(m_results.position, lastPosition) > 0 || Quaternion.Angle(m_results.rotation, lastRotation) > 0 || m_results.crouching != lastCrouch) {
                        m_results.timeStamp = m_inputs.timeStamp;
                        //Struct need to be fully new to count as dirty 
                        //Convering some of the values to get less traffic
                        SyncResults tempResults;
                        tempResults.yaw = (ushort)(m_results.rotation.eulerAngles.y * 182);
                        tempResults.pitch = (ushort)(m_results.rotation.eulerAngles.x * 182);
                        tempResults.position = m_results.position;
                        tempResults.sprinting = m_results.sprinting;
                        tempResults.crouching = m_results.crouching;
                        tempResults.timeStamp = m_results.timeStamp;
                        syncResults = tempResults;
                    }
                    m_dataStep = 0;
                }
                m_dataStep += Time.fixedDeltaTime;
            } else {
                //Owner client. Non-authoritative part
                //Add inputs to the inputs list so they could be used during reconciliation process
                if (Vector3.Distance(m_results.position, lastPosition) > 0 || Quaternion.Angle(m_results.rotation, lastRotation) > 0 || m_results.crouching != lastCrouch) {
                    m_inputsList.Add(m_inputs);
                }
                //Sending inputs to the server
                //Unfortunately there is now method overload for [Command] so I need to write several almost similar functions
                //This one is needed to save on network traffic
                SyncInputs syncInputs;
                syncInputs.forward = (sbyte)(m_inputs.forward * 127);
                syncInputs.sides = (sbyte)(m_inputs.sides * 127);
                syncInputs.vertical = (sbyte)(m_inputs.vertical * 127);
                
                if (Vector3.Distance(m_results.position, lastPosition) > 0) {
                    
                    if (Quaternion.Angle(m_results.rotation, lastRotation) > 0) {
                        Cmd_MovementRotationInputs(syncInputs.forward, syncInputs.sides, syncInputs.vertical, m_inputs.pitch, m_inputs.yaw, m_inputs.sprint, m_inputs.crouch, m_inputs.timeStamp);
                    } else {
                        Cmd_MovementInputs(syncInputs.forward, syncInputs.sides, syncInputs.vertical, m_inputs.sprint, m_inputs.crouch, m_inputs.timeStamp);
                    }
                } else {
                    if (Quaternion.Angle(m_results.rotation, lastRotation) > 0) {
                        Cmd_RotationInputs(m_inputs.pitch, m_inputs.yaw, m_inputs.crouch, m_inputs.timeStamp);
                    } else {
                        Cmd_OnlyStances(m_inputs.crouch, m_inputs.timeStamp);
                    }
                }
            }
        } else {
            if (hasAuthority) {
                //Server

                //Check if there is atleast one record in inputs list
                if (m_inputsList.Count == 0) {
                    return;
                }
                //Move and rotate part. Nothing interesting here
                Inputs inputs = m_inputsList[0];
                m_inputsList.RemoveAt(0);
                Vector3 lastPosition = m_results.position;
                Quaternion lastRotation = m_results.rotation;
                bool lastCrouch = m_results.crouching;
                m_results.rotation = Rotate(inputs, m_results);
                m_results.crouching = Crouch(inputs, m_results);
                m_results.sprinting = Sprint(inputs, m_results);
                m_results.position = Move(inputs, m_results);
                //Sending results to other clients(state sync)

                if (m_dataStep >= GetNetworkSendInterval()) {
                    if (Vector3.Distance(m_results.position, lastPosition) > 0 || Quaternion.Angle(m_results.rotation, lastRotation) > 0 || m_results.crouching != lastCrouch) {
                        //Struct need to be fully new to count as dirty 
                        //Convering some of the values to get less traffic
                        m_results.timeStamp = inputs.timeStamp;
                        SyncResults tempResults;
                        tempResults.yaw = (ushort)(m_results.rotation.eulerAngles.y * 182);
                        tempResults.pitch = (ushort)(m_results.rotation.eulerAngles.x * 182);
                        tempResults.position = m_results.position;
                        tempResults.sprinting = m_results.sprinting;
                        tempResults.crouching = m_results.crouching;
                        tempResults.timeStamp = m_results.timeStamp;
                        syncResults = tempResults;
                    }
                    m_dataStep = 0;
                }
                m_dataStep += Time.fixedDeltaTime;
            } else {
                //Non-owner client a.k.a. dummy client
                //there should be at least two records in the results list so it would be possible to interpolate between them in case if there would be some dropped packed or latency spike
                //And yes this stupid structure should be here because it should start playing data when there are at least two records and continue playing even if there is only one record left 
                if (m_resultsList.Count == 0) {
                    m_playData = false;
                }
                if (m_resultsList.Count >= 2) {
                    m_playData = true;
                }
                if (m_playData) {
                    if (m_dataStep == 0) {
                        m_startPosition = m_results.position;
                        m_startRotation = m_results.rotation;
                    }
                    m_step = 1 / (GetNetworkSendInterval());
                    m_results.rotation = Quaternion.Slerp(m_startRotation, m_resultsList[0].rotation, m_dataStep);
                    m_results.position = Vector3.Lerp(m_startPosition, m_resultsList[0].position, m_dataStep);
                    m_results.crouching = m_resultsList[0].crouching;
                    m_results.sprinting = m_resultsList[0].sprinting;
                    m_dataStep += m_step * Time.fixedDeltaTime;
                    if (m_dataStep >= 1) {
                        m_dataStep = 0;
                        m_resultsList.RemoveAt(0);
                    }
                }
                UpdateRotation(m_results.rotation);
                UpdatePosition(m_results.position);
                UpdateCrouch(m_results.crouching);
                UpdateSprinting(m_results.sprinting);
            }
        }
    }

    // Command

    [Command(channel = 0)]
    void Cmd_OnlyStances(bool crouch, float timeStamp) {
        if (hasAuthority && !isLocalPlayer) {
            Inputs inputs;
            inputs.forward = 0;
            inputs.sides = 0;
            inputs.pitch = 0;
            inputs.vertical = 0;
            inputs.yaw = 0;
            inputs.sprint = false;
            inputs.crouch = crouch;
            inputs.timeStamp = timeStamp;
            m_inputsList.Add(inputs);
        }
    }
    [Command(channel = 0)]
    void Cmd_RotationInputs(float pitch, float yaw, bool crouch, float timeStamp) {
        if (hasAuthority && !isLocalPlayer) {
            Inputs inputs;
            inputs.forward = 0;
            inputs.sides = 0;
            inputs.vertical = 0;
            inputs.pitch = pitch;
            inputs.yaw = yaw;
            inputs.sprint = false;
            inputs.crouch = crouch;
            inputs.timeStamp = timeStamp;
            m_inputsList.Add(inputs);
        }
    }
    [Command(channel = 0)]
    void Cmd_MovementRotationInputs(sbyte forward, sbyte sides, sbyte vertical, float pitch, float yaw, bool sprint, bool crouch, float timeStamp) {
        if (hasAuthority && !isLocalPlayer) {
            Inputs inputs;
            inputs.forward = Mathf.Clamp((float)forward / 127, -1, 1);
            inputs.sides = Mathf.Clamp((float)sides / 127, -1, 1);
            inputs.vertical = Mathf.Clamp((float)vertical / 127, -1, 1);
            inputs.pitch = pitch;
            inputs.yaw = yaw;
            inputs.sprint = sprint;
            inputs.crouch = crouch;
            inputs.timeStamp = timeStamp;
            m_inputsList.Add(inputs);
        }
    }
    [Command(channel = 0)]
    void Cmd_MovementInputs(sbyte forward, sbyte sides, sbyte vertical, bool sprint, bool crouch, float timeStamp) {
        if (hasAuthority && !isLocalPlayer) {
            Inputs inputs;
            inputs.forward = Mathf.Clamp((float)forward / 127, -1, 1);
            inputs.sides = Mathf.Clamp((float)sides / 127, -1, 1);
            inputs.vertical = Mathf.Clamp((float)vertical / 127, -1, 1);
            inputs.pitch = 0;
            inputs.yaw = 0;
            inputs.sprint = sprint;
            inputs.crouch = crouch;
            inputs.timeStamp = timeStamp;
            m_inputsList.Add(inputs);
        }
    }

    public virtual void GetInputs(ref Inputs inputs) {
        inputs.sides = Input.GetAxisRaw("Horizontal");
        inputs.forward = Input.GetAxisRaw("Vertical");

        inputs.yaw = -Input.GetAxis("Mouse Y") * m_mouseSense * Time.fixedDeltaTime / Time.deltaTime;
        inputs.pitch = Input.GetAxis("Mouse X") * m_mouseSense * Time.fixedDeltaTime / Time.deltaTime;
        inputs.sprint = Input.GetButton("Sprint");
        inputs.crouch = Input.GetButton("Crouch");

        if (Input.GetButtonDown("Jump") && inputs.vertical <= -0.9f) {
            m_jumping = true;
        }
        float verticalTarget = -1;
        if (m_jumping) {
            verticalTarget = 1;
            if (inputs.vertical >= 0.9f) {
                m_jumping = false;
            }
        }
        inputs.vertical = Mathf.Lerp(inputs.vertical, verticalTarget, 20 * Time.deltaTime);
    }

    public virtual void UpdatePosition(Vector3 newPosition) {
        transform.position = newPosition;
    }

    public virtual void UpdateRotation(Quaternion newRotation) {
        transform.rotation = newRotation;
    }

    public virtual void UpdateCrouch(bool crouch) {

    }

    public virtual void UpdateSprinting(bool sprinting) {

    }

    public virtual Vector3 Move(Inputs inputs, Results current) {

        transform.position = current.position;
        float speed = 2;
        if (current.crouching) {
            speed = 1.5f;
        }
        if (current.sprinting) {
            speed = 3;
        }
        transform.Translate(Vector3.ClampMagnitude(new Vector3(inputs.sides, inputs.vertical, inputs.forward), 1) * speed * Time.fixedDeltaTime);
        /*

        ^ ori
        v edit
        
        float speed = 2;
        if (current.crouching)
        {
            speed = 1.5f;
        }
        if (current.sprinting)
        {
            speed = 3;
        }
        m_Move = inputs.forward * Vector3.forward + inputs.sides * Vector3.right;
        m_Move *= speed;
        Vector3 v = m_Move / Time.fixedDeltaTime;
        v.y = router.n_rigidbody.velocity.y;
        router.n_rigidbody.velocity = v;
        return router.n_rigidbody.velocity;
        */
        //router.n_rigidbody.velocity = current.position;
        //return router.n_rigidbody.velocity;
        return transform.position;
    }

    public virtual Quaternion Rotate(Inputs inputs, Results current) {
        transform.rotation = current.rotation;
        float mHor = transform.eulerAngles.y + inputs.pitch * Time.fixedDeltaTime;
        float mVert = transform.eulerAngles.x + inputs.yaw * Time.fixedDeltaTime;

        if (mVert > 180)
            mVert -= 360;
        transform.rotation = Quaternion.Euler(mVert, mHor, 0);
        return transform.rotation;
    }

    public virtual bool Sprint(Inputs inputs, Results current) {
        return inputs.sprint;
    }

    public virtual bool Crouch(Inputs inputs, Results current) {
        return inputs.crouch;
    }

    // Client Callbacks
    [ClientCallback]
    void RecieveResults(SyncResults syncResults) {
        // Convering values back

        Results results;
        results.rotation = Quaternion.Euler((float)syncResults.pitch / 182, (float)syncResults.yaw / 182, 0);
        results.position = syncResults.position;
        results.sprinting = syncResults.sprinting;
        results.crouching = syncResults.crouching;
        results.timeStamp = syncResults.timeStamp;

        // Discard out of order results
        if (results.timeStamp <= m_lastTimeStamp) {
            return;
        }
        m_lastTimeStamp = results.timeStamp;
        // Non-owner client
        if (!isLocalPlayer && !hasAuthority) {

            // Adding results to the results list so they can be used in interpolation process
            results.timeStamp = Time.time;
            m_resultsList.Add(results);
        }

        // Owner client
        // Server client reconciliation process should be executed in order to client's rotation and position with server values but do it without jittering
        if (isLocalPlayer && !hasAuthority) {
            // Update client's position and rotation with ones from server 
            m_results.rotation = results.rotation;
            m_results.position = results.position;
            int foundIndex = -1;

            // Search recieved time stamp in client's inputs list
            for (int index = 0; index < m_inputsList.Count; index++) {
                // If time stamp found run through all inputs starting from needed time stamp 
                if (m_inputsList[index].timeStamp > results.timeStamp) {
                    foundIndex = index;
                    break;
                }
            }
            if (foundIndex == -1) {
                // Clear Inputs list if no needed records found 
                while (m_inputsList.Count != 0) {
                    m_inputsList.RemoveAt(0);
                }
                return;
            }
            // Replay all of the recorded inputs from the user
            for (int subIndex = foundIndex; subIndex < m_inputsList.Count; subIndex++) {
                m_results.rotation = Rotate(m_inputsList[subIndex], m_results);
                m_results.crouching = Crouch(m_inputsList[subIndex], m_results);
                m_results.sprinting = Sprint(m_inputsList[subIndex], m_results);

                m_results.position = Move(m_inputsList[subIndex], m_results);
            }
            // Remove all inputs recorded before the current target time stamp
            int targetCount = m_inputsList.Count - foundIndex;
            while (m_inputsList.Count > targetCount) {
                m_inputsList.RemoveAt(0);
            }
        }
    }


    // // // Helper Methods
    // Public method to set our start position
    public void SetStartPosition(Vector3 position) { m_results.position = position; }

    // Public method to set our start rotation
    public void SetStartRotation(Quaternion rotation) { m_results.rotation = rotation; }

    // Callable by UI buttons and such...
    public void InputsLocked() { m_controlsLocked = true; m_CursorLockMode = CursorLockMode.None; }
    public void InputsUnlock() { m_controlsLocked = false; m_CursorLockMode = CursorLockMode.Locked; }
    public void InputsSetLock(bool lockedstate) { if (lockedstate) { InputsLocked(); } else { InputsUnlock(); } SetCursorState(); }

    // Apply requested cursor state
    public void SetCursorState() {
        Cursor.lockState = m_CursorLockMode;
        // Hide cursor when locking
        Cursor.visible = (CursorLockMode.Locked != m_CursorLockMode);
    }

    // A short helper for Debug.Log and uNET status.
    public string GetObjectDebugInfo() {
        var thisObjectDebugInfo = gameObject.name + "|S:" + GetShorterAnswerToBool(isServer) + "|A:" + GetShorterAnswerToBool(hasAuthority) + "|C:" + GetShorterAnswerToBool(isClient) + "|L:" + GetShorterAnswerToBool(isLocalPlayer) + "|";
        return thisObjectDebugInfo;
    }

    // Stupid helper for GetObjectDebugInfo
    public string GetShorterAnswerToBool(bool input) {
        if (input) { return "Y"; } else { return "N"; }
    }
}