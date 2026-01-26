using UnityEngine;

namespace Pastis.CamFlow
{
    /// <summary>
    /// Applies translation/rotation/zoom to a rig (Transform).
    /// Put this on the same object you want to move/rotate (often the Camera or a parent rig).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraMotor : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float fastMultiplier = 2.5f;
        [SerializeField] private float slowMultiplier = 0.35f;

        [Header("Rotation")]
        [SerializeField] private float yawSpeed = 0.15f;   // degrees per pixel-ish (scaled)
        [SerializeField] private float pitchSpeed = 0.15f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        [Header("Zoom")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float zoomSpeed = 2f; // FOV step per scroll unit
        [SerializeField] private float minFov = 20f;
        [SerializeField] private float maxFov = 80f;

        [Header("Cinematic (smoothing)")]
        [SerializeField] private float positionSmoothTime = 0.12f;
        [SerializeField] private float rotationSmoothTime = 0.10f;
        [SerializeField] private float fovSmoothTime = 0.08f;

        public bool CinematicEnabled { get; set; } = true;

        private Vector3 desiredPosition;
        private Vector3 positionVelocity;

        private Quaternion desiredRotation;
        private float rotationVelocity; // for SmoothDampAngle-based yaw/pitch
        private float desiredYaw;
        private float desiredPitch;

        private float desiredFov;
        private float fovVelocity;

        private void Reset()
        {
            targetCamera ??= GetComponent<Camera>();
        }

        private void Awake()
        {
            if (targetCamera == null) targetCamera = GetComponentInChildren<Camera>();
            desiredPosition = transform.position;
            desiredRotation = transform.rotation;

            var euler = transform.rotation.eulerAngles;
            desiredYaw = euler.y;
            desiredPitch = NormalizePitch(euler.x);

            if (targetCamera != null)
                desiredFov = targetCamera.fieldOfView;
        }

        public void TickFree(float dt, CameraInputProvider input)
        {
            // Translation in local XZ plane (world up)
            Vector2 move = input.Move;
            float speed = moveSpeed;
            if (input.Fast) speed *= fastMultiplier;
            if (input.Slow) speed *= slowMultiplier;

            Vector3 right = transform.right;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            desiredPosition += (right * move.x + forward * move.y) * (speed * dt);

            // Rotation
            Vector2 look = input.LookDelta;
            desiredYaw += look.x * yawSpeed;
            desiredPitch -= look.y * pitchSpeed;
            desiredPitch = Mathf.Clamp(desiredPitch, minPitch, maxPitch);

            desiredRotation = Quaternion.Euler(desiredPitch, desiredYaw, 0f);

            // Zoom (FOV)
            if (targetCamera != null)
            {
                float zoom = input.ZoomDelta;
                if (Mathf.Abs(zoom) > 0.001f)
                {
                    desiredFov = Mathf.Clamp(desiredFov - zoom * zoomSpeed, minFov, maxFov);
                }
            }

            Apply(dt);
        }

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
                if (targetCamera != null) targetCamera.fieldOfView = desiredFov;
                return;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref positionVelocity, positionSmoothTime, Mathf.Infinity, dt);

            // Smooth rotation by smoothing yaw/pitch angles
            Vector3 euler = transform.rotation.eulerAngles;
            float currentYaw = euler.y;
            float currentPitch = NormalizePitch(euler.x);

            float newYaw = Mathf.SmoothDampAngle(currentYaw, desiredYaw, ref rotationVelocity, rotationSmoothTime, Mathf.Infinity, dt);
            // reuse velocity for simplicity; acceptable for v1. You can split yaw/pitch later.
            float newPitch = Mathf.Lerp(currentPitch, desiredPitch, 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, rotationSmoothTime)));

            transform.rotation = Quaternion.Euler(newPitch, newYaw, 0f);

            if (targetCamera != null)
            {
                targetCamera.fieldOfView = Mathf.SmoothDamp(targetCamera.fieldOfView, desiredFov, ref fovVelocity, fovSmoothTime, Mathf.Infinity, dt);
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
