using UnityEngine;
using System.Collections;

namespace Cosmobot
{
    public class Grabber : MonoBehaviour
    {
        public bool automaticGrabMode;
        public GameObject grabbedItem = null;
        public GameObject lastItem = null;

        #region grab
        [Header("Grab")]
        public Vector3 grabOffset;
        public float grabDistance;
        #endregion grab

        #region release
        [Header("Release")]
        public float releaseDistance;
        public float releaseResetTime = 15f;
        public LayerMask releaseExcludedLayer;
        #endregion release

        private Coroutine resetCoroutine;
        private RouteMovement routeMovement;

        private IEnumerator ResetLastItemCoroutine()
        {
            yield return new WaitForSeconds(releaseResetTime);
            lastItem = null;
            resetCoroutine = StartCoroutine(ResetLastItemCoroutine());
        }

        private void ResetTimer()
        {
            if (resetCoroutine is not null) StopCoroutine(resetCoroutine);

            resetCoroutine = StartCoroutine(ResetLastItemCoroutine());
        }

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

                if (collider.CompareTag("Item")) GrabItem(collider.gameObject);
            }
        }

        public void MoveGrabbedItem()
        {
            if (grabbedItem) grabbedItem.transform.position = transform.position + grabOffset;
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

                    grabbedItem.transform.rotation = Quaternion.identity;

                    Rigidbody itemRigidbody = grabbedItem.GetComponent<Rigidbody>();
                    if (itemRigidbody is not null)
                    {
                        itemRigidbody.isKinematic = true;
                    }
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
                    Rigidbody itemRigidbody = grabbedItem.GetComponent<Rigidbody>();
                    if (itemRigidbody is not null)
                    {
                        itemRigidbody.isKinematic = false;
                    }

                    Vector3 facingDirection = routeMovement.transform.forward;
                    grabbedItem.transform.position = new Vector3(pos.x, hit.point.y, pos.z) + facingDirection * releaseDistance;
                    grabbedItem.transform.rotation = Quaternion.identity;



                    ResetTimer();
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

                    if (distance < nearestDistance && nearItem.gameObject != lastItem)
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
