using UnityEngine;
using TransformationFPS.Vehicles;

namespace TransformationFPS.Player
{
    /// <summary>
    /// Destiny-Sparrow-style summon/mount/dismount. On mount, the player is reparented under the
    /// vehicle (rather than just moving the camera) so everything that reads the player's world
    /// transform - terrain streaming's target, enemy targeting via the "Player" tag, etc. - keeps
    /// working automatically without extra per-frame sync code. While mounted, on-foot movement
    /// and the weapon are disabled, A/D steer and W/S throttle the vehicle, and the mouse freely
    /// looks around independently of the vehicle's heading (like a driving-game chase view, just
    /// first-person).
    /// </summary>
    [RequireComponent(typeof(FirstPersonController))]
    [RequireComponent(typeof(CharacterController))]
    public class VehicleMountController : MonoBehaviour
    {
        [Header("Input")]
        public KeyCode summonKey = KeyCode.V;
        public KeyCode boostKey = KeyCode.LeftShift;

        [Header("Mount")]
        public float summonForwardOffset = 3f;
        public float dismountForwardOffset = 2.5f;
        public Vector3 seatLocalPosition = new Vector3(0f, 0.55f, 0f);

        [Header("Mounted Look")]
        public float lookSensitivity = 2.2f;
        public float lookPitchLimit = 80f;

        public bool IsMounted { get; private set; }
        public HoverVehicleController ActiveVehicle { get; private set; }

        private FirstPersonController _movement;
        private CharacterController _characterController;
        private WeaponController _weapon;
        private Transform _cameraPivot;

        private float _mountedYaw;
        private float _mountedPitch;

        private void Awake()
        {
            _movement = GetComponent<FirstPersonController>();
            _characterController = GetComponent<CharacterController>();
            _weapon = GetComponent<WeaponController>();
            _cameraPivot = _movement.cameraPivot;
        }

        private void Update()
        {
            if (Input.GetKeyDown(summonKey))
            {
                if (IsMounted) Dismount();
                else Mount();
            }

            if (IsMounted && ActiveVehicle != null)
            {
                float throttle = Input.GetAxisRaw("Vertical");
                float steer = Input.GetAxisRaw("Horizontal");
                bool boost = Input.GetKey(boostKey);
                ActiveVehicle.SetDriveInput(throttle, steer, boost);

                HandleMountedLook();
            }
        }

        private void Mount()
        {
            if (ActiveVehicle == null)
            {
                ActiveVehicle = VehicleFactory.CreateHoverVehicle(transform.position + transform.forward * summonForwardOffset, transform.rotation);
            }
            else
            {
                ActiveVehicle.transform.SetPositionAndRotation(transform.position + transform.forward * summonForwardOffset, transform.rotation);
                var rb = ActiveVehicle.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            ActiveVehicle.IsPlayerControlled = true;

            _movement.enabled = false;
            _characterController.enabled = false;
            if (_weapon != null) _weapon.enabled = false;

            transform.SetParent(ActiveVehicle.transform, false);
            transform.localPosition = seatLocalPosition;
            transform.localRotation = Quaternion.identity;

            _mountedYaw = 0f;
            _mountedPitch = 0f;
            if (_cameraPivot != null)
            {
                _cameraPivot.localRotation = Quaternion.identity;
            }

            IsMounted = true;
        }

        private void Dismount()
        {
            if (ActiveVehicle == null) return;

            ActiveVehicle.IsPlayerControlled = false;
            ActiveVehicle.SetDriveInput(0f, 0f, false);

            Vector3 vehicleForward = ActiveVehicle.transform.forward;
            vehicleForward.y = 0f;
            vehicleForward.Normalize();

            Vector3 candidate = ActiveVehicle.transform.position - vehicleForward * dismountForwardOffset;
            Vector3 rayStart = candidate + Vector3.up * 20f;
            candidate.y = Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 100f)
                ? hit.point.y + 0.05f
                : ActiveVehicle.transform.position.y;

            Quaternion dismountRotation = Quaternion.Euler(0f, ActiveVehicle.transform.eulerAngles.y, 0f);

            transform.SetParent(null, true);
            transform.SetPositionAndRotation(candidate, dismountRotation);

            _characterController.enabled = true;
            _movement.ResetLook(0f);
            _movement.enabled = true;
            if (_weapon != null) _weapon.enabled = true;

            IsMounted = false;
        }

        private void HandleMountedLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

            _mountedYaw += mouseX;
            _mountedPitch = Mathf.Clamp(_mountedPitch - mouseY, -lookPitchLimit, lookPitchLimit);

            if (_cameraPivot != null)
            {
                _cameraPivot.localRotation = Quaternion.Euler(_mountedPitch, _mountedYaw, 0f);
            }
        }
    }
}
