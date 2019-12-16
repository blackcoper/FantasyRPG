using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
[System.Serializable]
public class MovementProperties
{
    public float speed = 2;
    public float turnSpeed = 1;
}

public class PlayerMovement : NetworkBehaviour {
    public MovementProperties movementProperties;
    [SerializeField] float m_JumpPower = 12f;
    [Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
    [SerializeField] float m_MoveSpeedMultiplier = 1f;
    [SerializeField] float m_GroundCheckDistance = 2.1f;

    Rigidbody m_Rigidbody;
    // Animator m_Animator;

    //Vector3 m_GroundNormal;
    float m_TurnAmount;
    float m_ForwardAmount;
    public bool m_IsGrounded;
    float m_OrigGroundCheckDistance;

    private Vector3 m_Move;
    private Vector3 m_Mouse;
    private bool m_Jump;
    
    void Start () {
        // m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        if (!m_Jump)
        {
            m_Jump = Input.GetButtonDown("Jump");
        }
    }
    
    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool crouch = Input.GetKey(KeyCode.C);
        m_Move = v * Vector3.forward + h * Vector3.right;
#if !MOBILE_INPUT
        if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 2f;
#endif
        Move(m_Move, crouch, m_Jump);
        m_Jump = false;
    }
     

    public void Move(Vector3 move, bool crouch, bool jump)
    {
        CheckGroundStatus();
        if (Time.deltaTime > 0)
        {
            Vector3 v = (move * m_MoveSpeedMultiplier) / Time.deltaTime;
            v.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = v;
        }
        if (m_IsGrounded) {
           
            HandleGroundedMovement(crouch, jump);
        } else {
            HandleAirborneMovement();
        }
        LookAtMousePosition();
        // ScaleCapsuleForCrouching(crouch);
        // PreventStandingInLowHeadroom();
        // UpdateAnimator(move);
    }

    Ray cameraRay;
    RaycastHit cameraRayHit;

    void LookAtMousePosition() {
        if (Vector3.Distance(Input.mousePosition, m_Mouse) > 0.5f) {
            cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay, out cameraRayHit))
            {
                if (cameraRayHit.transform.tag == "Ground")
                {
                    Vector3 targetPosition = new Vector3(cameraRayHit.point.x, transform.position.y, cameraRayHit.point.z);
                    transform.LookAt(targetPosition);
                    m_Mouse = Input.mousePosition;
                }
            }
        }
    }

    void CheckGroundStatus()
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
        {
            //m_GroundNormal = hitInfo.normal;
            m_IsGrounded = true;
            // m_Animator.applyRootMotion = true;
        }
        else
        {
            m_IsGrounded = false;
            //m_GroundNormal = Vector3.up;
            // m_Animator.applyRootMotion = false;
        }
    }

    void HandleGroundedMovement(bool crouch, bool jump)
    {
        if (jump && !crouch) //&& m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded")
        {
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
            m_IsGrounded = false;
        }
    }

    void HandleAirborneMovement()
    {
        Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
        m_Rigidbody.AddForce(extraGravityForce);
    }
}
