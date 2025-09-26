using UnityEngine;

namespace AngryDogs.Player
{
    /// <summary>
    /// Handles forward sprinting and lane-based dodging for Riley.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovementController : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField] private float forwardSpeed = 12f;
        [SerializeField] private float lateralSpeed = 8f;
        [SerializeField] private float gravity = -30f;

        [Header("Lane Settings")]
        [SerializeField] private float laneWidth = 3f;
        [SerializeField] private int laneCount = 3;
        [SerializeField, Tooltip("Smoothing factor for lateral lerp.")]
        private float laneLerp = 12f;

        private CharacterController _controller;
        private Vector2 _cachedInput;
        private float _verticalVelocity;
        private float _targetLaneOffset;
        private float _currentLaneOffset;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        public void ProcessInput(Vector2 input)
        {
            _cachedInput = input;
        }

        private void Update()
        {
            MoveForward();
            ApplyGravity();
        }

        private void MoveForward()
        {
            _targetLaneOffset += _cachedInput.x * laneWidth * Time.deltaTime;
            _targetLaneOffset = Mathf.Clamp(_targetLaneOffset, -LaneHalfWidth(), LaneHalfWidth());
            _currentLaneOffset = Mathf.Lerp(_currentLaneOffset, _targetLaneOffset, 1f - Mathf.Exp(-laneLerp * Time.deltaTime));

            var targetPosition = transform.TransformPoint(new Vector3(_currentLaneOffset, 0f, 0f));
            var lateralDirection = (targetPosition - transform.position);
            lateralDirection.y = 0f;

            var lateralVelocity = lateralDirection.normalized * lateralSpeed;
            var forwardVelocity = transform.forward * forwardSpeed;

            var displacement = (forwardVelocity + lateralVelocity) * Time.deltaTime;
            displacement.y = _verticalVelocity * Time.deltaTime;
            _controller.Move(displacement);
        }

        private void ApplyGravity()
        {
            if (_controller.isGrounded)
            {
                _verticalVelocity = -1f; // small negative to keep grounded
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }
        }

        private float LaneHalfWidth()
        {
            var totalWidth = (laneCount - 1) * laneWidth;
            return totalWidth * 0.5f;
        }
    }
}
