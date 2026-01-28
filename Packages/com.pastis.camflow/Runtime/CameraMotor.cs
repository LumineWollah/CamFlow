using UnityEngine;

namespace Pastis.CamFlow
{
    /// <summary>
    /// Applies translation, rotation and zoom to a camera rig.
    /// Driven by CameraCommand (virtualized input).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraMotor : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float fastMultiplier = 2.5f;
        [SerializeField] private float slowMultiplier = 0.35f;

        [Header("Rotation")]
        [SerializeField] private float yawSpeed = 0.15f;
        [SerializeField] private float pitchSpeed = 0.15f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        [Header("Zoom (FOV)")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minFov = 20f;
        [SerializeField] private float maxFov = 80f;

        [Header("Cinematic (smoothing)")]
        [SerializeField] private float positionSmoothTime = 0.12f;
        [SerializeField] private float rotationSmoothTime = 0.10f;
        [SerializeField] private float fovSmoothTime = 0.08f;

        public bool CinematicEnabled { get; set; } = true;
        public Camera TargetCamera => targetCamera;

        private Vector3 desiredPosition;
        private Quaternion desiredRotation;
        private float desiredFov;

        private Vector3 positionVelocity;
        private float fovVelocity;

        private float desiredYaw;
        private float desiredPitch;

        private void Reset()
        {
            targetCamera ??= GetComponent<Camera>();
        }

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = GetComponentInChildren<Camera>();

            desiredPosition = transform.position;
            desiredRotation = transform.rotation;

            Vector3 euler = transform.rotation.eulerAngles;
            desiredYaw = euler.y;
            desiredPitch = NormalizePitch(euler.x);

            if (targetCamera != null)
                desiredFov = targetCamera.fieldOfView;
        }

        /// <summary>
        /// Free camera movement driven by a CameraCommand.
        /// </summary>
        public void TickFree(float dt, in CameraCommand cmd)
        {
            float speed = moveSpeed;
            if (cmd.fast) speed *= fastMultiplier;
            if (cmd.slow) speed *= slowMultiplier;

            Vector3 right = transform.right;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            desiredPosition +=
                (right * cmd.planarMove.x +
                 forward * cmd.planarMove.y +
                 Vector3.up * cmd.verticalMove) * (speed * dt);

            // Rotation
            desiredYaw += cmd.lookDelta.x * yawSpeed;
            desiredPitch -= cmd.lookDelta.y * pitchSpeed;
            desiredPitch = Mathf.Clamp(desiredPitch, minPitch, maxPitch);

            desiredRotation = Quaternion.Euler(desiredPitch, desiredYaw, 0f);

            // Zoom
            if (targetCamera != null && Mathf.Abs(cmd.zoomDelta) > 0.001f)
            {
                desiredFov = Mathf.Clamp(
                    desiredFov - cmd.zoomDelta * zoomSpeed,
                    minFov,
                    maxFov
                );
            }

            Apply(dt);
        }

        /// <summary>
        /// Follow mode: motor is directly driven by an external system (follower).
        /// </summary>
        public void TickFollow(float dt, Vector3 followPosition, Quaternion followRotation, float? followFov = null)
        {
            desiredPosition = followPosition;
            desiredRotation = followRotation;

            if (followFov.HasValue && targetCamera != null)
                desiredFov = Mathf.Clamp(followFov.Value, minFov, maxFov);

            Apply(dt);
        }

        private void Apply(float dt)
        {
            if (!CinematicEnabled)
            {
                transform.SetPositionAndRotation(desiredPosition, desiredRotation);
                if (targetCamera != null)
                    targetCamera.fieldOfView = desiredFov;
                return;
            }

            // Position smoothing
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref positionVelocity,
                positionSmoothTime,
                Mathf.Infinity,
                dt
            );

            // Rotation smoothing (Quaternion-based, works for LookAt & moving targets)
            float rotT = 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, rotationSmoothTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotT);

            // FOV smoothing
            if (targetCamera != null)
            {
                targetCamera.fieldOfView = Mathf.SmoothDamp(
                    targetCamera.fieldOfView,
                    desiredFov,
                    ref fovVelocity,
                    fovSmoothTime,
                    Mathf.Infinity,
                    dt
                );
            }
        }

        public void ClampPosition(CameraBounds bounds)
        {
            desiredPosition = bounds.Clamp(desiredPosition);

            if (!CinematicEnabled)
                transform.position = desiredPosition;
        }

        private static float NormalizePitch(float x)
        {
            x = Mathf.Repeat(x + 180f, 360f) - 180f;
            return x;
        }
    }
}
