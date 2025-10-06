using UnityEngine;

namespace Cosmobot
{
    public class RobotInteractionHandler : MonoBehaviour
    {
        private IInteractable interaction  ;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IInteractable encounteredInteraction))
            {
                interaction = encounteredInteraction;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IInteractable encounteredInteraction))
            {
                if (encounteredInteraction == interaction)
                {
                    interaction = null;
                }
            }
        }

        public void Interact()
        {
            interaction.Use();
        }
    }
}
