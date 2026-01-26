using UnityEngine;

namespace Pastis.CamFlow
{
    /// <summary>
    /// Axis-aligned bounds clamping in world space.
    /// v1 uses center + size (AABB). Later you can support BoxCollider, polygon, etc.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraBounds : MonoBehaviour
    {
        [SerializeField] private bool enabledBounds = false;

        [Header("World AABB")]
        [SerializeField] private Vector3 center = Vector3.zero;
        [SerializeField] private Vector3 size = new Vector3(50f, 30f, 50f);

        public bool Enabled => enabledBounds;

        public void SetEnabled(bool enabledValue) => enabledBounds = enabledValue;

        public Vector3 Clamp(Vector3 position)
        {
            Vector3 half = size * 0.5f;
            float x = Mathf.Clamp(position.x, center.x - half.x, center.x + half.x);
            float y = Mathf.Clamp(position.y, center.y - half.y, center.y + half.y);
            float z = Mathf.Clamp(position.z, center.z - half.z, center.z + half.z);
            return new Vector3(x, y, z);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
