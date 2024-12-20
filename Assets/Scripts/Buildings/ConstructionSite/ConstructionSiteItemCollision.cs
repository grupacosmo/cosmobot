using Cosmobot.ItemSystem;
using UnityEngine;

namespace Cosmobot
{
    public class ConstructionSiteItemCollision : MonoBehaviour
    {
        void OnCollisionEnter(Collision col) {
            if (col.gameObject.GetComponent<Item>() is null) {
                return;
            }
            
            GetComponentInParent<ConstructionSite>().DecreaseResourceRequirement(col.gameObject.GetComponent<Item>().ItemInfo);
            Destroy(col.gameObject);
        }
    }
}
