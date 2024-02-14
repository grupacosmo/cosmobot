using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class Tracking : MonoBehaviour {
        //for tracking to work remember to freeze turret's position in place, else it could fly away idk why
        public float speed = 20.0f;
        public float angleRange = 160f;

        GameObject target = null;
        Vector3 lastKnownPosition = Vector3.zero;
        Quaternion lookAtRotation;
        Quaternion defaultRotation;
        Quaternion maxAngleRotation;
        Quaternion minAngleRotation;
        bool goMaxAngle = false;

        void Start () {
            defaultRotation = transform.rotation;
            maxAngleRotation = Quaternion.AngleAxis(angleRange/2, Vector3.up);
            minAngleRotation = Quaternion.AngleAxis(angleRange/2*-1, Vector3.up);
        }
        void Update () {
            if(target){
                if(lastKnownPosition != target.transform.position){
                    if(!IsTargetInRange(target))
                    {
                        UnSetTarget();
                    }
                }

                if(transform.rotation != lookAtRotation){
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAtRotation, speed * Time.deltaTime);
                }
            }
            else
            {
                if(goMaxAngle)
                {
                    IdleScanningMax();
                }
                else
                {
                    IdleScanningMin();
                }
            }
        }

        public void SetTarget(GameObject toTarget){
            target = toTarget;
        }

        public void UnSetTarget(){
            target = null;
        }

        public bool IsTargetInRange(GameObject target)
        {
            lastKnownPosition = target.transform.position;
            lookAtRotation = Quaternion.LookRotation(lastKnownPosition - transform.position);
            if(Quaternion.Angle(defaultRotation, lookAtRotation)<=angleRange/2 && Quaternion.Angle(defaultRotation, lookAtRotation)>=angleRange/2*-1)
            {
                return true;
            }
            return false;
        }

        public void IdleScanningMin()
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, minAngleRotation, speed * Time.deltaTime);
            if(transform.rotation==minAngleRotation)
            {
                goMaxAngle=true;
            }
        }

        public void IdleScanningMax()
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, maxAngleRotation, speed * Time.deltaTime);
            if(transform.rotation==maxAngleRotation)
            {
                goMaxAngle=false;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if(other.gameObject.tag=="Enemy")
            {
                if(IsTargetInRange(other.gameObject))
                {
                    SetTarget(other.gameObject);
                }
            }
        }

        private void OnTriggerExit(Collider other) {
            if(other.gameObject == target)
            {
                UnSetTarget();
            }
        }

        private void OnTriggerStay(Collider other) {
            if(target==null)
            {
                if(other.gameObject.tag=="Enemy")
                {
                    if(IsTargetInRange(other.gameObject))
                    {
                        SetTarget(other.gameObject);
                    }
                }
            }   
        }
    }
}
