using UnityEngine;

namespace Cosmobot
{
    public class MaterialDeposit : MonoBehaviour
    {
        [SerializeField] private GameObject material;
        [SerializeField] private int oreLeft;

        public GameObject Mine()
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
