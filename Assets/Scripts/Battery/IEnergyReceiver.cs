using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public interface IEnergyReceiver
    {
        float CollectEnergy(float amount);
    }
}
