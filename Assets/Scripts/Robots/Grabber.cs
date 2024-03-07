using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Cosmobot
{
    public class Grabber : MonoBehaviour
    {
        public GameObject grabbedItem;
        public int weightTier;

        public float grabbedItemOffset;
        public float grabDistance;

        private DefaultInputActions actions;

        private void Update()
        {
            if (grabbedItem != null) 
            {
                // in case of adding backpack, this is the section that we'll want to expand
                // (probably add some more offsets, to synchronise it with player movement so it would fit in the right slot)
                grabbedItem.transform.position = transform.position + new Vector3(0, grabbedItemOffset, 0);
            }
        }

        GameObject FindNearestItem()
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

            return nearestItem.gameObject;
        }

        public void OnPickup(InputAction.CallbackContext context)
        {
            if (grabbedItem == null)
            {
                GameObject nearestItem = FindNearestItem();
                grabbedItem = nearestItem;
            }
            else
            {
                grabbedItem = null;
            }


        }
    }
}
