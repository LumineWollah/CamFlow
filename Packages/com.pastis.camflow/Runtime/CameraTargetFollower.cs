using UnityEngine;

namespace Pastis.CamFlow
{
    /// <summary>
    /// Follow + optional orbit (RMB drag via input.LookDelta) around the target.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraTargetFollower : MonoBehaviour
    {
        [SerializeField] private bool enabledFollow = false;
        [SerializeField] private Transform target;

        [Header("Follow / Look")]
        [SerializeField] private bool lookAtTarget = true;

        [Header("Orbit (Follow mode)")]
        [SerializeField] private bool orbitEnabled = true;
        [SerializeField] private float orbitYawSpeed = 0.15f;
        [SerializeField] private float orbitPitchSpeed = 0.15f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        [Tooltip("Default offset used to initialize orbit angles/distance.")]
        [SerializeField] private Vector3 defaultOffset = new Vector3(0f, 6f, -8f);

        // Orbit state (around target)
        private float yaw;      // degrees
        private float pitch;    // degrees
        private float distance; // units

        public bool Enabled => enabledFollow;
        public Transform Target => target;

        public void SetTarget(Transform t)
        {
            target = t;
            if (target != null)
            {
                enabledFollow = true;
                InitializeOrbitFromOffset(defaultOffset);
            }
            else
            {
                enabledFollow = false;
            }
        }

        public void SetEnabled(bool enabledValue) => enabledFollow = enabledValue;

        public void ClearTargetAndDisable()
        {
            target = null;
            enabledFollow = false;
        }

        private void InitializeOrbitFromOffset(Vector3 offset)
        {
            // Convert offset -> yaw/pitch/distance
            distance = Mathf.Max(0.01f, offset.magnitude);

            // We interpret offset as "camera position relative to target"
            Vector3 dir = (offset / distance).normalized;

            // yaw around world up
            yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            // pitch: positive = looking down from above (Unity pitch convention varies; this works well)
            pitch = Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f)) * Mathf.Rad2Deg;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        private Vector3 ComputeOrbitOffset()
        {
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

            // Start from a "back" vector so default looks like (0,?, -distance) when yaw=0 pitch=0
            Vector3 offset = rot * (Vector3.back * distance);
            return offset;
        }

        /// <summary>
        /// Follow tick. If orbitEnabled, RMB drag (input.LookDelta) updates orbit yaw/pitch.
        /// </summary>
        public void TickFollow(float dt, CameraMotor motor, in CameraCommand cmd)
        {
            if (!enabledFollow || target == null) return;

            if (orbitEnabled)
            {
                Vector2 look = cmd.lookDelta;
                if (look.sqrMagnitude > 0.000001f)
                {
                    yaw += look.x * orbitYawSpeed;
                    pitch -= look.y * orbitPitchSpeed;
                    pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                }
            }

            Vector3 desiredPos = target.position + ComputeOrbitOffset();

            Quaternion desiredRot = lookAtTarget
                ? Quaternion.LookRotation((target.position - desiredPos).normalized, Vector3.up)
                : motor.transform.rotation;

            motor.TickFollow(dt, desiredPos, desiredRot);
        }
    }
}
