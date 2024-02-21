using UnityEngine;

namespace Cosmobot
{
    public class FollowObject : MonoBehaviour
    {
        public Transform target;

        private void Update()
        {
            transform.position = target.position;
        }
    }
}
