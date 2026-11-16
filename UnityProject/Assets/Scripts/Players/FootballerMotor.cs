using UnityEngine;

namespace Zakareya.DreamFootball.Players
{
    /// <summary>
    /// Lightweight humanoid locomotion motor. A mobile UI or input adapter supplies
    /// the normalized movement vector through SetMoveInput.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class FootballerMotor : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float walkSpeed = 3.2f;
        [SerializeField, Min(0f)] private float sprintSpeed = 5.8f;
        [SerializeField, Min(0f)] private float turnSharpness = 12f;
        [SerializeField] private float gravity = -20f;

        [Header("References")]
        [SerializeField] private Transform movementCamera;
        [SerializeField] private Animator animator;

        private CharacterController controller;
        private Vector2 moveInput;
        private bool sprintRequested;
        private float verticalVelocity;

        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        private static readonly int IsSprinting = Animator.StringToHash("IsSprinting");

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        /// <summary>Supply joystick input in the range -1..1 on each axis.</summary>
        public void SetMoveInput(Vector2 value) => moveInput = Vector2.ClampMagnitude(value, 1f);

        /// <summary>Call from a touch sprint button's press/release events.</summary>
        public void SetSprint(bool value) => sprintRequested = value;

        private void Update()
        {
            Vector3 forward = movementCamera != null
                ? Vector3.ProjectOnPlane(movementCamera.forward, Vector3.up).normalized
                : Vector3.forward;
            Vector3 right = movementCamera != null
                ? Vector3.ProjectOnPlane(movementCamera.right, Vector3.up).normalized
                : Vector3.right;

            Vector3 direction = Vector3.ClampMagnitude(
                right * moveInput.x + forward * moveInput.y, 1f);

            bool moving = direction.sqrMagnitude > 0.001f;
            bool sprinting = moving && sprintRequested;
            float speed = sprinting ? sprintSpeed : walkSpeed;

            if (moving)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRotation, turnSharpness * Time.deltaTime);
            }

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = direction * speed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);

            if (animator != null)
            {
                animator.SetFloat(MoveSpeed, moving ? (sprinting ? 1f : 0.55f) : 0f, 0.1f, Time.deltaTime);
                animator.SetBool(IsSprinting, sprinting);
            }
        }
    }
}
