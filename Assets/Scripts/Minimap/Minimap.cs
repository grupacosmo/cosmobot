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

        private DefaultInputActions actions;

        private void LateUpdate()
        {
            Vector3 position = player.position;
            position.y = transform.position.y;

            transform.position = player.position;
            transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }


        private void OnEnable()
        {
            if (actions == null)
            {
                actions = new DefaultInputActions();
                actions.Minimap.SetCallbacks(this);
            }

            actions.Minimap.Enable();
        }

        private void OnDisable()
        {
            actions.Minimap.Disable();
        }

        public void OnToggle(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                minimap.SetActive(!minimap.activeSelf);
            }
        }
    }
}
