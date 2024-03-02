using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmobot
{
    public class Minimap : MonoBehaviour, DefaultInputActions.IMinimapActions
    {
        [SerializeField]
        private Transform player;
        [SerializeField]
        private GameObject minimap;
        DefaultInputActions actions;

        public void OnToggle(InputAction.CallbackContext context)
        {
            if (context.performed)
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


        void OnEnable()
        {
            if (actions == null)
            {
                actions = new DefaultInputActions();
                actions.Minimap.SetCallbacks(this);
            }
            actions.Minimap.Enable();
        }

        void OnDisable()
        {
            actions.Minimap.Disable();
        }
    }
}
