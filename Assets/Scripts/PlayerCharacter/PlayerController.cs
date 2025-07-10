using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmobot
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour, DefaultInputActions.IPlayerMovementActions
    {
        public enum RotationMode
        {
            MovementDirection,
            CameraDirection
        }

        private static readonly int AnimatorSpeed = Animator.StringToHash("speed");
        private static readonly int AnimatorJump = Animator.StringToHash("jump");

        [SerializeField]
        private float moveSpeed;

        [SerializeField]
        private float acceleration;

        [SerializeField]
        private float jumpForce;

        [SerializeField]
        private float gravity;

        [SerializeField]
        private float maxFloorAngleDegrees;

        [SerializeField]
        private float playerRotationSpeed;

        [SerializeField]
        private RotationMode rotationMode;

        [SerializeField]
        private Transform cameraTransform;

        [SerializeField]
        private Transform groundCheckOrigin;

        [SerializeField]
        private Animator animator;

        private DefaultInputActions actions;
        private float groundCheckDistance;
        private float groundCheckRadius;
        private Vector3 groundNormal = Vector3.up;
        private Vector3 inputDirection = Vector3.zero;
        private bool inputJump;

        private Vector3 inputMove = Vector3.zero;

        private bool isGrounded;

        private Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.freezeRotation = true;
            var coll = GetComponent<CapsuleCollider>();
            var radius = coll.radius;
            groundCheckRadius = radius * 0.99f; // 0.99 - padding to make spherecast detect floor
            groundCheckDistance = radius;
        }

        private void Update()
        {
            ProcessRotation();
        }

        private void FixedUpdate()
        {
            GroundCheck();
            ProcessMovement();
        }

        private void OnEnable()
        {
            if (actions is null)
            {
                actions = new DefaultInputActions();
                actions.PlayerMovement.SetCallbacks(this);
            }

            actions.PlayerMovement.Enable();
        }

        private void OnDisable()
        {
            actions.PlayerMovement.Disable();
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            var inputMoveRaw = context.action.ReadValue<Vector2>();
            inputMove = new Vector3(inputMoveRaw.x, 0, inputMoveRaw.y);
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            inputJump = context.performed;
        }

        private void ProcessMovement()
        {
            var velocityDelta = CalculateVelocityDelta();

            var shouldJump = inputJump && isGrounded;
            inputJump = false;
            if (shouldJump)
            {
                velocityDelta.y = jumpForce;
                animator.SetTrigger(AnimatorJump);
            }
            else
            {
                velocityDelta -= gravity * Time.fixedDeltaTime * groundNormal;
            }

            rb.AddForce(velocityDelta, ForceMode.VelocityChange);
            animator.SetFloat(AnimatorSpeed, new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).sqrMagnitude);
        }

        private Vector3 CalculateVelocityDelta()
        {
            var cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;
            inputDirection = Quaternion.LookRotation(cameraForward) * inputMove;

            var targetVelocity = inputDirection * moveSpeed + new Vector3(0, rb.linearVelocity.y, 0);
            var velocityDelta = Vector3.MoveTowards(rb.linearVelocity, targetVelocity,
                acceleration * Time.fixedDeltaTime) - rb.linearVelocity;
            return velocityDelta;
        }

        private void GroundCheck()
        {
            var origin = groundCheckOrigin.position;

            Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out var hitInfo, groundCheckDistance);
            if (hitInfo.collider is not null)
            {
                groundNormal = hitInfo.normal;
                var floorAngleDegrees = Mathf.Acos(Vector3.Dot(Vector3.up, groundNormal)) * Mathf.Rad2Deg;
                isGrounded = floorAngleDegrees <= maxFloorAngleDegrees;
            }
            else
            {
                isGrounded = false;
            }

            if (!isGrounded) groundNormal = Vector3.up;
        }

        public void SetRotationMode(RotationMode mode)
        {
            rotationMode = mode;
        }

        private void ProcessRotation()
        {
            if (rotationMode == RotationMode.MovementDirection)
            {
                if (inputDirection.sqrMagnitude <= 0.01f) return;
                var toRotation = Quaternion.LookRotation(inputDirection);
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, toRotation, playerRotationSpeed * Time.deltaTime);
            }
            else
            {
                var faceDirection = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
                transform.rotation = Quaternion.LookRotation(faceDirection);
            }
        }
    }
}
