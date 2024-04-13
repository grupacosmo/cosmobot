using UnityEngine;

namespace Cosmobot
{
    public class InfiniteEnergyInterface : MonoBehaviourEnergyInterface
    {
        public float InputTransferRate = 99999f;
        public float OutputTransferRate = 99999f;

        public override float Capacity => float.PositiveInfinity;
        public override float Charge => float.PositiveInfinity;

        public override float TransferEnergyIn(float amount)
        {
            return Mathf.Min(amount, InputTransferRate);
        }

        public override float TransferEnergyOut(float amount)
        {
            return Mathf.Min(amount, OutputTransferRate);
        }

    }
}