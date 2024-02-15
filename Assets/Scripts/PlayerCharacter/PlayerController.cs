using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
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

        private Rigidbody rb;
        private Transform groundCheckOrigin;
        private Transform cameraTransform;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.freezeRotation = true;
            groundCheckOrigin = transform.Find("GroundCheckOrigin");
            cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            HandleInput();
        }

        private void FixedUpdate()
        {
            GroundCheck();
            ProcessMovement();
            ProcessRotation();
        }
        
        private void HandleInput()
        {
            inputMove = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
            inputMove = Quaternion.AngleAxis(cameraTransform.eulerAngles.y, Vector3.up) * inputMove;
            inputJump = Input.GetKeyDown(KeyCode.Space) || inputJump;
        }

        private void ProcessMovement()
        {
            Vector3 targetVelocity = inputMove * moveSpeed + new Vector3(0, rb.velocity.y, 0);
            Vector3 neededForce = Vector3.MoveTowards(rb.velocity, targetVelocity, 
                acceleration * Time.fixedDeltaTime) - rb.velocity;

            bool shouldJump = inputJump && isGrounded;
            inputJump = false;
            neededForce.y = shouldJump ? jumpForce : neededForce.y - gravity * Time.fixedDeltaTime;
            
            rb.AddForce(neededForce, ForceMode.VelocityChange);
        }

        private void ProcessRotation()
        {
            if (inputMove.sqrMagnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(inputMove.x, inputMove.z) * Mathf.Rad2Deg;
                float newAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);
                transform.rotation = Quaternion.Euler(0, newAngle, 0);
                Debug.Log(targetAngle);
            }
        }

        private void GroundCheck()
        {
            float radius = 0.45f;
            Vector3 origin = groundCheckOrigin.position;
            float cast_distance = 0.6f;

            RaycastHit hitInfo;
            Physics.SphereCast(origin, radius, Vector3.down, out hitInfo, cast_distance);
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
            
        }
    }
}
