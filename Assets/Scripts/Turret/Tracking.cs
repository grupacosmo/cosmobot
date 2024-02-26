using System;
using System.Collections;
using System.Collections.Generic;
using Cosmobot.Entity;
using UnityEngine;

namespace Cosmobot
{
    [RequireComponent(typeof(TurretStats))]
    public class Tracking : MonoBehaviour
    {

        public Health target { get; private set; } = null;
        private TurretStats turretStats;
        private HashSet<Health> targetsList = new();
        private Vector3 lastKnownPosition = Vector3.zero;
        private Quaternion lookAtRotation;
        private Quaternion defaultRotation;
        private Quaternion maxAngleRotation;
        private Quaternion minAngleRotation;
        private bool goMaxAngle = false;

        void Start()
        {
            turretStats = GetComponent<TurretStats>();
            defaultRotation = transform.rotation;
            maxAngleRotation = Quaternion.AngleAxis(turretStats.angleRange / 2, Vector3.up);
            minAngleRotation = Quaternion.AngleAxis(turretStats.angleRange / 2 * -1, Vector3.up);
        }

        private void Death(Health source, float oldHealth, float damageValue)
        {
            RemoveTargetFromList(source);
        }

        void Update()
        {
            ChooseTarget();
            if (target)
            {
                UpdateTarget();
            }
            else
            {
                IdleScanning();
            }
        }

        private void IdleScanning()
        {
            if (goMaxAngle)
            {
                IdleScanningMax();
            }
            else
            {
                IdleScanningMin();
            }
        }

        private void UpdateTarget()
        {
            if (lastKnownPosition != target.transform.position)
            {
                if (!UpdateTargetRange(target))
                {
                    RemoveTargetFromList(target);
                }
            }
            if (transform.rotation != lookAtRotation)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAtRotation, turretStats.speed * Time.deltaTime);
            }
        }

        private void SetTarget(Health newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// Chooses which enemy in range should be targeted based on distance (shortest distance)
        /// </summary>
        private void ChooseTarget()
        {
            float minDistance = Vector3.Distance(transform.position, new Vector3(1000, 1000, 1000));
            Health closestTarget = null;
            foreach (Health potentialTarget in targetsList)
            {
                float targetDistance = Vector3.Distance(potentialTarget.transform.position, transform.position);
                if (minDistance > targetDistance)
                {
                    minDistance = targetDistance;
                    closestTarget = potentialTarget;
                }
            }
            Health oldTarget = target;
            SetTarget(closestTarget);
            if (oldTarget)
            {
                oldTarget.OnDeath -= Death;
            }
            if (target)
            {
                target.OnDeath += Death;
            }
        }

        private void AddTargetToList(Health newTarget)
        {
            targetsList.Add(newTarget);
        }

        private void RemoveTargetFromList(Health leavingTarget)
        {
            targetsList.Remove(leavingTarget);
        }

        private bool UpdateTargetRange(Health target)
        {
            UpdateTargetPosition(target);
            if (Quaternion.Angle(defaultRotation, lookAtRotation) <= turretStats.angleRange / 2 && Quaternion.Angle(defaultRotation, lookAtRotation) >= turretStats.angleRange / 2 * -1)
            {
                return true;
            }
            return false;
        }

        private void UpdateTargetPosition(Health target)
        {
            lastKnownPosition = target.transform.position;
            lookAtRotation = Quaternion.LookRotation(lastKnownPosition - transform.position);
        }

        private void IdleScanningMin()
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, minAngleRotation, turretStats.speed * Time.deltaTime);
            if (transform.rotation == minAngleRotation)
            {
                goMaxAngle = true;
            }
        }

        private void IdleScanningMax()
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, maxAngleRotation, turretStats.speed * Time.deltaTime);
            if (transform.rotation == maxAngleRotation)
            {
                goMaxAngle = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                if (UpdateTargetRange(other.gameObject.GetComponent<Health>()))
                {
                    AddTargetToList(other.gameObject.GetComponent<Health>());
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                RemoveTargetFromList(other.gameObject.GetComponent<Health>());
            }
        }
    }
}
