using UnityEngine;

namespace Cosmobot
{
    public class CameraPosition : MonoBehaviour
    {
        public Transform cameraPosition;

        private void Update()
        {
            transform.position = cameraPosition.position;
        }
    }
}
