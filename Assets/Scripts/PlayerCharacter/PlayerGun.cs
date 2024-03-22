using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class PlayerGun : MonoBehaviour
    {

        [SerializeField] private PlayerCamera playerCamera;

        public void Update()
        {
            transform.rotation = playerCamera.transform.rotation;
        }
    }
}
