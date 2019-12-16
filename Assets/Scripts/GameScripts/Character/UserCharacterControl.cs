using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using System;

namespace FantasyRPG.Character {
    [RequireComponent(typeof(ThirdPersonController))]
    public class UserCharacterControl : MonoBehaviour {
        public new Camera camera;

        [Serializable]
        public class MouseOrbitSettings {
            public Transform target;
            public float distanceMin = 3f;
            public float distanceMax = 5f;
            public float xAimSpeed = 30.0f;
            public float yAimSpeed = 30.0f;
            public float xSpeed = 60.0f;
            public float ySpeed = 60.0f;
            public float yMinLimit = -20f;
            public float yMaxLimit = 80f;
            
            public float xOffset = 0.6f;
            public float yOffset = 0.0f;
            public bool lockCursor = true;
            public bool cameraCollision = true;
            [NonSerialized]
            public float distance = 4.0f;
            [NonSerialized]
            public float xCurrentSpeed = 0f;
            [NonSerialized]
            public float yCurrentSpeed = 0f;
            [NonSerialized]
            public float x = 0.0f;
            [NonSerialized]
            public float y = 0.0f;

        } 

        public MouseOrbitSettings mouseOrbitSettings = new MouseOrbitSettings();
        private ThirdPersonController m_Character;
        private bool m_Jump;
        private bool zoomedIn = false;
        private bool m_cursorIsLocked = true;

        void Start() {
            if(camera == null) {
                camera = Camera.main;
            }
            m_Character = GetComponent<ThirdPersonController>();
            Vector3 angles = transform.eulerAngles;
            mouseOrbitSettings.x = angles.y;
            mouseOrbitSettings.y = angles.x;
        }

        void Update() {
            zoomedIn = false;
            if (CrossPlatformInputManager.GetButton("Fire2")) {
                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1)) {
                    zoomedIn = true;
                }
            }
            if (CrossPlatformInputManager.GetAxis("Vertical") != 0f || CrossPlatformInputManager.GetAxis("Horizontal") != 0f) {
                Rotate();
            }
            Aim();
            UpdateCursorLock();
            m_Character.RotateView();

            if (CrossPlatformInputManager.GetButtonDown("Jump") && !m_Jump) {
                m_Jump = true;
                m_cursorIsLocked = true;
            }
        }
        void LateUpdate() {
            if (mouseOrbitSettings.target && m_cursorIsLocked) {
                mouseOrbitSettings.x += CrossPlatformInputManager.GetAxis("Mouse X") * mouseOrbitSettings.xCurrentSpeed * mouseOrbitSettings.distance * 0.02f;
                mouseOrbitSettings.y -= CrossPlatformInputManager.GetAxis("Mouse Y") * mouseOrbitSettings.yCurrentSpeed * 0.02f;

                mouseOrbitSettings.y = ClampAngle(mouseOrbitSettings.y, mouseOrbitSettings.yMinLimit, mouseOrbitSettings.yMaxLimit);

                Quaternion rotation = Quaternion.Euler(mouseOrbitSettings.y, mouseOrbitSettings.x, 0);

                mouseOrbitSettings.distance = Mathf.Clamp(mouseOrbitSettings.distance - CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") * 5, mouseOrbitSettings.distanceMin, mouseOrbitSettings.distanceMax);
                if (mouseOrbitSettings.cameraCollision) {
                    RaycastHit hit;
                    if (Physics.Linecast(mouseOrbitSettings.target.position, camera.transform.position, out hit)) {
                        mouseOrbitSettings.distance -= hit.distance;
                    }
                }                
                Vector3 negDistance = new Vector3(mouseOrbitSettings.xOffset, mouseOrbitSettings.yOffset, -mouseOrbitSettings.distance);
                Vector3 position = rotation * negDistance + mouseOrbitSettings.target.position;

                camera.transform.rotation = rotation;
                camera.transform.position = position;
            }
        }

        void FixedUpdate() {
            Vector2 input = GetInput();
            Vector3 desiredMove = Vector3.zero;
            if (Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) {
                desiredMove = camera.transform.forward * input.y + camera.transform.right * input.x;
                desiredMove = Vector3.ProjectOnPlane(desiredMove, m_Character.GroundContactNormal).normalized;
                if (zoomedIn) {
                    m_Character.movementSettings.CurrentTargetSpeed =m_Character.movementSettings.AimSpeed;
                }
                desiredMove.x = desiredMove.x * m_Character.movementSettings.CurrentTargetSpeed;
                desiredMove.z = desiredMove.z * m_Character.movementSettings.CurrentTargetSpeed;
                desiredMove.y = desiredMove.y * m_Character.movementSettings.CurrentTargetSpeed;
                m_cursorIsLocked = true;
            }
            //GameObject.Find("DebugText").GetComponent<UnityEngine.UI.Text>().text = desiredMove.ToString();
            m_Character.Move(desiredMove, m_Jump);
            m_Jump = false;
        }

        private Vector2 GetInput() {
            Vector2 input = new Vector2 {
                x = CrossPlatformInputManager.GetAxis("Horizontal"),
                y = CrossPlatformInputManager.GetAxis("Vertical")
            };
            m_Character.movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }
        void Aim() {
            if (zoomedIn) {
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, m_Character.zoomFOV, Time.deltaTime * m_Character.zoomingSpeed);
                m_cursorIsLocked = true;
                Rotate();
                mouseOrbitSettings.xCurrentSpeed = mouseOrbitSettings.xAimSpeed;
                mouseOrbitSettings.yCurrentSpeed = mouseOrbitSettings.yAimSpeed;
            } else {
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, m_Character.normalFOV, Time.deltaTime * m_Character.zoomingSpeed);
                mouseOrbitSettings.xCurrentSpeed = mouseOrbitSettings.xSpeed;
                mouseOrbitSettings.yCurrentSpeed = mouseOrbitSettings.ySpeed;
            }
        }

        void Rotate() {
            Vector3 aimRotation = transform.rotation.eulerAngles;
            aimRotation.y = camera.transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(aimRotation), Time.deltaTime * m_Character.rotateSpeed);
        }

        float ClampAngle(float angle, float min, float max) {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }

        public void UpdateCursorLock() {
            if (mouseOrbitSettings.lockCursor)
                InternalLockUpdate();
        }

        private void InternalLockUpdate() {
            if (Input.GetKeyUp(KeyCode.Escape)) {
                m_cursorIsLocked = false;
            } else if (Input.GetMouseButtonUp(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1)) {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } else if (!m_cursorIsLocked) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
