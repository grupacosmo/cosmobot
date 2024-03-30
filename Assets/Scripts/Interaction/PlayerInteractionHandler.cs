using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

namespace Cosmobot
{
    public class PlayerInteractionHandler : MonoBehaviour, DefaultInputActions.IInteractionActions
    {
        private IInteractable interaction = null;
        private DefaultInputActions actions;
        [SerializeField]
        private TMP_Text interactionPrompt;

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

        public void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Interact();
            }
        }

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
    }
}
