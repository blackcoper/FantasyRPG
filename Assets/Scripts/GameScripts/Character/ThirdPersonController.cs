using UnityEngine;
using System.Collections;
using System;
using UnityStandardAssets.CrossPlatformInput;

namespace FantasyRPG.Character {
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    public class ThirdPersonController : MonoBehaviour {

        [Serializable]
        public class MovementSettings {
            public float m_RunCycleLegOffset = 0.2f;
            public float m_AnimSpeedMultiplier = 1f;
            public float AimSpeed = 2.0f;
            public float ForwardSpeed = 3.0f; 
            public float BackwardSpeed = 2.0f;
            public float StrafeSpeed = 3.0f; 
            public float RunMultiplier = 2.0f;
            public KeyCode RunKey = KeyCode.LeftShift;
            public float JumpForce = 30f;
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [HideInInspector]
            public float CurrentTargetSpeed = 8f;

#if !MOBILE_INPUT
            private bool m_Running;
#endif

            public void UpdateDesiredTargetSpeed(Vector2 input) {
                if (input == Vector2.zero) return;
                if (input.x > 0 || input.x < 0) {
                    //strafe
                    CurrentTargetSpeed = StrafeSpeed;
                }
                if (input.y < 0) {
                    //backwards
                    CurrentTargetSpeed = BackwardSpeed;
                }
                if (input.y > 0) {
                    //forwards
                    //handled last as if strafing and moving forward at the same time forwards speed should take precedence
                    CurrentTargetSpeed = ForwardSpeed;
                }
#if !MOBILE_INPUT
                if (Input.GetKey(RunKey)) {
                    CurrentTargetSpeed *= RunMultiplier;
                    m_Running = true;
                } else {
                    m_Running = false;
                }
#endif
            }

#if !MOBILE_INPUT
            public bool Running {
                get { return m_Running; }
            }
#endif
        }

        [Serializable]
        public class AdvancedSettings {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundHelperDistance = 0.5f; // stops the character
            public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
            public bool airControl; // can the user control the direction that is being moved in the air
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
        }
        
        public MovementSettings movementSettings = new MovementSettings();        
        public AdvancedSettings advancedSettings = new AdvancedSettings();
        
        //public Camera camera;
        public float rotateSpeed = 2f;
        public float zoomFOV = 20;
        public float normalFOV = 40;
        public float zoomingSpeed = 3f;
        
        private Rigidbody m_RigidBody;
        private Animator m_Animator;
        private CapsuleCollider m_Capsule;
        private float m_YRotation;
        private Vector3 m_GroundContactNormal;
        public bool m_PreviouslyGrounded, m_Jumping, m_IsGrounded;
        
        const float k_Half = 0.5f;
        float m_TurnAmount;
        float m_ForwardAmount;
        bool m_Crouching;

        float turnLimit;
        float backLimit;
        float forwardLimit;

        public Vector3 Velocity {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded {
            get { return m_IsGrounded; }
        }

        public bool Jumping {
            get { return m_Jumping; }
        }

        public bool Running {
            get {
#if !MOBILE_INPUT
                return movementSettings.Running;
#else
	            return false;
#endif
            }
        }
        
        public Vector3 GroundContactNormal {
            get {
                return m_GroundContactNormal;
            }
            set {
                m_GroundContactNormal = value;
            }
        }

        private void Start() {
            m_Animator = GetComponentInChildren<Animator>();
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            //mouseLook.Init(transform, cam.transform);
            
            // Make the rigid body not change rotation
            if (m_RigidBody != null) {
                m_RigidBody.freezeRotation = true;
            }

            turnLimit = movementSettings.StrafeSpeed * movementSettings.RunMultiplier;
            backLimit = movementSettings.BackwardSpeed * movementSettings.RunMultiplier;
            forwardLimit = movementSettings.ForwardSpeed * movementSettings.RunMultiplier;
        }

        public void Move(Vector3 desiredMove,bool m_Jump) {
            GroundCheck();
            
            if (advancedSettings.airControl || m_IsGrounded) {
                if (m_RigidBody.velocity.sqrMagnitude < (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed)) {
                    m_RigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
                }
            }            

            if (m_IsGrounded) {
                m_RigidBody.drag = 5f;

                if (m_Jump) {
                    m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                    m_Jumping = true;
                }

                if (!m_Jumping && Mathf.Abs(desiredMove.x) < float.Epsilon && Mathf.Abs(desiredMove.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f) {
                    m_RigidBody.Sleep();
                }
            } else {
                m_RigidBody.drag = 0f;
                if (m_PreviouslyGrounded && !m_Jumping) {
                    StickToGroundHelper();
                }
            }
            UpdateAnimator(desiredMove, m_Jump);
            m_Jump = false;
        }
        
        void UpdateAnimator(Vector3 move, bool jump) {
            //if (move.magnitude > 1f) move.Normalize();
            
            move = transform.InverseTransformDirection(move);
            m_TurnAmount = Utility.Math.LimitedRange(move.x, -turnLimit, turnLimit, -1, 1);
            m_ForwardAmount = Utility.Math.LimitedRange(move.z, 0, move.z > 0 ? forwardLimit : backLimit, 0, 1);
            //GameObject.Find("DebugText").GetComponent<UnityEngine.UI.Text>().text = "move : "+move + "\nForward : "+m_ForwardAmount+ ", Turn : "+m_TurnAmount;
            
            m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
            m_Animator.SetBool("Crouch", m_Crouching);
            m_Animator.SetBool("OnGround", m_IsGrounded);
            if (!m_IsGrounded && jump) {
                m_Animator.SetFloat("Jump", m_RigidBody.velocity.y);
            }
                
            // calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
            float runCycle =
                Mathf.Repeat(
                    m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + movementSettings.m_RunCycleLegOffset, 1);
            float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
            if (m_IsGrounded) {
                m_Animator.SetFloat("JumpLeg", jumpLeg);
            }

            // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
            // which affects the movement speed because of the root motion.
            //if (m_IsGrounded && move.magnitude > 0) {
            //    m_Animator.speed = movementSettings.m_AnimSpeedMultiplier;
            //} else {
            //    // don't use that while airborne
            //    m_Animator.speed = 1;
            //}
        }

        private float SlopeMultiplier() {
            float angle = Vector3.Angle(GroundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }


        private void StickToGroundHelper() {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height / 2f) - m_Capsule.radius) +
                                   advancedSettings.stickToGroundHelperDistance, ~0, QueryTriggerInteraction.Ignore)) {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f) {
                    m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
                }
            }
        }
        
        public void RotateView() {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;
            
            if (m_IsGrounded || advancedSettings.airControl) {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation * m_RigidBody.velocity;
            }
        }

        //public void OnAnimatorMove() {
        //    // we implement this function to override the default root motion.
        //    // this allows us to modify the positional speed before it's applied.
        //    if (m_IsGrounded && Time.deltaTime > 0) {
        //        Vector3 v = (m_Animator.deltaPosition * movementSettings.CurrentTargetSpeed) / Time.deltaTime;

        //        // we preserve the existing y part of the current velocity.
        //        v.y = m_RigidBody.velocity.y;
        //        m_RigidBody.velocity = v;
        //    }
        //}

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        public void GroundCheck() {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height / 2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance, ~0, QueryTriggerInteraction.Ignore)) {
                m_IsGrounded = true;
                GroundContactNormal = hitInfo.normal;
            } else {
                m_IsGrounded = false;
                GroundContactNormal = Vector3.up;
            }
            if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping) {
                m_Jumping = false;
            }
        }
    }
}