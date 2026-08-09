using LilyOfValley.Inputs;
using UnityEngine;

namespace LilyOfValley.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        #region Field

        private const float MaxDirectionMagnitudeSquared = 1f;
        private const float JumpVelocityGravityFactor = -2f;

        [SerializeField] private InputReader inputReader;

        [SerializeField] private float walkSpeed = 4.5f;

        [SerializeField] private float sprintSpeed = 8f;

        [SerializeField] private float acceleration = 14f;

        [SerializeField] private float jumpHeight = 1.2f;

        [SerializeField] private float gravity = -22f;

        [SerializeField] private float groundStickForce = -3f;

        [SerializeField] private float jumpBufferSeconds = 0.12f;

        [SerializeField] private float coyoteSeconds = 0.1f;

        private CharacterController _controller;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private float _jumpBufferTimer;
        private float _coyoteTimer;

        #endregion

        #region Property

        public bool IsGrounded => this._controller.isGrounded;

        public float CurrentSpeed => this._horizontalVelocity.magnitude;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            this._controller = GetComponent<CharacterController>();

            if (this.inputReader != null) return;

            Debug.LogError($"{nameof(PlayerMotor)}: {nameof(this.inputReader)} is not assigned.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (this.inputReader == null) return;

            this.inputReader.JumpPressed += OnJumpPressed;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            UpdateTimers(deltaTime);
            UpdateHorizontalVelocity(deltaTime);
            UpdateVerticalVelocity(deltaTime);

            var velocity = this._horizontalVelocity;
            velocity.y = this._verticalVelocity;
            this._controller.Move(velocity * deltaTime);
        }

        private void OnDisable()
        {
            if (this.inputReader == null) return;

            this.inputReader.JumpPressed -= OnJumpPressed;
        }

        #endregion

        #region Movement

        private void UpdateTimers(float deltaTime)
        {
            this._jumpBufferTimer -= deltaTime;
            this._coyoteTimer = this._controller.isGrounded ? this.coyoteSeconds : this._coyoteTimer - deltaTime;
        }

        private void UpdateHorizontalVelocity(float deltaTime)
        {
            var input = this.inputReader.MoveInput;
            var direction = (transform.right * input.x) + (transform.forward * input.y);
            if (direction.sqrMagnitude > MaxDirectionMagnitudeSquared) direction.Normalize();

            var speed = this.inputReader.IsSprinting ? this.sprintSpeed : this.walkSpeed;
            var target = direction * speed;
            this._horizontalVelocity = Vector3.MoveTowards(this._horizontalVelocity, target, this.acceleration * deltaTime);
        }

        private void UpdateVerticalVelocity(float deltaTime)
        {
            if (this._controller.isGrounded && this._verticalVelocity < 0f) this._verticalVelocity = this.groundStickForce;

            if (this._jumpBufferTimer > 0f && this._coyoteTimer > 0f)
            {
                this._verticalVelocity = Mathf.Sqrt(this.jumpHeight * JumpVelocityGravityFactor * this.gravity);
                this._jumpBufferTimer = 0f;
                this._coyoteTimer = 0f;
            }

            this._verticalVelocity += this.gravity * deltaTime;
        }

        #endregion

        #region Method

        private void OnJumpPressed() => this._jumpBufferTimer = this.jumpBufferSeconds;

        #endregion
    }
}
