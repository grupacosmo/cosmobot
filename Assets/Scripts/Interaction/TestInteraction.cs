using System;
using System.Collections;
using System.Collections.Generic;
using Codice.Client.BaseCommands.TubeClient;
using UnityEngine;

namespace Cosmobot
{
    [RequireComponent(typeof(Collider))]
    public class TestInteraction : MonoBehaviour, IInteractable
    {
        private int i=0;
        private int j=0;
        private bool isInteractionPossible = true;
        public string Prompt {get; private set;} = "Press 'E' to interact";
        public void Use()
        {
            if(isInteractionPossible)
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
