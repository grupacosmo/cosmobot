using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot.Research
{
    [System.Serializable]
    public struct ResearchUnlockable
    {
        public string Id;
        public bool Unlocked;
        public string Name;
        public float ResearchCost;
    }
}
