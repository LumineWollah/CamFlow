using System.Collections.Generic;
using UnityEngine;

namespace Pastis.CamFlow
{
    /// <summary>
    /// Defines a group of targets that should all fit in the camera view.
    /// </summary>
    public sealed class CamFlowTargetGroup : MonoBehaviour
    {
        [Tooltip("List of transforms that must all be visible by the camera.")]
        public List<Transform> targets = new();

        public bool IsValid
        {
            get
            {
                for (int i = targets.Count - 1; i >= 0; i--)
                {
                    if (targets[i] == null)
                        targets.RemoveAt(i);
                }
                return targets.Count > 0;
            }
        }

        /// <summary>
        /// Computes a bounding box enclosing all targets.
        /// </summary>
        public Bounds ComputeBounds()
        {
            Bounds b = new Bounds(targets[0].position, Vector3.zero);
            for (int i = 1; i < targets.Count; i++)
                b.Encapsulate(targets[i].position);
            return b;
        }
    }
}
