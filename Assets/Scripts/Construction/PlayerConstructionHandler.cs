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

        void Start()
        {
            
        }

        void Update()
        {
            Vector3? buildPoint = GetBuildPoint();
            Debug.Log(buildPoint);
            if (buildPoint != null) {
                buildPointIndicator.position = (Vector3)buildPoint;
            }
        }

        public Vector3? GetBuildPoint() {
            Ray cameraRay = new Ray(cameraTransform.position, cameraTransform.forward);
            bool cameraRaySuccess = Physics.Raycast(cameraRay, out RaycastHit cameraRayHit, maxBuildDistance * 2, buildTargetingCollisionMask);

            if (cameraRaySuccess) {
                return cameraRayHit.point;
            }
            else {
                Vector3 orig = playerTransform.position + Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up) * maxBuildDistance + Vector3.up * maxTerrainHeight;
                Ray skyRay = new Ray(orig, Vector3.down);
                bool skyRaySuccess = Physics.Raycast(skyRay, out RaycastHit skyRayHit, maxTerrainHeight*2, buildTargetingCollisionMask);
                
                if (skyRaySuccess) {
                    return skyRayHit.point;
                }
            }

            return null;
        }
    }
}
