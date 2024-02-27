using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmobot
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour, DefaultInputActions.IPlayerMovementActions
    {
        public float moveSpeed;
        public float acceleration;
        public float jumpForce;
        public float gravity;
        public float maxFloorAngleDegrees;

        public Transform cameraTransform;

        private Vector3 inputMove = Vector3.zero;
        private bool inputJump;

        private bool isGrounded;
        private Vector3 groundNormal = Vector3.up;
        private float groundCheckRadius;
        private float groundCheckDistance;

        private Rigidbody rb;
        private CapsuleCollider coll;
        private Transform groundCheckOrigin;

        private DefaultInputActions actions;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.freezeRotation = true;
            coll = GetComponent<CapsuleCollider>();
            groundCheckOrigin = transform.Find("GroundCheckOrigin");
            var radius = coll.radius;
            groundCheckRadius = radius * 0.99f; // 0.99 - padding to make spherecast detect floor
            groundCheckDistance = radius;
        }

        private void FixedUpdate()
        {
            GroundCheck();
            ProcessMovement();
        }

        private void ProcessMovement()
        {
            var velocityDelta = CalculateVelocityDelta();

            var shouldJump = inputJump && isGrounded;
            inputJump = false;
            if (shouldJump)
            {
                velocityDelta.y = jumpForce;
            }
            else
            {
                velocityDelta -= gravity * Time.fixedDeltaTime * groundNormal;
            }

            rb.AddForce(velocityDelta, ForceMode.VelocityChange);
        }

        private Vector3 CalculateVelocityDelta()
        {
            var cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;
            var inputDirection = Quaternion.LookRotation(cameraForward) * inputMove;

            var velocity = rb.velocity;
            var targetVelocity = inputDirection * moveSpeed + new Vector3(0, velocity.y, 0);
            return Vector3.MoveTowards(velocity, targetVelocity,
                acceleration * Time.fixedDeltaTime) - rb.velocity;
        }

        private void GroundCheck()
        {
            var origin = groundCheckOrigin.position;

            Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out var hitInfo, groundCheckDistance);
            if (hitInfo.collider != null)
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

        public void OnMovement(InputAction.CallbackContext context)
        {
            var inputMoveRaw = context.action.ReadValue<Vector2>();
            inputMove = new Vector3(inputMoveRaw.x, 0, inputMoveRaw.y);
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                inputJump = true;
            }
        }

        private void OnEnable()
        {
            if (actions == null)
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
    }
}