using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class Minimap : MonoBehaviour
    {
        public Transform player;
        public GameObject minimap;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                minimap.SetActive(!minimap.activeSelf);
            }
        }

        void LateUpdate()
        {
            Vector3 position = player.position;
            position.y = transform.position.y;

            transform.position = player.position;
            transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }
    }
}
