using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmobot
{
    public class PlayerGun : MonoBehaviour, DefaultInputActions.IPlayerGunActions
    {

        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float maxPickupRange; // relative to character, not camera
        [SerializeField] private float carryHoldingDistance;
        [SerializeField] private float holdingDistanceZoomMultiplier = 1f;
        [SerializeField] private Vector3 carryPositionFirstPersonOffset;
        [SerializeField] private Vector3 carryPositionThirdPersonOffset;
        [SerializeField] private float carryForceMultiplier = 1f;

        private bool carryingItem = false;
        private Transform carriedItemTransform = null;
        private Rigidbody carriedItemBody = null;

        private Vector3 carryPosition;

        private DefaultInputActions actions;

        public void Update()
        {
            UpdateModel();
        }

        public void FixedUpdate()
        {
            var carryPosOffset = playerCamera.isFirstPerson ? carryPositionFirstPersonOffset : carryPositionThirdPersonOffset;
            carryPosOffset = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * carryPosOffset;

            var holdDistance = carryHoldingDistance * (playerCamera.isZoomed ? holdingDistanceZoomMultiplier : 1);

            carryPosition = transform.position + cameraTransform.forward * holdDistance + carryPosOffset;
            if (carryingItem)
            {
                var force = (carryPosition - carriedItemTransform.position) * carryForceMultiplier
                    - carriedItemBody.velocity;
                carriedItemBody.AddForce(force, ForceMode.VelocityChange);
            }
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
            if(newItemTransform != null)
            {
                carriedItemTransform = newItemTransform;
                carriedItemBody = newItemTransform.GetComponent<Rigidbody>();
                carryingItem = true;

                carriedItemBody.excludeLayers |= 1 << LayerMask.NameToLayer("Player");
            }
            else
            {
                carriedItemBody.excludeLayers &= ~(1 << LayerMask.NameToLayer("Player"));

                carriedItemTransform = null;
                carriedItemBody = null;
                carryingItem = false;
            }
        }

        public void OnPickup(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                int mask = LayerMask.GetMask("Item");
                Ray ray = new(cameraTransform.position, cameraTransform.forward);
                if (carryingItem)
                {
                    SetCarriedItem(null);
                }
                else if (Physics.Raycast(ray, out var hit, 1000f, mask))
                {
                    if (hit.transform.CompareTag("Item") && 
                        (hit.transform.position-transform.position).magnitude<maxPickupRange)
                    {
                        SetCarriedItem(hit.transform);
                    }
                }
            }    
        }

        private void OnEnable()
        {
            if (actions == null)
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
