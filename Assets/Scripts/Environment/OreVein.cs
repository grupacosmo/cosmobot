using Cosmobot.ItemSystem;
using UnityEngine;

namespace Cosmobot
{
    public class OreVein : MonoBehaviour
    {
        [SerializeField] private ItemInfo material;
        [SerializeField] private int oreLeft;

        public ItemInfo Mine()
        {
            DepleteDeposit();

            return material;
        }

        private void DepleteDeposit()
        {
            oreLeft--;

            if (oreLeft <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
