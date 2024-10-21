using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class PlayerConstructionHandler : MonoBehaviour
    {

        [SerializeField] Transform cameraTransform;
        [SerializeField] Transform playerTransform;
        [SerializeField] Transform buildPointIndicator;
        [SerializeField] LayerMask buildTargetingCollisionMask;
        [SerializeField] float maxBuildDistance = 20.0f;
        [SerializeField] float maxTerrainHeight = 100.0f;
        [SerializeField] float gridSize = 1.0f;

        void LateUpdate()
        {
            Vector3 buildPoint = SnapToGrid(GetBuildPoint(), false, false);
            Ray skyRay = new Ray(buildPoint + Vector3.up * maxTerrainHeight, Vector3.down);
            bool skyRaySuccess = Physics.Raycast(skyRay, out RaycastHit skyRayHit, maxTerrainHeight*2, buildTargetingCollisionMask);
            buildPointIndicator.position = skyRayHit.point;
        }

        private Vector3 GetBuildPoint() {
            Ray cameraRay = new Ray(cameraTransform.position, cameraTransform.forward);
            bool cameraRaySuccess = Physics.Raycast(cameraRay, out RaycastHit cameraRayHit, maxBuildDistance * 2, buildTargetingCollisionMask);

            if (cameraRaySuccess && Vector3.ProjectOnPlane(cameraTransform.position - cameraRayHit.point, Vector3.up).magnitude < maxBuildDistance) {
                return new Vector3(cameraRayHit.point.x, 0, cameraRayHit.point.z);
            }

            return Vector3.ProjectOnPlane(cameraTransform.position, Vector3.up) + Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up) * maxBuildDistance;
        }

        private Vector3 SnapToGrid(Vector3 vec, bool centerX, bool centerZ) {
            vec /= gridSize;
            Vector3 newVec = new Vector3(Mathf.Round(vec.x), 0, Mathf.Round(vec.z));
            newVec *= gridSize;

            return newVec + new Vector3(centerX ? gridSize/2.0f : 0, 0, centerZ ? gridSize/2.0f : 0);
        }
    }
}
