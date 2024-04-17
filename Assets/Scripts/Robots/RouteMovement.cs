using UnityEngine;

namespace Cosmobot
{

    [RequireComponent(typeof(Rigidbody))]
    public class RouteMovement : MonoBehaviour
    {
        #region start variables
        public bool canMove;
        public float robotSpeed;
        public float gravity = 50;
        public float distanceToGround = 0.1f;

        public Vector3 direction = Vector3.zero;

        #endregion start variables

        #region route
        [Header("Route")]
        public bool loopMode;
        public float arrivalThreshold;
        public int routeIndex = 0;
        public Route[] route;
        private bool routeForward = true;
        // arrivalThreshold had been tested for 0.5f, but might not work correctly for higher velocities
        // (but i don't think we ever plan to set the velocity THAT high, we're talking about setting it to 999 speed)
        #endregion route

        #region ground check
        private bool isGrounded;
        private float groundCheckRadius;
        private float groundCheckDistance;
        public float maxFloorAngleDegrees;
        private Vector3 groundNormal = Vector3.up;
        public Transform groundCheckOrigin;
        private CapsuleCollider capsuleCollider;
        #endregion ground check

        private Rigidbody rb;
        private Grabber grabber;

        void Start()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            var radius = capsuleCollider.radius;
            groundCheckRadius = radius * 0.99f; // 0.99 - padding to make spherecast detect floor
            groundCheckDistance = radius;

            rb = GetComponent<Rigidbody>();
            grabber = GetComponentInChildren<Grabber>();
        }

        void FixedUpdate()
        {
            GroundCheck();
            ProcessMovement();
            ProcessRotation();
        }

        private Vector3 CalculateVelocityDelta()
        {
            direction.y = 0f;

            var inputDirection = Quaternion.LookRotation(direction);

            var velocity = rb.velocity;
            var targetVelocity = direction * robotSpeed + new Vector3(0, velocity.y, 0);
            return Vector3.MoveTowards(velocity, targetVelocity,
                50 * Time.fixedDeltaTime) - rb.velocity;
        }

        private void GroundCheck()
        {
            var origin = groundCheckOrigin.position;

            Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out var hitInfo, groundCheckDistance);
            if (hitInfo.collider != null && rb.velocity.y < 0.01)
            {
                groundNormal = hitInfo.normal;
                var floorAngleDegrees = Mathf.Acos(Vector3.Dot(Vector3.up, groundNormal)) * Mathf.Rad2Deg;
                isGrounded = floorAngleDegrees <= maxFloorAngleDegrees;
            }
            else isGrounded = false;

            if (!isGrounded) groundNormal = Vector3.up;
        }

        private void ProcessMovement()
        {
            if (CheckIfArrived(route[routeIndex].waypoint))
            {
                UpdateRoute();
                if (grabber)
                {
                    TryRelease();
                    TryGrab();
                }
            }

            Vector3 velocityDelta;

            if (canMove) velocityDelta = CalculateVelocityDelta();
            else velocityDelta = Vector3.zero;

            velocityDelta -= gravity * Time.fixedDeltaTime * groundNormal;
            rb.AddForce(velocityDelta, ForceMode.VelocityChange);
        }

        private void ProcessRotation()
        {
            direction = GetDirectionToPoint(transform.position, route[routeIndex].waypoint);
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = rotation;
        }

        Vector3 GetDirectionToPoint(Vector3 pointFrom, Vector3 pointTo)
        {
            Vector3 directionToPoint = pointTo - pointFrom;
            directionToPoint = new Vector3(directionToPoint.x, 0, directionToPoint.z).normalized;
            return directionToPoint;
        }

        private void UpdateRoute()
        {
            if (routeForward)
            {
                if (routeIndex == route.Length - 1)
                {
                    if (loopMode) routeIndex = -1;
                    else routeForward = false;
                }
            }
            else if (routeIndex == 0) routeForward = true;

            int add_amount = routeForward ? 1 : -1;
            routeIndex += add_amount;
        }

        private void TryRelease()
        {
            if (route[routeIndex].release && grabber.grabbedItem is not null)
            {
                grabber.ReleaseItem();
            }
        }
        private void TryGrab()
        {
            if (grabber.automaticGrabMode) return;
            if (route[routeIndex].grab && grabber.grabbedItem is null)
            {
                grabber.GrabItem();
            }
        }
        private bool CheckIfArrived(Vector3 point)
        {
            var currentPosition = new Vector3(transform.position.x, 0f, transform.position.z);
            point = new Vector3(point.x, 0f, point.z);

            float distanceToTarget = Vector3.Distance(currentPosition, point);

            if (distanceToTarget <= arrivalThreshold)
            {
                Debug.Log($"Robot has arrived at the point number '{routeIndex}'.");
                return true;
            }
            else return false;
        }
    }
}
