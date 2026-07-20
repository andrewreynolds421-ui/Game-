using UnityEngine;

namespace TransformationFPS.Player
{
    /// <summary>
    /// Destiny-style movement: fast base speed, sprint, double-ish jump feel via short air control,
    /// crouch and slide-free simplicity for the MVP. Reads modifiers from TransformationManager
    /// so an active Form can boost speed/jump without this script knowing about abilities.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("References")]
        public Transform cameraPivot;

        [Header("Movement")]
        public float walkSpeed = 6.5f;
        public float sprintSpeed = 10f;
        public float crouchSpeed = 3.5f;
        public float jumpHeight = 1.6f;
        public float gravity = -22f;
        public float airControl = 0.6f;

        [Header("Look")]
        public float mouseSensitivity = 2.2f;
        public float lookPitchLimit = 85f;

        [Header("Crouch")]
        public float standHeight = 1.9f;
        public float crouchHeight = 1.1f;

        // Multipliers applied externally (e.g. by an active TransformationForm)
        [HideInInspector] public float speedMultiplier = 1f;
        [HideInInspector] public float jumpMultiplier = 1f;

        private CharacterController _controller;
        private Vector3 _velocity;
        private float _pitch;
        private bool _isCrouching;

        public bool IsGrounded => _controller.isGrounded;
        public bool IsSprinting { get; private set; }
        public Vector3 HorizontalVelocity { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (cameraPivot == null && Camera.main != null)
            {
                cameraPivot = Camera.main.transform;
            }
        }

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleLook();
            HandleCrouch();
            HandleMovement();
        }

        /// <summary>
        /// Realigns internal look state to a level camera pivot. Used after dismounting a
        /// vehicle, where the camera pivot may have been reoriented by mounted free-look and
        /// would otherwise snap unpredictably on the first post-dismount mouse move.
        /// </summary>
        public void ResetLook(float pitchDegrees = 0f)
        {
            _pitch = pitchDegrees;
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }
        }

        private void HandleLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            _pitch -= mouseY;
            _pitch = Mathf.Clamp(_pitch, -lookPitchLimit, lookPitchLimit);
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }
        }

        private void HandleCrouch()
        {
            _isCrouching = Input.GetKey(KeyCode.C);
            float targetHeight = _isCrouching ? crouchHeight : standHeight;
            _controller.height = Mathf.Lerp(_controller.height, targetHeight, Time.deltaTime * 10f);
            _controller.center = new Vector3(0f, _controller.height / 2f, 0f);
        }

        private void HandleMovement()
        {
            bool grounded = _controller.isGrounded;
            if (grounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
            }

            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            Vector3 inputDir = (transform.right * x + transform.forward * z);
            if (inputDir.sqrMagnitude > 1f)
            {
                inputDir.Normalize();
            }

            IsSprinting = Input.GetKey(KeyCode.LeftShift) && z > 0f && !_isCrouching;
            float targetSpeed = _isCrouching ? crouchSpeed : (IsSprinting ? sprintSpeed : walkSpeed);
            targetSpeed *= speedMultiplier;

            Vector3 desiredMove = inputDir * targetSpeed;
            float control = grounded ? 1f : airControl;
            HorizontalVelocity = Vector3.Lerp(HorizontalVelocity, desiredMove, control * (grounded ? 15f : 5f) * Time.deltaTime);

            if (grounded && Input.GetButtonDown("Jump"))
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * jumpMultiplier * -2f * gravity);
            }

            _velocity.y += gravity * Time.deltaTime;

            Vector3 motion = (HorizontalVelocity + Vector3.up * _velocity.y) * Time.deltaTime;
            _controller.Move(motion);
        }
    }
}
