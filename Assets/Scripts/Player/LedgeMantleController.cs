using UnityEngine;

namespace TransformationFPS.Player
{
    /// <summary>
    /// Auto-mantles over ledges within reach while walking forward into them: a chest-height
    /// raycast detects a wall (checked against its normal so shallow terrain slopes don't
    /// misfire this as a "wall"), a raycast down from above the wall finds the ledge surface,
    /// and if there's headroom to stand there, the player is smoothly moved up onto it.
    /// </summary>
    [RequireComponent(typeof(FirstPersonController))]
    [RequireComponent(typeof(CharacterController))]
    public class LedgeMantleController : MonoBehaviour
    {
        [Header("Detection")]
        public float forwardCheckDistance = 0.7f;
        public float chestHeight = 1.0f;
        public float minMantleHeight = 0.4f;
        public float maxMantleHeight = 1.9f;
        public float clearanceRadius = 0.3f;
        [Tooltip("A hit surface with an up-facing normal above this is treated as ground/slope, not a mantleable wall.")]
        public float maxWallUpNormal = 0.5f;
        public LayerMask obstacleMask = ~0;

        [Header("Motion")]
        public float mantleDuration = 0.35f;

        public bool IsMantling { get; private set; }

        private FirstPersonController _movement;
        private CharacterController _controller;

        private float _mantleTimer;
        private Vector3 _mantleStart;
        private Vector3 _mantleTarget;

        private void Awake()
        {
            _movement = GetComponent<FirstPersonController>();
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (IsMantling)
            {
                UpdateMantle();
                return;
            }

            if (!_movement.IsGrounded) return;

            float z = Input.GetAxisRaw("Vertical");
            if (z <= 0.1f) return; // only auto-mantle while moving forward

            TryStartMantle();
        }

        private void TryStartMantle()
        {
            Vector3 feet = transform.position;
            Vector3 chestOrigin = feet + Vector3.up * chestHeight;

            if (!Physics.Raycast(chestOrigin, transform.forward, out RaycastHit wallHit, forwardCheckDistance, obstacleMask))
            {
                return;
            }
            if (Vector3.Dot(wallHit.normal, Vector3.up) > maxWallUpNormal)
            {
                return; // shallow slope, not a wall - let normal CharacterController movement handle it
            }

            Vector3 aboveOrigin = feet + Vector3.up * maxMantleHeight + transform.forward * (forwardCheckDistance + 0.1f);
            if (!Physics.Raycast(aboveOrigin, Vector3.down, out RaycastHit ledgeHit, maxMantleHeight, obstacleMask))
            {
                return;
            }

            float ledgeHeight = ledgeHit.point.y - feet.y;
            if (ledgeHeight < minMantleHeight || ledgeHeight > maxMantleHeight)
            {
                return;
            }

            Vector3 landSpot = ledgeHit.point + Vector3.up * 0.05f;
            if (Physics.CheckSphere(landSpot + Vector3.up * clearanceRadius, clearanceRadius, obstacleMask))
            {
                return; // not enough headroom to stand there
            }

            StartMantle(landSpot);
        }

        private void StartMantle(Vector3 target)
        {
            IsMantling = true;
            _mantleTimer = 0f;
            _mantleStart = transform.position;
            _mantleTarget = target;

            _movement.MovementSuppressed = true;
            _movement.VerticalVelocity = 0f;
        }

        private void UpdateMantle()
        {
            _mantleTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_mantleTimer / mantleDuration);

            Vector3 next = Vector3.Lerp(_mantleStart, _mantleTarget, t);
            _controller.Move(next - transform.position);

            if (t >= 1f)
            {
                IsMantling = false;
                _movement.MovementSuppressed = false;
            }
        }
    }
}
