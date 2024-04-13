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
        
        public virtual bool TransferEnergyInOrFail(float amount) {
            return ((IEnergyInterface)this).TransferEnergyInOrFail(amount); // wtf C#
        }
        public virtual bool TransferEnergyOutOrFail(float amount) {
            return ((IEnergyInterface)this).TransferEnergyOutOrFail(amount);
        }
    }
}