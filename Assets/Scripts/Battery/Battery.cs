using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using Cosmobot.ItemSystem;

namespace Cosmobot
{
    public class Battery : MonoBehaviour, IEnergyProvider, IEnergyReceiver
    {
        private Item item;
        private List<IEnergyReceiver> receivers = new();
        private float currentTransferTime;

        public float CollectEnergy(float amount)
        {
            float rest = 0;
            if (item.FloatValue[ItemDataConstants.Charge] + amount > item.FloatValue[ItemDataConstants.MaxCharge])
            {
                rest = item.FloatValue[ItemDataConstants.Charge] + amount - item.FloatValue[ItemDataConstants.MaxCharge];
                item.FloatValue[ItemDataConstants.Charge] = item.FloatValue[ItemDataConstants.MaxCharge];
            }
            else
            {
                item.FloatValue[ItemDataConstants.Charge] += amount;
            }
            return rest;
        }

        public void TransferEnergy()
        {
            if (receivers.Count > 0 && item.FloatValue[ItemDataConstants.Charge] >= item.FloatValue[ItemDataConstants.TransferRate])
            {
                float transferAmount = item.FloatValue[ItemDataConstants.TransferRate] / receivers.Count;
                foreach (IEnergyReceiver receiver in receivers)
                {
                    item.FloatValue[ItemDataConstants.Charge] = item.FloatValue[ItemDataConstants.Charge] - transferAmount + receiver.CollectEnergy(transferAmount);
                }
            }
        }

        void Update()
        {
            currentTransferTime += Time.deltaTime;
            if (currentTransferTime > 1)
            {
                currentTransferTime = 0;
                TransferEnergy();
            }
        }

        void Awake()
        {
            item = GetComponent<Item>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IEnergyReceiver encounteredReceiver) && !other.TryGetComponent(out Battery battery))
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
