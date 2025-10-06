namespace Cosmobot
{
    public class InfiniteEnergyInterface : MonoBehaviourEnergyInterface
    {
        public override float Capacity => float.PositiveInfinity;
        public override float Charge => float.PositiveInfinity;

        public override float TransferEnergyIn(float amount)
        {
            return 0;
        }

        public override float TransferEnergyOut(float amount)
        {
            return 0;
        }
    }
}
