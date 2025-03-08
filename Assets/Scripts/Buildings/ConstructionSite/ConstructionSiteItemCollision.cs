using Cosmobot.ItemSystem;
using UnityEngine;

namespace Cosmobot
{
    public class ConstructionSiteItemCollision : MonoBehaviour
    {
        void OnCollisionEnter(Collision col) {
            if (col.gameObject.GetComponent<ItemComponent>() is null) {
                return;
            }
            
            ItemInfo collisionItemInfo = col.gameObject.GetComponent<ItemComponent>().ItemInfo;
            ConstructionSite constructionSite = GetComponentInParent<ConstructionSite>();
            if (constructionSite.ConstructionSiteResources.ContainsKey(collisionItemInfo) ) {
                if (constructionSite.ConstructionSiteResources.GetValue(collisionItemInfo).GetComponent<ResourcePreviewController>().ResourceRequirement > 0) {
                    constructionSite.DecreaseResourceRequirement(collisionItemInfo);
                    Destroy(col.gameObject);
                }
            }
        }
    }
}
