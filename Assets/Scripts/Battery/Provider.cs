using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    [RequireComponent(typeof(ProviderStats))]
    public class Provider : MonoBehaviour, IEnergyProvider
    {
        private ProviderStats providerStats;
        private List<IEnergyReceiver> receivers = new();
        private float currentTransferTime;

        public void TransferEnergy()
        {
            if (receivers.Count > 0)
            {
                float transferAmount = providerStats.maxEnergyPerSecond / receivers.Count;
                foreach (IEnergyReceiver receiver in receivers)
                {
                    providerStats.currentCapacity = providerStats.currentCapacity - transferAmount + receiver.CollectEnergy(transferAmount);
                }
            }
        }

        void Start()
        {
            providerStats = GetComponent<ProviderStats>();
        }

        void Update()
        {
            currentTransferTime += Time.deltaTime;
            if (currentTransferTime > 1f)
            {
                currentTransferTime = 0;
                TransferEnergy();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IEnergyReceiver encounteredReceiver) && !other.isTrigger)
            {
                receivers.Add(encounteredReceiver);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IEnergyReceiver encounteredReceiver))
            {
                receivers.Remove(encounteredReceiver);
            }
        }
    }
}
