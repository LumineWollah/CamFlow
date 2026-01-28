using UnityEngine;

namespace Pastis.CamFlow
{
    /// <summary>
    /// A per-frame command describing how the camera should move.
    /// All values are expressed in the camera rig's local frame unless stated otherwise.
    /// </summary>
    public struct CameraCommand
    {
        /// <summary> X = strafe, Y = forward </summary>
        public Vector2 planarMove;

        /// <summary> +up / -down in world up </summary>
        public float verticalMove;

        /// <summary> Mouse/controller look delta (yaw/pitch) </summary>
        public Vector2 lookDelta;

        /// <summary> Scroll delta (positive means zoom in by convention in your motor) </summary>
        public float zoomDelta;

        /// <summary> Speed modifiers </summary>
        public bool fast;
        public bool slow;

        /// <summary> If true, toggle cinematic mode this frame </summary>
        public bool toggleCinematic;

        /// <summary>
        /// If true, the driver wants to drive the camera via follow/orbit target logic (optional).
        /// You can ignore this if you prefer: driver can also directly set rig pose via a custom motor later.
        /// </summary>
        public bool requestFollow;

        /// <summary> Optional follow target request </summary>
        public Transform followTarget;

        /// <summary> If true, clear current follow target </summary>
        public bool clearFollow;

        public static CameraCommand Empty => default;
    }
}
