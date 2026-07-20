using UnityEngine;
using TransformationFPS.Abilities;

namespace TransformationFPS.Player
{
    /// <summary>
    /// Drives the equipped TransformationForm's aerial move - its equivalent of a Destiny class's
    /// jump identity - on pressing Jump while airborne: extra jumps (MultiJump), a slow-fall +
    /// forward-drift float (Glide), or an instant obstacle-clamped forward teleport (Blink).
    /// </summary>
    [RequireComponent(typeof(FirstPersonController))]
    [RequireComponent(typeof(TransformationManager))]
    [RequireComponent(typeof(CharacterController))]
    public class AerialMobilityController : MonoBehaviour
    {
        private FirstPersonController _movement;
        private TransformationManager _transformation;
        private CharacterController _controller;

        private int _airJumpsRemaining;
        private bool _isGliding;
        private float _glideEndTime;
        private float _nextBlinkReadyTime;

        public bool IsGliding => _isGliding;
        public int AirJumpsRemaining => _airJumpsRemaining;
        public float BlinkCooldownRemaining => Mathf.Max(0f, _nextBlinkReadyTime - Time.time);

        private void Awake()
        {
            _movement = GetComponent<FirstPersonController>();
            _transformation = GetComponent<TransformationManager>();
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            var form = _transformation.equippedForm;
            if (form == null) return;

            bool grounded = _movement.IsGrounded;
            if (grounded)
            {
                _airJumpsRemaining = form.extraAirJumps;
                if (_isGliding) EndGlide();
            }

            if (_isGliding && Time.time >= _glideEndTime)
            {
                EndGlide();
            }

            if (Input.GetButtonDown("Jump") && !grounded)
            {
                HandleAerialMoveInput(form);
            }

            if (_isGliding)
            {
                ApplyGlideForwardDrift(form);
            }
        }

        private void HandleAerialMoveInput(TransformationForm form)
        {
            switch (form.aerialMoveType)
            {
                case AerialMoveType.MultiJump:
                    TryAirJump(form);
                    break;
                case AerialMoveType.Glide:
                    TryStartGlide(form);
                    break;
                case AerialMoveType.Blink:
                    TryBlink(form);
                    break;
            }
        }

        private void TryAirJump(TransformationForm form)
        {
            if (_airJumpsRemaining <= 0) return;
            _airJumpsRemaining--;
            _movement.PerformJumpImpulse(form.airJumpHeightMultiplier);
        }

        private void TryStartGlide(TransformationForm form)
        {
            if (_isGliding) return;
            _isGliding = true;
            _glideEndTime = Time.time + form.glideDuration;
            _movement.FallSpeedClamp = form.glideFallSpeed;
        }

        private void EndGlide()
        {
            _isGliding = false;
            _movement.FallSpeedClamp = float.PositiveInfinity;
        }

        private void ApplyGlideForwardDrift(TransformationForm form)
        {
            if (_movement.cameraPivot == null) return;

            Vector3 forward = _movement.cameraPivot.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) return;
            forward.Normalize();

            _controller.Move(forward * form.glideForwardSpeed * Time.deltaTime);
        }

        private void TryBlink(TransformationForm form)
        {
            if (BlinkCooldownRemaining > 0f) return;
            if (_movement.cameraPivot == null) return;

            _nextBlinkReadyTime = Time.time + form.blinkCooldown;

            Vector3 direction = _movement.cameraPivot.forward;
            float distance = form.blinkDistance;

            if (Physics.Raycast(_movement.cameraPivot.position, direction, out RaycastHit hit, distance))
            {
                distance = Mathf.Max(0f, hit.distance - 0.5f);
            }

            _controller.Move(direction * distance);
            _movement.VerticalVelocity = 0f;
        }
    }
}
