using System.Collections;
using System.Collections.Generic;
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

        private float rotationSpeed = 900f;

        private Vector3 inputMove = Vector3.zero;
        private bool inputJump = false;

        private bool isGrounded = false;
        private Vector3 groundNormal = Vector3.up;
        private float groundCheckRadius;
        private float groundCheckDistance;

        private Rigidbody rb;
        private CapsuleCollider coll;
        private Transform groundCheckOrigin;
        private Transform cameraTransform;

        DefaultInputActions actions;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.freezeRotation = true;
            coll = GetComponent<CapsuleCollider>();
            groundCheckOrigin = transform.Find("GroundCheckOrigin");
            groundCheckRadius = coll.radius * 0.99f; // 0.99 - padding to make spherecast detect floor
            groundCheckDistance = coll.radius;
            cameraTransform = Camera.main.transform;
        }

        private void FixedUpdate()
        {
            GroundCheck();
            ProcessMovement();
            ProcessRotation();
        }

        private void ProcessMovement()
        {
            Vector3 targetVelocity = inputMove * moveSpeed + new Vector3(0, rb.velocity.y, 0);
            Vector3 velocityDelta = Vector3.MoveTowards(rb.velocity, targetVelocity, 
                acceleration * Time.fixedDeltaTime) - rb.velocity;

            bool shouldJump = inputJump && isGrounded;
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

        private void ProcessRotation()
        {
            if (inputMove.sqrMagnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(inputMove.x, inputMove.z) * Mathf.Rad2Deg;
                float newAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);
                transform.rotation = Quaternion.Euler(0, newAngle, 0);
            }
        }

        private void GroundCheck()
        {
            Vector3 origin = groundCheckOrigin.position;

            Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out RaycastHit hitInfo, groundCheckDistance);
            if (hitInfo.collider != null)
            {
                groundNormal = hitInfo.normal;
                float floorAngleDegrees = Mathf.Acos(Vector3.Dot(Vector3.up, groundNormal)) * Mathf.Rad2Deg;
                isGrounded = floorAngleDegrees <= maxFloorAngleDegrees;  
            }
            else
            {
                isGrounded = false;
            }
            
            if (!isGrounded) groundNormal = Vector3.up;

        }

        // Input stuff
        public void OnMovement(InputAction.CallbackContext context)
        {
            Vector2 inputMoveRaw = context.action.ReadValue<Vector2>();
            inputMove = new Vector3(inputMoveRaw.x, 0, inputMoveRaw.y);
            inputMove = Quaternion.AngleAxis(cameraTransform.eulerAngles.y, Vector3.up) * inputMove;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                inputJump = true;
            }
            
        }

        void OnEnable()
        {
            if (actions == null)
            {
                actions = new DefaultInputActions();
                actions.PlayerMovement.SetCallbacks(this);
            }
            actions.PlayerMovement.Enable();
        }

        void OnDisable()
        {
            actions.PlayerMovement.Disable();
        }
    }
}
