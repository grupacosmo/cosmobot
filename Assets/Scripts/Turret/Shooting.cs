using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class Shooting : MonoBehaviour
    {
        ParticleSystem ps;
        Ray ray;
        RaycastHit hit;
        Tracking tracking;
        TurretStats turretStats;
        //public float fireRate = 2f;
        //public float power = 12.5f;
        bool canFire = true;
        void Start()
        {
            tracking = GetComponent<Tracking>();
            ps = GetComponent<ParticleSystem>();
            turretStats = GetComponent<TurretStats>();
        }

        // Update is called once per frame
        void Update()
        {
            ray = new Ray(transform.position, transform.forward);
            if(Physics.Raycast(ray, out hit))
            {
                if(hit.collider.gameObject==tracking.target && canFire)
                {
                    StartCoroutine(Shoot(hit.collider.gameObject));
                }
            }
        }


        public IEnumerator Shoot(GameObject target)
        {
            canFire = false;
            ps.Play();
            EnemyAi targetAi = target.GetComponent<EnemyAi>();
            if(targetAi.Damaged(turretStats.power)<=0)
            {
                tracking.RemoveTargetFromList(target);
                Destroy(target);
            }
            yield return new WaitForSeconds(turretStats.fireRate);
            canFire=true;
            ps.Stop();
            
            
        }
    }
}
