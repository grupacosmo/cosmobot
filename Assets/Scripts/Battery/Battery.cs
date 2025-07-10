using System.Collections.Generic;
using Cosmobot.ItemSystem;
using UnityEngine;

namespace Cosmobot
{
    public class Battery : MonoBehaviour, IEnergyProvider, IEnergyReceiver
    {
        private readonly List<IEnergyReceiver> receivers = new();
        private float currentTransferTime;
        private ItemComponent itemComponent;

        private void Awake()
        {
            itemComponent = GetComponent<ItemComponent>();
        }

        private void Update()
        {
            currentTransferTime += Time.deltaTime;
            if (currentTransferTime > 1)
            {
                currentTransferTime = 0;
                TransferEnergy();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IEnergyReceiver encounteredReceiver)
                && !other.TryGetComponent(out Battery battery))
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

        public void TransferEnergy()
        {
            if (receivers.Count > 0
                && itemComponent.FloatValue[ItemDataConstants.Charge]
                >= itemComponent.FloatValue[ItemDataConstants.TransferRate])
            {
                float transferAmount = itemComponent.FloatValue[ItemDataConstants.TransferRate] / receivers.Count;
                foreach (IEnergyReceiver receiver in receivers)
                {
                    itemComponent.FloatValue[ItemDataConstants.Charge] =
                        itemComponent.FloatValue[ItemDataConstants.Charge] - transferAmount
                        + receiver.CollectEnergy(transferAmount);
                }
            }
        }

        public float CollectEnergy(float amount)
        {
            float rest = 0;
            if (itemComponent.FloatValue[ItemDataConstants.Charge] + amount
                > itemComponent.FloatValue[ItemDataConstants.MaxCharge])
            {
                rest = itemComponent.FloatValue[ItemDataConstants.Charge] + amount
                       - itemComponent.FloatValue[ItemDataConstants.MaxCharge];
                itemComponent.FloatValue[ItemDataConstants.Charge] =
                    itemComponent.FloatValue[ItemDataConstants.MaxCharge];
            }
            else
            {
                itemComponent.FloatValue[ItemDataConstants.Charge] += amount;
            }

            return rest;
        }
    }
}
