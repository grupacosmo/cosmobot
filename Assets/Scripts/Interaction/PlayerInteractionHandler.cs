using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmobot
{
    public class PlayerInteractionHandler : MonoBehaviour, DefaultInputActions.IInteractionActions
    {
        [SerializeField]
        private TMP_Text interactionPrompt;

        private DefaultInputActions actions;
        private IInteractable interaction  ;

        private void OnEnable()
        {
            if (actions == null)
            {
                actions = new DefaultInputActions();
                actions.Interaction.SetCallbacks(this);
            }

            actions.Interaction.Enable();
        }

        private void OnDisable()
        {
            actions.Interaction.Disable();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IInteractable encounteredInteraction))
            {
                interaction = encounteredInteraction;
                ShowInteractionPrompt();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IInteractable encounteredInteraction))
            {
                if (encounteredInteraction == interaction)
                {
                    interaction = null;
                    interactionPrompt.enabled = false;
                }
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Interact();
            }
        }

        private void ShowInteractionPrompt()
        {
            interactionPrompt.text = interaction?.Prompt;
            interactionPrompt.enabled = true;
        }

        private void Interact()
        {
            interaction?.Use();
            ShowInteractionPrompt();
        }
    }
}
