using UnityEngine;
using System.Collections.Generic;

namespace FC_ParkourSystem
{
    /// <summary>
    /// Data structure for ledge/climb point information
    /// </summary>
    [System.Serializable]
    public struct ClimbLedgeData
    {
        public RaycastHit ledgeHit;
        public Vector3 climbUpPoint;
        public Vector3 hangPoint;
        public bool isValid;
    }

    /// <summary>
    /// Data structure for wall detection
    /// </summary>
    [System.Serializable]
    public struct WallCheckData
    {
        public bool isWall;
        public RaycastHit wallHit;
        public float distanceToWall;
    }

    /// <summary>
    /// Helper class for environment scanning (ledges, walls, obstacles)
    /// </summary>
    public class EnvironmentScanner
    {
        private Transform playerTransform;
        private LayerMask climbLayer;
        private float detectionRange;

        public EnvironmentScanner(Transform player, LayerMask layer, float range)
        {
            playerTransform = player;
            climbLayer = layer;
            detectionRange = range;
        }

        /// <summary>
        /// Check if player can drop to a ledge below
        /// </summary>
        public bool DropLedgeCheck(Vector3 direction, out ClimbLedgeData ledgeData)
        {
            ledgeData = new ClimbLedgeData();

            // Cast ray downward from player position
            Vector3 origin = playerTransform.position;
            Vector3 rayDirection = Vector3.down;

            if (Physics.Raycast(origin, rayDirection, out RaycastHit hit, detectionRange, climbLayer))
            {
                // Found ledge below
                ledgeData.ledgeHit = hit;
                ledgeData.hangPoint = hit.point + Vector3.up * 0.1f; // Slightly above ledge
                ledgeData.climbUpPoint = hit.point + Vector3.up * 1.5f; // Standing position
                ledgeData.isValid = true;
                return true;
            }

            // Also check forward and down (for ledges in front)
            origin = playerTransform.position + direction * 0.5f;
            if (Physics.Raycast(origin, rayDirection, out hit, detectionRange, climbLayer))
            {
                ledgeData.ledgeHit = hit;
                ledgeData.hangPoint = hit.point + Vector3.up * 0.1f;
                ledgeData.climbUpPoint = hit.point + Vector3.up * 1.5f;
                ledgeData.isValid = true;
                return true;
            }

            ledgeData.isValid = false;
            return false;
        }

        /// <summary>
        /// Check for climbable ledge in front and above
        /// </summary>
        public bool LedgeCheck(Vector3 direction, out ClimbLedgeData ledgeData)
        {
            ledgeData = new ClimbLedgeData();

            // Check forward at chest height
            Vector3 origin = playerTransform.position + Vector3.up * 1.5f;
            
            if (Physics.Raycast(origin, direction, out RaycastHit wallHit, detectionRange, climbLayer))
            {
                // Found wall, now check for ledge on top
                Vector3 ledgeCheckOrigin = wallHit.point + Vector3.up * 2f - direction * 0.2f;
                
                if (Physics.Raycast(ledgeCheckOrigin, Vector3.down, out RaycastHit ledgeHit, 2.5f, climbLayer))
                {
                    ledgeData.ledgeHit = ledgeHit;
                    ledgeData.hangPoint = ledgeHit.point - direction * 0.3f + Vector3.up * 0.1f;
                    ledgeData.climbUpPoint = ledgeHit.point + Vector3.up * 1.5f;
                    ledgeData.isValid = true;
                    return true;
                }
            }

            ledgeData.isValid = false;
            return false;
        }

        /// <summary>
        /// Check for obstacles at specific height
        /// </summary>
        public bool ObstacleCheck(Vector3 direction, float height, out RaycastHit hit)
        {
            Vector3 origin = playerTransform.position + Vector3.up * height;
            return Physics.Raycast(origin, direction, out hit, detectionRange, climbLayer);
        }
    }

    /// <summary>
    /// Represents a climbable point in the world
    /// </summary>
    public class ClimbPoint : MonoBehaviour
    {
        [Header("Climb Point Settings")]
        public Transform mountPoint;
        public Transform dismountPoint;
        public bool isFreeHang = false;

        [Header("Connections")]
        public List<ClimbPoint> connectedPoints = new List<ClimbPoint>();

        private void OnDrawGizmos()
        {
            // Draw mount point
            if (mountPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(mountPoint.position, 0.2f);
                Gizmos.DrawLine(transform.position, mountPoint.position);
            }

            // Draw dismount point
            if (dismountPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(dismountPoint.position, 0.2f);
                Gizmos.DrawLine(transform.position, dismountPoint.position);
            }

            // Draw connections
            Gizmos.color = Color.yellow;
            foreach (var point in connectedPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawLine(transform.position, point.transform.position);
                }
            }
        }
    }
}
