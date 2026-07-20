using UnityEngine;

namespace TransformationFPS.Vehicles
{
    /// <summary>
    /// Destiny-Sparrow-equivalent hover vehicle: a spring/damper raycast keeps it floating at a
    /// fixed height above whatever terrain is beneath it, forward thrust and yaw torque drive it,
    /// and a separate stabilization torque keeps it upright regardless of terrain slope.
    /// Drive input is only applied while <see cref="IsPlayerControlled"/> is true; otherwise the
    /// vehicle just idles in place, hovering, which is also its resting state after a dismount.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class HoverVehicleController : MonoBehaviour
    {
        [Header("Body")]
        public float mass = 220f;
        public float linearDrag = 1.5f;
        public float angularDrag = 4f;

        [Header("Hover")]
        public float hoverHeight = 1.4f;
        public float hoverForce = 60f;
        public float hoverDamping = 6f;
        public LayerMask groundMask = ~0;
        public float maxHoverRayDistance = 6f;

        [Header("Drive")]
        public float acceleration = 40f;
        public float boostAcceleration = 90f;
        public float maxSpeed = 28f;
        public float boostMaxSpeed = 48f;
        public float turnTorque = 25f;
        public float uprightTorque = 10f;

        public bool IsPlayerControlled { get; set; }
        public float CurrentSpeed => _rb != null ? new Vector3(_rb.velocity.x, 0f, _rb.velocity.z).magnitude : 0f;

        private Rigidbody _rb;
        private float _throttleInput;
        private float _steerInput;
        private bool _boosting;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.mass = mass;
            _rb.drag = linearDrag;
            _rb.angularDrag = angularDrag;
            _rb.centerOfMass = new Vector3(0f, -0.3f, 0f);
        }

        public void SetDriveInput(float throttle, float steer, bool boost)
        {
            _throttleInput = Mathf.Clamp(throttle, -1f, 1f);
            _steerInput = Mathf.Clamp(steer, -1f, 1f);
            _boosting = boost;
        }

        private void FixedUpdate()
        {
            ApplyHover();
            if (IsPlayerControlled)
            {
                ApplyDrive();
            }
            ApplyUprightStabilization();
        }

        private void ApplyHover()
        {
            // Rigidbody.useGravity already pulls it down every FixedUpdate; when there's no
            // ground within range, that default gravity is all that should apply here.
            if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxHoverRayDistance, groundMask))
            {
                return;
            }

            float compression = hoverHeight - hit.distance;
            float springForce = compression * hoverForce;
            float damping = -_rb.velocity.y * hoverDamping;
            _rb.AddForce(Vector3.up * (springForce + damping), ForceMode.Acceleration);
        }

        private void ApplyDrive()
        {
            float accel = _boosting ? boostAcceleration : acceleration;
            float speedCap = _boosting ? boostMaxSpeed : maxSpeed;

            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            if (CurrentSpeed < speedCap || _throttleInput < 0f)
            {
                _rb.AddForce(forward * (_throttleInput * accel), ForceMode.Acceleration);
            }

            _rb.AddTorque(Vector3.up * (_steerInput * turnTorque), ForceMode.Acceleration);
        }

        private void ApplyUprightStabilization()
        {
            // Rotating transform.up onto world up (and nothing else) leaves yaw free, so this
            // never fights the steering torque above - it only corrects pitch/roll from terrain slope.
            Quaternion correction = Quaternion.FromToRotation(transform.up, Vector3.up);
            correction.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;
            if (Mathf.Abs(angle) > 0.01f)
            {
                _rb.AddTorque(axis.normalized * (angle * Mathf.Deg2Rad * uprightTorque), ForceMode.Acceleration);
            }
        }
    }
}
