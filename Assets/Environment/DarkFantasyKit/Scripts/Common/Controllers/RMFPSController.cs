namespace Runemark.Common.Gameplay
{
    using UnityEngine;
    
    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    public class RMFPSController : MonoBehaviour
    {       
        public float mouseSensitivityX = 1.0f;
        public float mouseSensitivityY = 1.0f;
        public float walkSpeed = 10.0f;
        public float JumpForce = 250.0f;
        public float groundDistance;
        public float deltaGround = 0.2f;
        public LayerMask GroundedMask;

        // Orientation state.
        Quaternion _initialOrientation;

        // Cached cursor state.
        CursorLockMode _previousLockState;
        bool _wasCursorVisible;



        Vector3 _moveAmount;
        Vector3 _smoothMoveVelocity;
 public Vector3 gravitationalDirection;
 public float gravity = 10;
        Transform _cameraTransform;
        float _verticalLookRotation;

        Rigidbody rb;

        
        bool _grounded;
        bool _cursorVisible;

        private Vector3 characterNormal; //to be used in determining the ground
        private Vector3 groundNormal; //to collect surface normal


        // Use this for initialization
        void Start()
        {
            LockMouse();
            characterNormal = transform.up;
            _cameraTransform = GetComponentInChildren<Camera>().transform;
            rb = GetComponent<Rigidbody>();
           
        }

        // Update is called once per frame
        void Update()
        {

            // rotation
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivityX);
            _verticalLookRotation += Input.GetAxis("Mouse Y") * mouseSensitivityY;
            _verticalLookRotation = Mathf.Clamp(_verticalLookRotation, -60, 60);
            _cameraTransform.localEulerAngles = Vector3.left * _verticalLookRotation;

            // movement
            Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
            Vector3 targetMoveAmount = moveDir * walkSpeed;
            _moveAmount = Vector3.SmoothDamp(_moveAmount, targetMoveAmount, ref _smoothMoveVelocity, .15f);

            // jump
            if (Input.GetButtonDown("Jump"))
            {
                if (_grounded)
                {
                    rb.AddForce(transform.up * JumpForce);
                    
                }
            }

            if (Input.GetMouseButtonUp(1)){
                gravitationalDirection = _cameraTransform.forward;
                rb.transform.Rotate(_cameraTransform.up);
                //Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

                // Draw a ray pointing at our target in
                //Debug.DrawRay(transform.position, newDirection, Color.red);

                // Calculate a rotation a step closer to the target and applies rotation to this object
                //transform.rotation = Quaternion.LookRotation(newDirection);

            }

            Ray ray = new Ray(transform.position, -characterNormal); //for checking ground
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1 + .1f, GroundedMask))
            {
                _grounded = true;
                groundNormal = hit.normal;
            }
            else
            {
                _grounded = false;
                groundNormal = Vector3.up;
            }

            /* Lock/unlock mouse on click */
            if (Input.GetKeyDown(KeyCode.K))
            {
                if (!_cursorVisible)
                {
                    UnlockMouse();
                }
                else
                {
                    LockMouse();
                }
            }
        }

        void FixedUpdate()
        {
            rb.MovePosition(rb.position + transform.TransformDirection(_moveAmount) * Time.fixedDeltaTime);
            //apply's gravity towards the ground based on normal
            rb.AddForce(-gravity * rb.mass * characterNormal);
        }

        void UnlockMouse()
        {
            // When switched off, put everything back the way we found it.
            Cursor.visible = _wasCursorVisible;
            Cursor.lockState = _previousLockState;
            transform.localRotation = _initialOrientation;
        }

        void LockMouse()
        {
            // Cache our starting orientation as our center point.
            _initialOrientation = transform.localRotation;

            // Cache the previous cursor state so we can restore it later.
            _previousLockState = Cursor.lockState;
            _wasCursorVisible = Cursor.visible;

            // Hide & lock the cursor for that FPS experience
            // and to avoid distractions / accidental clicks
            // from the mouse cursor moving around.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    #if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RMFPSController), true)]
    public class RMFPSControllerEditor : CustomInspectorBase
    {   
        protected override string Title { get { return "First Person Controller"; } }
        protected override string Description { get { return "This is a simple FPS controller."; } }
    }
    #endif
}
