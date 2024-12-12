using Cosmobot.BuildingSystem;
using UnityEngine;

namespace Cosmobot
{
    public class BaseBuilding : MonoBehaviour
    {
        public BuildingInfo buildingInfo { get; protected set; }

        public Vector2Int GridSize => buildingInfo.GridSize;

        // Remove the building, without destruction effects or dropping resources, etc.
        public void Despawn() 
        {
            Destroy(gameObject);
        }
    }
}
