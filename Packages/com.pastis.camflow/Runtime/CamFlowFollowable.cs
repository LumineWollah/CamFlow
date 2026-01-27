using UnityEngine;

namespace Pastis.CamFlow
{
    /// <summary>
    /// Add this component to any object you want to be clickable/selectable as a follow target.
    /// Optionally set FollowTransform (e.g., a child "Head" transform).
    /// </summary>
    public sealed class CamFlowFollowable : MonoBehaviour
    {
        [SerializeField] private Transform followTransform;

        public Transform FollowTransform => followTransform;
    }
}