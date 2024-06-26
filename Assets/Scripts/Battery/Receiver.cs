using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    [RequireComponent(typeof(ReceiverStats))]
    public class Receiver : MonoBehaviour, IEnergyReceiver
    {
        private ReceiverStats receiverStats;
        
        public float CollectEnergy(float amount)
        {
            float rest = 0;
            if (receiverStats.currentCapacity + amount > receiverStats.maxCapacity)
            {
                rest = receiverStats.currentCapacity + amount - receiverStats.maxCapacity;
                receiverStats.currentCapacity = receiverStats.maxCapacity;
            }
            else
            {
                receiverStats.currentCapacity += amount;
            }
            return rest;
        }

        void Start()
        {
            receiverStats = GetComponent<ReceiverStats>();
        }
    }
}
