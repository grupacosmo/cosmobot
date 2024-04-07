using System.Collections;
using System.Collections.Generic;
using Codice.Client.Common;
using Codice.CM.Common;
using Cosmobot.ItemSystem;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

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

        private RouteMovement routeMovement;
        public void Start()
        {
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
                if ((grabbedItem == null) && (lastItem != collider.gameObject) && collider.CompareTag("Item"))
                {
                    GrabItem();
                }
            }
        }

        public void MoveGrabbedItem()
        {
            if (grabbedItem != null) 
            {
                grabbedItem.transform.position = transform.position + grabOffset;
            }
        }

        public void GrabItem()
        {   
            if (grabbedItem == null)
            {
                GameObject nearestItem = GetNearestItem();
                grabbedItem = nearestItem;
                
                Debug.Log("Grabbing item: SUCCESSFUL");
            }
        }

        public void ReleaseItem()
        {
            lastItem = grabbedItem;
            if (grabbedItem != null)
            {
                Vector3 pos = grabbedItem.transform.position;
                Ray ray = new Ray(new Vector3(pos.x, pos.y, pos.z), Vector3.down);
                RaycastHit hit;
                LayerMask playerMask = 1 << LayerMask.NameToLayer("Enemy");

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~playerMask))
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
                if (nearItem.gameObject.tag == "Item")
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
