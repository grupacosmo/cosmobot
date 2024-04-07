using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class GrabberTest : MonoBehaviour
    {
        Grabber grabber;
        void Start()
        {
            grabber = GetComponentInParent<Grabber>();
        }
        private void OnTriggerEnter(Collider collision) 
        {
            if (collision.gameObject.CompareTag("Item"))
            {
                grabber.GrabItem();
            }
        }
    }
}
