using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class Tracking : MonoBehaviour {
        
        TurretStats turretStats;
        public GameObject target = null;
        HashSet<GameObject> targetsList = new();
        Vector3 lastKnownPosition = Vector3.zero;
        Quaternion lookAtRotation;
        Quaternion defaultRotation;
        Quaternion maxAngleRotation;
        Quaternion minAngleRotation;
        bool goMaxAngle = false;

        void Start () {
            turretStats = GetComponent<TurretStats>();
            defaultRotation = transform.rotation;
        }
        void Update () {
            maxAngleRotation = Quaternion.AngleAxis(turretStats.angleRange/2, Vector3.up);
            minAngleRotation = Quaternion.AngleAxis(turretStats.angleRange/2*-1, Vector3.up);
            ChooseTarget();
            if(target){
                if(lastKnownPosition != target.transform.position){
                    if(!IsTargetInRange(target))
                    {
                        RemoveTargetFromList(target);
                    }
                }

                if(transform.rotation != lookAtRotation){
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAtRotation, turretStats.speed * Time.deltaTime);
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

        public void SetTarget(GameObject newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// Chooses which enemy in range should be targeted based on distance (shortest distance)
        /// </summary>
        public void ChooseTarget(){
            float minDistance = Vector3.Distance(transform.position, new Vector3(1000,1000,1000));
            GameObject closestTarget = null;
            foreach (GameObject potentialTarget in targetsList)
            {
                float targetDistance = Vector3.Distance(potentialTarget.transform.position, transform.position);
                if(minDistance>targetDistance)
                {
                    minDistance = targetDistance;
                    closestTarget = potentialTarget;
                }
            }
            SetTarget(closestTarget);
        }

        public void AddTargetToList(GameObject newTarget)
        {
            targetsList.Add(newTarget);
        }

        public void RemoveTargetFromList(GameObject leavingTarget)
        {
            targetsList.Remove(leavingTarget);
        }

        /// <summary>
        /// Checks if target is in range, based on angle rotation of a turret
        /// </summary>
        /// <param name="target">Currently targeted enemy</param>
        /// <returns>true if target is in range, false if it isn't</returns>
        public bool IsTargetInRange(GameObject target)
        {
            lastKnownPosition = target.transform.position;
            lookAtRotation = Quaternion.LookRotation(lastKnownPosition - transform.position);
            if(Quaternion.Angle(defaultRotation, lookAtRotation)<=turretStats.angleRange/2 && Quaternion.Angle(defaultRotation, lookAtRotation)>=turretStats.angleRange/2*-1)
            {
                return true;
            }
            return false;
        }

        public void IdleScanningMin()
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, minAngleRotation, turretStats.speed * Time.deltaTime);
            if(transform.rotation==minAngleRotation)
            {
                goMaxAngle=true;
            }
        }

        public void IdleScanningMax()
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, maxAngleRotation, turretStats.speed * Time.deltaTime);
            if(transform.rotation==maxAngleRotation)
            {
                goMaxAngle=false;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if(other.gameObject.CompareTag("Enemy"))
            {
                if(IsTargetInRange(other.gameObject))
                {
                    AddTargetToList(other.gameObject);
                }
            }
        }

        private void OnTriggerExit(Collider other) {
            if(other.gameObject.CompareTag("Enemy"))
            {
                RemoveTargetFromList(other.gameObject);
            }
        }

        private void OnTriggerStay(Collider other) {
            if(other.gameObject.CompareTag("Enemy"))
            {
                if(IsTargetInRange(other.gameObject))
                {
                    AddTargetToList(other.gameObject);
                }
            }
        }
    }
}
