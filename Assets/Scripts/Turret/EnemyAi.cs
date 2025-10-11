using Cosmobot.Entity;
using UnityEngine;

namespace Cosmobot
{
    public class EnemyAi : MonoBehaviour
    {
        public Transform pointA;
        public Transform pointB;
        public float speed;
        private Health health;

        // Update is called once per frame
        private void Start()
        {
            health = gameObject.GetComponent<Health>();
            health.OnDeath += Death;
        }

        private void Update()
        {
            transform.position =
                Vector3.Lerp(pointA.position, pointB.position, Mathf.Pow(Mathf.Sin(Time.time * speed), 2));
        }

        private void Death(Health source, float oldHealth, float damageValue)
        {
            Destroy(gameObject);
        }
    }
}
