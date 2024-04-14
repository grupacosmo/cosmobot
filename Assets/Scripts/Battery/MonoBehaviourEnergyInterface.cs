using UnityEngine;

namespace Cosmobot
{
    // kill me
    // TODO: find a way to make SerializeReference work with UnityEngine.Object 
    public abstract class MonoBehaviourEnergyInterface : MonoBehaviour, IEnergyInterface
    {
        public abstract float Capacity { get; }
        public abstract float Charge { get; }
        public abstract float TransferEnergyIn(float amount);
        public abstract float TransferEnergyOut(float amount);
    }
}