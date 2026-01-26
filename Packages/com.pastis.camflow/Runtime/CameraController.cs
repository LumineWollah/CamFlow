using UnityEngine;

namespace Pastis.CamFlow
{
    /// <summary>
    /// Facade that wires Input -> Motor (+ optional Follow/Bounds).
    /// Attach to a Camera (or a parent rig), then assign references in inspector.
    /// </summary>
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

        private void Reset()
        {
            // Best-effort auto-wiring if components are on same GameObject.
            input ??= GetComponent<CameraInputProvider>();
            motor ??= GetComponent<CameraMotor>();
            follower ??= GetComponent<CameraTargetFollower>();
            bounds ??= GetComponent<CameraBounds>();
        }

        private void OnValidate()
        {
            if (motor != null)
                motor.CinematicEnabled = cinematicEnabled;
        }

        private void Awake()
        {
            if (motor == null)
            {
                Debug.LogError("[CamFlow] CameraMotor reference is missing.", this);
                enabled = false;
                return;
            }

            motor.CinematicEnabled = cinematicEnabled;
        }

        private void Update()
        {
            if (input == null) return;

            if (input.ConsumeToggleCinematicPressed())
            {
                cinematicEnabled = !cinematicEnabled;
                motor.CinematicEnabled = cinematicEnabled;
            }

            if (follower != null && follower.Enabled && follower.Target != null)
            {
                // Follow drives desired rig position/orientation.
                follower.TickFollow(Time.deltaTime, motor);
            }
            else
            {
                // Free mode drives motion from input.
                motor.TickFree(Time.deltaTime, input);
            }

            if (bounds != null && bounds.Enabled)
            {
                motor.ClampPosition(bounds);
            }
        }

        /// <summary> Public API: assign a follow target at runtime. </summary>
        public void SetTarget(Transform target)
        {
            if (follower == null)
            {
                Debug.LogWarning("[CamFlow] No CameraTargetFollower on this rig.", this);
                return;
            }

            follower.SetTarget(target);
        }

        public void SetCinematic(bool enabledValue)
        {
            cinematicEnabled = enabledValue;
            motor.CinematicEnabled = enabledValue;
        }
    }
}
