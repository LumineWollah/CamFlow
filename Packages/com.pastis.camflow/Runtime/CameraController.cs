using UnityEngine;
using UnityEngine.InputSystem;

namespace Pastis.CamFlow
{
    [DisallowMultipleComponent]
    public sealed class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraInputProvider input;
        [SerializeField] private CameraMotor motor;
        [SerializeField] private CameraTargetFollower follower;
        [SerializeField] private CameraBounds bounds;

        [Header("Mode")]
        [SerializeField] private bool cinematicEnabled = true;

        [Header("Selection / Follow")]
        [SerializeField] private bool clickToFollowEnabled = true;
        [SerializeField] private LayerMask followLayerMask = ~0; // everything by default
        [SerializeField] private float maxPickDistance = 1000f;
        [SerializeField] private bool stopFollowOnMoveInput = true;

        private void Reset()
        {
            AutoWire();
        }

        private void Awake()
        {
            AutoWire();

            if (motor == null)
            {
                Debug.LogError("[CamFlow] CameraMotor reference is missing.", this);
                enabled = false;
                return;
            }

            motor.CinematicEnabled = cinematicEnabled;
        }

        private void OnValidate()
        {
            if (motor != null)
                motor.CinematicEnabled = cinematicEnabled;
        }

        private void AutoWire()
        {
            input ??= GetComponent<CameraInputProvider>();
            motor ??= GetComponent<CameraMotor>();
            follower ??= GetComponent<CameraTargetFollower>();
            bounds ??= GetComponent<CameraBounds>();
        }

        private void Update()
        {
            if (input == null || motor == null) return;

            // Toggle cinematic
            if (input.ConsumeToggleCinematicPressed())
            {
                cinematicEnabled = !cinematicEnabled;
                motor.CinematicEnabled = cinematicEnabled;
            }

            // Click-to-follow selection
            if (clickToFollowEnabled && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryPickFollowTarget();
            }

            // Release follow as soon as movement is pressed
            if (stopFollowOnMoveInput && follower != null && follower.Enabled && input.Move.sqrMagnitude > 0.0001f)
            {
                follower.ClearTargetAndDisable();
            }

            // Main behavior: Follow overrides Free
            if (follower != null && follower.Enabled && follower.Target != null)
            {
                follower.TickFollow(Time.deltaTime, motor);
            }
            else
            {
                motor.TickFree(Time.deltaTime, input);
            }

            // Optional bounds clamping
            if (bounds != null && bounds.Enabled)
            {
                motor.ClampPosition(bounds);
            }
        }

        private void TryPickFollowTarget()
        {
            Camera cam = motor.TargetCamera != null ? motor.TargetCamera : Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, maxPickDistance, followLayerMask, QueryTriggerInteraction.Ignore))
                return;

            // Only follow objects that are explicitly taggéd by our component
            CamFlowFollowable followable = hit.collider.GetComponentInParent<CamFlowFollowable>();
            if (followable == null) return;

            Transform t = followable.FollowTransform != null ? followable.FollowTransform : followable.transform;

            if (follower == null)
            {
                Debug.LogWarning("[CamFlow] Click-to-follow is enabled but no CameraTargetFollower is on this rig.", this);
                return;
            }

            follower.SetTarget(t);
            follower.SetEnabled(true);
        }

        public void SetTarget(Transform target)
        {
            if (follower == null)
            {
                Debug.LogWarning("[CamFlow] No CameraTargetFollower on this rig.", this);
                return;
            }

            follower.SetTarget(target);
            follower.SetEnabled(target != null);
        }

        public void SetCinematic(bool enabledValue)
        {
            cinematicEnabled = enabledValue;
            motor.CinematicEnabled = enabledValue;
        }
    }
}
