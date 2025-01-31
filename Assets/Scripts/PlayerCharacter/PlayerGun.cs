using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmobot
{
    public class PlayerGun : MonoBehaviour, DefaultInputActions.IPlayerGunActions
    {
        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private Transform cameraTransform;
        [SerializeField, Tooltip("Relative to character, not camera")] private float maxPickupRange;
        [SerializeField] private float carryHoldingDistance;
        [SerializeField] private float holdingDistanceZoomMultiplier = 1f;
        [SerializeField] private Vector3 carryPositionFirstPersonOffset;
        [SerializeField] private Vector3 carryPositionThirdPersonOffset;
        [SerializeField] private float carryForceMultiplier = 1f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private LayerMask itemLayer;

        private bool carryingItem = false;
        private Transform carriedItemTransform = null;
        private Rigidbody carriedItemBody = null;

        private Vector3 carryPosition;

        private DefaultInputActions actions;

        private void LateUpdate()
        {
            UpdateModel();
        }

        private void FixedUpdate()
        {
            ProcessCarrying();
        }

        private void ProcessCarrying()
        {
            if (carryingItem)
            {
                if (IsCarriedItemInvalid()) return;

                Vector3 carryPosOffset = playerCamera.IsFirstPerson ? carryPositionFirstPersonOffset : carryPositionThirdPersonOffset;
                carryPosOffset = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * carryPosOffset;

                var holdDistance = carryHoldingDistance * (playerCamera.IsZoomed ? holdingDistanceZoomMultiplier : 1);
                carryPosition = transform.position + cameraTransform.forward * holdDistance + carryPosOffset;
                
                Vector3 force = (carryPosition - carriedItemTransform.position) * carryForceMultiplier
                    - carriedItemBody.velocity;
                carriedItemBody.AddForce(force, ForceMode.VelocityChange);
            }
        }

        private bool IsCarriedItemInvalid() 
        {
            if (carriedItemTransform == null) 
            {
                carriedItemBody = null;
                carryingItem = false;
                return true;
            }
            return false;
        }

        private void UpdateModel()
        {
            transform.rotation = playerCamera.transform.rotation;
        }

        public void OnShoot(InputAction.CallbackContext context)
        {
            
        }

        private void SetCarriedItem(Transform newItemTransform)
        {
            if (newItemTransform is not null)
            {
                carriedItemTransform = newItemTransform;
                carriedItemBody = newItemTransform.GetComponent<Rigidbody>();
                carryingItem = true;

                carriedItemBody.excludeLayers |= playerLayer.value;
            }
            else
            {
                carriedItemBody.excludeLayers &= ~playerLayer.value;

                carriedItemTransform = null;
                carriedItemBody = null;
                carryingItem = false;
            }
        }

        public void OnPickup(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                int mask = itemLayer;
                Ray ray = new(cameraTransform.position, cameraTransform.forward);
                if (carryingItem)
                {
                    SetCarriedItem(null);
                }
                else if (Physics.Raycast(ray, out var hit, 1000f, mask))
                {
                    var hitIsItem = hit.transform.CompareTag("Item");
                    var hitInPickupRange = (hit.transform.position - transform.position).magnitude < maxPickupRange;
                    if (hitIsItem && hitInPickupRange)
                    {
                        SetCarriedItem(hit.transform);
                    }
                }
            }    
        }

        private void OnEnable()
        {
            if (actions is null)
            {
                actions = new DefaultInputActions();
                actions.PlayerGun.SetCallbacks(this);
            }

            actions.PlayerGun.Enable();
        }

        private void OnDisable()
        {
            actions.PlayerGun.Disable();
        }
    }
}
