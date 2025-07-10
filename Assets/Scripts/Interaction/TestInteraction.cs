using System.Collections;
using UnityEngine;

namespace Cosmobot
{
    [RequireComponent(typeof(Collider))]
    public class TestInteraction : MonoBehaviour, IInteractable
    {
        private int i;
        private bool isInteractionPossible = true;
        private int j;
        public string Prompt { get; private set; } = "Press 'E' to interact";

        public void Use()
        {
            if (isInteractionPossible)
            {
                StartCoroutine(InteractionPossible());
            }
            else
            {
                StartCoroutine(InteractionImpossible());
            }
        }

        private IEnumerator InteractionImpossible()
        {
            j++;
            Prompt = $"Interaction is impossible, please wait. {j}";
            yield return new WaitForSeconds(1);
            Prompt = $"Press 'E' to interact! {j}";
        }

        private IEnumerator InteractionPossible()
        {
            isInteractionPossible = false;
            i++;
            Debug.Log($"Interaction triggered #{i}");
            yield return new WaitForSeconds(5);
            isInteractionPossible = true;
        }
    }
}
