using UnityEngine;

namespace Cosmobot
{
    public class PlayerFallProtector : MonoBehaviour
    {
        public float voidYLevel = -20;

        private void FixedUpdate()
        {
            Transform player = GameManager.PlayerTransform;
            if (player && player.position.y < voidYLevel)
            {
                player.position = transform.position;
                player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.1f);
            Gizmos.color = new Color(0,0,0, 0.8f);
            Vector3 voidPos = new Vector3(transform.position.x, voidYLevel, transform.position.z);
            Gizmos.DrawCube(voidPos, new Vector3(1000, 1, 1000));
        }
#endif
    }
}
