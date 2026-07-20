using UnityEngine;

namespace TransformationFPS.Player
{
    /// <summary>
    /// Destiny-style movement: fast base speed, sprint, sprint-slide, and a small public API
    /// (<see cref="VerticalVelocity"/>, <see cref="FallSpeedClamp"/>, <see cref="PerformJumpImpulse"/>,
    /// <see cref="MovementSuppressed"/>) that lets other systems - per-Form aerial moves, ledge
    /// mantling - drive or momentarily take over movement without duplicating its velocity/gravity/
    /// collision handling. Reads speed/jump multipliers from TransformationManager so an active
    /// Form can boost them without this script knowing about abilities.
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

        [Header("Slide")]
        public float slideSpeed = 12f;
        public float slideDuration = 0.6f;
        public float slideDeceleration = 14f;

        // Multipliers applied externally (e.g. by an active TransformationForm)
        [HideInInspector] public float speedMultiplier = 1f;
        [HideInInspector] public float jumpMultiplier = 1f;

        /// <summary>Max fall speed magnitude; other systems (e.g. a Glide aerial move) can clamp this temporarily.</summary>
        public float FallSpeedClamp { get; set; } = float.PositiveInfinity;

        /// <summary>While true, HandleMovement's WASD/gravity/jump/slide logic is skipped entirely, so an
        /// external system (e.g. ledge mantling) can drive the CharacterController directly. Mouse look
        /// still runs normally.</summary>
        public bool MovementSuppressed { get; set; }

        public float VerticalVelocity
        {
            get => _velocity.y;
            set => _velocity.y = value;
        }

        private CharacterController _controller;
        private Vector3 _velocity;
        private float _pitch;
        private bool _isCrouching;
        private bool _isSliding;
        private float _slideTimer;

        public bool IsGrounded => _controller.isGrounded;
        public bool IsSprinting { get; private set; }
        public bool IsSliding => _isSliding;
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

        /// <summary>Sets vertical velocity to the standard jump-arc impulse for the given height multiplier.
        /// Used for both the ground jump and per-Form air jumps, so they share one formula.</summary>
        public void PerformJumpImpulse(float heightMultiplier)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * heightMultiplier * jumpMultiplier * -2f * gravity);
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
            bool crouchHeld = Input.GetKey(KeyCode.C);

            if (Input.GetKeyDown(KeyCode.C) && IsSprinting && _controller.isGrounded && !_isSliding)
            {
                StartSlide();
            }
            else if (_isSliding && !crouchHeld)
            {
                EndSlide(); // releasing crouch early cancels the slide
            }

            _isCrouching = crouchHeld || _isSliding;
            float targetHeight = _isCrouching ? crouchHeight : standHeight;
            _controller.height = Mathf.Lerp(_controller.height, targetHeight, Time.deltaTime * 10f);
            _controller.center = new Vector3(0f, _controller.height / 2f, 0f);
        }

        private void StartSlide()
        {
            _isSliding = true;
            _slideTimer = slideDuration;
            Vector3 direction = HorizontalVelocity.sqrMagnitude > 0.01f ? HorizontalVelocity.normalized : transform.forward;
            HorizontalVelocity = direction * slideSpeed;
        }

        private void EndSlide()
        {
            _isSliding = false;
        }

        private void HandleMovement()
        {
            if (MovementSuppressed) return;

            bool grounded = _controller.isGrounded;
            if (grounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
            }

            if (_isSliding)
            {
                UpdateSlide();
            }
            else
            {
                UpdateWalkRun(grounded);
            }

            if (grounded && Input.GetButtonDown("Jump"))
            {
                if (_isSliding) EndSlide();
                PerformJumpImpulse(1f);
            }

            _velocity.y += gravity * Time.deltaTime;
            if (_velocity.y < -FallSpeedClamp)
            {
                _velocity.y = -FallSpeedClamp;
            }

            Vector3 motion = (HorizontalVelocity + Vector3.up * _velocity.y) * Time.deltaTime;
            _controller.Move(motion);
        }

        private void UpdateWalkRun(bool grounded)
        {
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
        }

        private void UpdateSlide()
        {
            _slideTimer -= Time.deltaTime;
            HorizontalVelocity = Vector3.MoveTowards(HorizontalVelocity, Vector3.zero, slideDeceleration * Time.deltaTime);
            if (_slideTimer <= 0f || HorizontalVelocity.magnitude < crouchSpeed)
            {
                EndSlide();
            }
        }
    }
}
