using Cosmobot.BuildingSystem;
using UnityEngine;

namespace Cosmobot
{
    public class BaseBuilding : MonoBehaviour
    {
        public BuildingInfo BuildingInfo { get; protected set; }

        public Vector2Int GridSize => BuildingInfo.GridSize;

        // Remove the building, without destruction effects or dropping resources, etc.
        public void Despawn()
        {
            Destroy(gameObject);
        }
    }
}
