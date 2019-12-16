using UnityEngine;
using System.Collections;

public class NetworkObserver : NetworkTimestepMovement
{
    public GameObject m_pausedPanel; // We assign this reference to the paused panel in the canvas through the inspector. 
    //public Transform m_observer; // We assign this reference through the inspector.
    public Quaternion m_observerRotation;
    public Vector3 m_observerRotationEuler;
    public float m_viewPitch;
    public float m_viewYaw;

    public float m_verticalMouseLookLimit = 170;
    public float m_snapDistance = 1;
    //private float m_verticalSpeed = 0;
    public float m_jumpHeight = 10;
    private bool m_jump = false;

    [SerializeField]
    float m_MoveSpeedMultiplier = 0.1f;
    [SerializeField]
    float m_RotateSpeedMultiplier = 1f;
    [SerializeField]
    float m_GroundCheckDistance = 2.1f;
    private Vector3 m_Move;
    //private Vector3 m_GroundNormal;
    public bool m_IsGrounded = false;
    // // // UnityEngine magic methods
    // Do not define Awake, Start, Update or FixedUpdate in subclasses of NetworkedTimestepMovement.

    // // // Parent class override methods
    public override void GetInputs(ref Inputs inputs)
    {
        inputs.sides = Input.GetAxisRaw("Horizontal");
        inputs.forward = Input.GetAxisRaw("Vertical");
        inputs.yaw = LookAtMousePosition();// - Input.GetAxis("Mouse Y") * m_mouseSense * Time.fixedDeltaTime / Time.deltaTime;
        inputs.pitch = Input.GetAxis("Mouse X") * m_mouseSense * Time.fixedDeltaTime / Time.deltaTime;
        inputs.sprint = Input.GetButton("Sprint");
        inputs.crouch = Input.GetButton("Crouch");

        CheckGroundStatus();
        float verticalTarget = -1;
        if (m_IsGrounded)
        {
            if (Input.GetButton("Jump"))
            {
                m_jump = true;
            }
            inputs.vertical = 0;
            verticalTarget = 0;
        }
        if (m_jump)
        {
            verticalTarget = 1;
            if (inputs.vertical >= 0.9f)
            {
                m_jump = false;
            }
        }
        inputs.vertical = Mathf.Lerp(inputs.vertical, verticalTarget, 20 * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (m_pausedPanel != null) { m_pausedPanel.SetActive(true); } // Enable the paused panel
            // lock the controls
            InputsSetLock(true);
        }
    }

    Ray cameraRay;
    RaycastHit cameraRayHit;
    private Vector3 m_Mouse = Vector3.zero;
    private float m_angle = 0f;

    float SignedAngle(Vector3 a, Vector3 b ) {
        var angle = Vector3.Angle(a, b);
        return angle * Mathf.Sign(Vector3.Cross(a, b).y);
    }

    public float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n) {
        return Mathf.Atan2(
            Vector3.Dot(n, Vector3.Cross(v1, v2)),
            Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }

    float LookAtMousePosition() {        
        if (Vector3.Distance(Input.mousePosition, m_Mouse) > 1f) {
            cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay, out cameraRayHit)) {
                //if (cameraRayHit.transform.tag == "Ground") {
                    Vector3 targetPosition = new Vector3(cameraRayHit.point.x, transform.position.y, cameraRayHit.point.z);
                    m_angle =  AngleSigned(targetPosition - transform.position, Vector3.forward, Vector3.down);
                    //AngleSigned(transform.position, targetPosition, transform.forward);
                    //Mathf.Sign(Vector3.Dot(Vector3.Cross(Vector3.Cross(red, green), red), transform.forward));
                    //m_angle = SignedAngle(transform.position, targetPosition);
                    //transform.LookAt(targetPosition);
                    m_Mouse = Input.mousePosition;
                //}
            }
        }
        return m_angle;
    }

    // Move, Rotate, UpdatePosition, and UpdateRotation.
    public override Vector3 Move(Inputs inputs, Results current)
    {

        //m_observer.position = current.position;
        float speed = 2;
        if (current.crouching)
        {
            speed = 1.5f;
        }
        if (current.sprinting)
        {
            speed = 3;
        }
        if (inputs.vertical > 0) //&& m_jump
        {
            //m_verticalSpeed = inputs.vertical * m_jumpHeight;
            router.n_rigidbody.velocity = new Vector3(router.n_rigidbody.velocity.x, inputs.vertical * m_jumpHeight, router.n_rigidbody.velocity.z);
        }
        else {
            //m_verticalSpeed = inputs.vertical * Physics.gravity.magnitude;
            Vector3 extraGravityForce = (Physics.gravity * Physics.gravity.magnitude) - Physics.gravity;
            router.n_rigidbody.AddForce(extraGravityForce);

        }

        m_Move = inputs.forward * Vector3.forward + inputs.sides * Vector3.right;
        m_Move *= speed;
        Vector3 v = (m_Move * m_MoveSpeedMultiplier) / Time.deltaTime;
        v.y = router.n_rigidbody.velocity.y;
        router.n_rigidbody.velocity = v;
        return router.n_rigidbody.position;

        //router.playerMovement.Move(m_observer.TransformDirection((Vector3.ClampMagnitude(new Vector3(inputs.sides, 0, inputs.forward), 1) * speed) + new Vector3(0, m_verticalSpeed, 0)) * Time.fixedDeltaTime, current.crouching, m_jump);
        //router.playerMovement.Move(new Vector3(inputs.sides, m_verticalSpeed, inputs.forward), current.crouching, m_jump);
        //return m_observer.position;
    }

    public override Quaternion Rotate(Inputs inputs, Results current)
    {

        ///m_observer.rotation = current.rotation;
        //float mHor = transform.eulerAngles.y + inputs.pitch * Time.fixedDeltaTime;
        float mVert  = Mathf.LerpAngle(transform.eulerAngles.y, inputs.yaw, m_RotateSpeedMultiplier * Time.deltaTime);  //* Time.fixedDeltaTime 

        //transform.rotation = Quaternion.Euler(0, mVert, 0);
        transform.eulerAngles = new Vector3(0, mVert, 0);
        return transform.rotation;
    }

    public override void UpdatePosition(Vector3 newPosition)
    {
        m_observer.position = newPosition;
        //router.n_rigidbody.MovePosition(newPosition);
        /*if (Vector3.Distance(newPosition, m_observer.position) > m_snapDistance)
        {
            m_observer.position = newPosition;
        }
        else {
            //m_observer.position = newPosition;// - m_observer.position;
            router.n_rigidbody.MovePosition(newPosition);// - m_observer.position
        }*/

    }

    public override void UpdateRotation(Quaternion newRotation)
    {
        m_observer.rotation = Quaternion.Euler(0, newRotation.eulerAngles.y, 0);
        //m_observerRotation = newRotation;
    }


    // Helper Rigidbody
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
}
