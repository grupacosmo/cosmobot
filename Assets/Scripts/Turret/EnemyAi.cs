using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class EnemyAi : MonoBehaviour {
        public Transform pointA;
        public Transform pointB;
        public float speed;
        public float hp = 100;
    
        // Update is called once per frame
        void Update () {
            transform.position = Vector3.Lerp(pointA.position, pointB.position, Mathf.Pow(Mathf.Sin(Time.time * speed), 2));
        }

        public float Damaged(float damageAmount)
        {
            hp-=damageAmount;
            Debug.Log(String.Format("ilosc hp {1}: {0}", hp, gameObject.name));
            return hp;
        }
    }
 
}
