using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class Tracking : MonoBehaviour {
        public float speed = 20.0f;

        public GameObject target = null;
        Vector3 lastKnownPosition = Vector3.zero;
        Quaternion lookAtRotation;
        Quaternion defaultRotation;

        void Start () {
            defaultRotation = transform.rotation;
        }
        // Update is called once per frame
        void Update () {
            if(target){
                if(lastKnownPosition != target.transform.position){
                    lastKnownPosition = target.transform.position;
                    lookAtRotation = Quaternion.LookRotation(lastKnownPosition - transform.position);
                }

                if(transform.rotation != lookAtRotation){
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAtRotation, speed * Time.deltaTime);
                }
            }
            else
            {
                FaceFront();
            }
        }

        public void SetTarget(GameObject toTarget){
            target = toTarget;
        }

        private void OnTriggerEnter(Collider other) {
            if(other.gameObject.tag=="Enemy")
            {
                SetTarget(other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other) {
            if(other.gameObject == target)
            {
                target = null;
            }
        }

        public void FaceFront()
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, defaultRotation, speed * Time.deltaTime);
        }
    }
}
