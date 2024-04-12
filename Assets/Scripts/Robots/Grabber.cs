using UnityEngine;


namespace Cosmobot
{
    public class Grabber : MonoBehaviour
    {
        public bool automaticGrabMode;
        public GameObject grabbedItem = null;
        public GameObject lastItem = null;

        public Vector3 grabOffset;
        public float grabDistance;
        public float releaseDistance;
        public LayerMask releaseExcludedLayer;

        private RouteMovement routeMovement;

        public void Start()
        {
            grabbedItem = null;
            lastItem = null;

            routeMovement = GetComponentInParent<RouteMovement>();
            transform.localScale = new Vector3(grabDistance, grabDistance, grabDistance);
        }

        private void FixedUpdate()
        {
            MoveGrabbedItem();
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (automaticGrabMode)
            {
                if (grabbedItem is not null) return;
                if (collider.gameObject == lastItem) return;
                //if (lastItem is null) return;
                //lastItem is not collider.gameObject && 
                if (collider.CompareTag("Item")) GrabItem();
            }
        }

        public void MoveGrabbedItem()
        {
            if (grabbedItem)
            {
                grabbedItem.transform.position = transform.position + grabOffset;
            }
        }

        public void GrabItem(GameObject item = null)
        {
            Debug.Log("Trying to grab an item...");
            if (grabbedItem is null)
            {
                if (item is null)
                {
                    GameObject nearestItem = GetNearestItem();
                    grabbedItem = nearestItem;
                }
                else grabbedItem = item;

                Debug.Log("Grabbing item: SUCCESSFUL");
            }
        }

        public void ReleaseItem()
        {
            if (grabbedItem)
            {
                lastItem = grabbedItem;
                Vector3 pos = grabbedItem.transform.position;
                Ray ray = new Ray(new Vector3(pos.x, pos.y, pos.z), Vector3.down);

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~releaseExcludedLayer))
                {
                    Debug.Log("Raycast: SUCCESFUL");

                    Vector3 facingDirection = routeMovement.transform.forward;

                    grabbedItem.transform.position = new Vector3(pos.x, hit.point.y, pos.z) + facingDirection * releaseDistance;
                }

                grabbedItem = null;

                Debug.Log("Releasing item: SUCCESFUL");
            }
        }

        GameObject GetNearestItem()
        {
            float nearestDistance = float.MaxValue;
            GameObject nearestItem = null;

            Collider[] nearItems = Physics.OverlapSphere(transform.position, grabDistance);
            foreach (Collider nearItem in nearItems)
            {
                if (nearItem.gameObject.CompareTag("Item"))
                {
                    float distance = Vector3.Distance(transform.position, nearItem.transform.position);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestItem = nearItem.gameObject;
                    }
                }
            }

            return nearestItem;
        }


    }
}
