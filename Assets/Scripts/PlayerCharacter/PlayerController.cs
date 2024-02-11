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

        private Vector3 input_move = Vector3.zero;
        private bool input_jump = false;
        private Vector3 velocity = Vector3.zero;

        private CharacterController cc;

        private void Start()
        {
            cc = GetComponent<CharacterController>();
        }

        private void Update()
        {
            HandleInput();
        }

        private void FixedUpdate()
        {
            ProcessMovement();
        }
        
        private void HandleInput()
        {
            input_move = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
            input_jump = Input.GetKeyDown(KeyCode.Space) || input_jump;
        }

        private void ProcessMovement()
        {
            velocity = cc.velocity;
            
            Vector3 targetVelocity = input_move * moveSpeed + new Vector3(0, velocity.y, 0);
            velocity = Vector3.MoveTowards(velocity, targetVelocity, acceleration * Time.fixedDeltaTime);

            velocity.y -= gravity * Time.fixedDeltaTime;
            if (input_jump && cc.isGrounded)
            {
                velocity.y = jumpForce;
            }
            input_jump = false;

            cc.Move(velocity * Time.fixedDeltaTime);
        }
    }
}
