using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed;
        public float acceleration;
        public float jumpForce;
        public float gravity;
        public float maxFloorAngleDegrees;

        private Vector3 inputMove = Vector3.zero;
        private bool inputJump = false;

        private bool isGrounded = false;
        private Vector3 groundNormal = Vector3.up;

        private Rigidbody rb;
        private Transform groundCheckOrigin;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            groundCheckOrigin = transform.Find("GroundCheckOrigin");
        }

        private void Update()
        {
            HandleInput();
        }

        private void FixedUpdate()
        {
            GroundCheck();
            ProcessMovement();
        }
        
        private void HandleInput()
        {
            inputMove = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
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

        private void GroundCheck()
        {
            float radius = 0.45f;
            Vector3 origin = groundCheckOrigin.position;
            RaycastHit hitInfo;
            Physics.SphereCast(origin, radius, Vector3.down, out hitInfo, 0.6f);
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
