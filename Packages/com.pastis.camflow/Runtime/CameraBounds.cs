using UnityEngine;

namespace Pastis.CamFlow
{
    /// <summary>
    /// Clamps camera position inside a volume.
    ///
    /// Preferred workflow:
    /// - Create a "Bounds" GameObject in the scene with a BoxCollider (trigger or not)
    /// - Assign it to BoundsRoot on the camera rig
    /// - Enable bounds
    ///
    /// Supports:
    /// - TransformBoxCollider: uses a BoxCollider found on BoundsRoot (or its children if enabled)
    /// - ThisBoxCollider: uses a BoxCollider on the same GameObject
    /// - ParentBoxCollider: uses a BoxCollider on the parent
    /// - ManualWorldAabb: fallback, manual center/size
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraBounds : MonoBehaviour
    {
        public enum BoundsSource
        {
            TransformBoxCollider,
            ParentBoxCollider,
            ThisBoxCollider,
            ManualWorldAabb
        }

        [SerializeField] private bool enabledBounds = false;

        [Header("Source")]
        [SerializeField] private BoundsSource source = BoundsSource.TransformBoxCollider;

        [Tooltip("Used when Source = TransformBoxCollider.")]
        [SerializeField] private Transform boundsRoot;

        [Tooltip("If true, searches for a BoxCollider in children of BoundsRoot. Otherwise requires it on BoundsRoot itself.")]
        [SerializeField] private bool searchInChildren = true;

        [Tooltip("If true, clamp only XZ and keep Y unchanged (useful for RTS).")]
        [SerializeField] private bool clampXZOnly = false;

        [Header("Manual World AABB (fallback)")]
        [SerializeField] private Vector3 manualCenter = Vector3.zero;
        [SerializeField] private Vector3 manualSize = new Vector3(50f, 30f, 50f);

        private BoxCollider cachedCollider;
        private Transform cachedColliderTransform;

        public bool Enabled => enabledBounds;

        public void SetEnabled(bool enabledValue) => enabledBounds = enabledValue;

        private void Awake()
        {
            CacheCollider();
        }

        private void OnValidate()
        {
            CacheCollider();
        }

        private void CacheCollider()
        {
            cachedCollider = null;
            cachedColliderTransform = null;

            switch (source)
            {
                case BoundsSource.ThisBoxCollider:
                    cachedCollider = GetComponent<BoxCollider>();
                    break;

                case BoundsSource.ParentBoxCollider:
                    if (transform.parent != null)
                        cachedCollider = transform.parent.GetComponent<BoxCollider>();
                    break;

                case BoundsSource.TransformBoxCollider:
                    if (boundsRoot != null)
                    {
                        cachedCollider = searchInChildren
                            ? boundsRoot.GetComponentInChildren<BoxCollider>()
                            : boundsRoot.GetComponent<BoxCollider>();
                    }
                    break;

                case BoundsSource.ManualWorldAabb:
                default:
                    break;
            }

            if (cachedCollider != null)
                cachedColliderTransform = cachedCollider.transform;
        }

        /// <summary>
        /// Clamp a world position to the configured bounds.
        /// </summary>
        public Vector3 Clamp(Vector3 worldPos)
        {
            if (source == BoundsSource.ManualWorldAabb)
                return ClampToManual(worldPos);

            if (cachedCollider == null || cachedColliderTransform == null)
                return ClampToManual(worldPos); // safe fallback

            return ClampToBoxCollider(worldPos, cachedCollider, cachedColliderTransform);
        }

        private Vector3 ClampToManual(Vector3 pos)
        {
            Vector3 half = manualSize * 0.5f;

            float x = Mathf.Clamp(pos.x, manualCenter.x - half.x, manualCenter.x + half.x);
            float y = clampXZOnly ? pos.y : Mathf.Clamp(pos.y, manualCenter.y - half.y, manualCenter.y + half.y);
            float z = Mathf.Clamp(pos.z, manualCenter.z - half.z, manualCenter.z + half.z);

            return new Vector3(x, y, z);
        }

        private Vector3 ClampToBoxCollider(Vector3 worldPos, BoxCollider col, Transform t)
        {
            // Convert world position into collider local space
            Vector3 localPos = t.InverseTransformPoint(worldPos);

            // collider.center and collider.size are in local space.
            Vector3 half = col.size * 0.5f;

            float minX = col.center.x - half.x;
            float maxX = col.center.x + half.x;
            float minY = col.center.y - half.y;
            float maxY = col.center.y + half.y;
            float minZ = col.center.z - half.z;
            float maxZ = col.center.z + half.z;

            localPos.x = Mathf.Clamp(localPos.x, minX, maxX);
            if (!clampXZOnly)
                localPos.y = Mathf.Clamp(localPos.y, minY, maxY);
            localPos.z = Mathf.Clamp(localPos.z, minZ, maxZ);

            // Back to world
            return t.TransformPoint(localPos);
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabledBounds) return;

            // Try to draw current source (not cached, so it's visible in edit mode changes)
            BoxCollider col = null;

            switch (source)
            {
                case BoundsSource.ThisBoxCollider:
                    col = GetComponent<BoxCollider>();
                    break;

                case BoundsSource.ParentBoxCollider:
                    if (transform.parent != null)
                        col = transform.parent.GetComponent<BoxCollider>();
                    break;

                case BoundsSource.TransformBoxCollider:
                    if (boundsRoot != null)
                        col = searchInChildren ? boundsRoot.GetComponentInChildren<BoxCollider>() : boundsRoot.GetComponent<BoxCollider>();
                    break;
            }

            if (source != BoundsSource.ManualWorldAabb && col != null)
            {
                Gizmos.matrix = col.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(col.center, col.size);
            }
            else
            {
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.DrawWireCube(manualCenter, manualSize);
            }
        }
    }
}
