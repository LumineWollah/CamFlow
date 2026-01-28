using UnityEngine;
using UnityEngine.InputSystem;

namespace Pastis.CamFlow
{
    /// <summary>
    /// Orchestrates camera behavior by selecting a command source
    /// (input provider or external driver) and applying it to the motor.
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

        [Header("External Driver (optional)")]
        [Tooltip("Optional driver assigned via Inspector (must implement ICamFlowDriver).")]
        [SerializeField] private MonoBehaviour driverBehaviour;

        [Header("Click To Follow")]
        [SerializeField] private bool clickToFollowEnabled = true;
        [SerializeField] private LayerMask followLayerMask = ~0;
        [SerializeField] private float maxPickDistance = 1000f;
        [SerializeField] private bool stopFollowOnMoveInput = true;

        private ICamFlowDriver externalDriver;

        // follow release guards
        private int ignoreReleaseFrames = 0;
        private Vector2 previousMove;

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

            if (driverBehaviour != null && driverBehaviour is ICamFlowDriver d)
                externalDriver = d;
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

        /// <summary>
        /// Allows external scripts to take control of the camera.
        /// </summary>
        public void SetDriver(ICamFlowDriver driver)
        {
            externalDriver = driver;
        }

        public void ClearDriver()
        {
            externalDriver = null;
        }

        private void LateUpdate()
        {
            if (motor == null)
                return;

            // 1) Click-to-follow selection (still allowed even with external driver)
            if (clickToFollowEnabled &&
                Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (TryPickFollowTarget())
                    ignoreReleaseFrames = 2;
            }

            // 2) Choose command source
            CameraCommand cmd = CameraCommand.Empty;
            bool hasExternal =
                externalDriver != null &&
                externalDriver.TryGetCommand(out cmd);

            if (!hasExternal)
            {
                if (input == null) return;
                cmd = input.ReadCommand();
            }

            // 3) Toggle cinematic
            if (cmd.toggleCinematic)
            {
                cinematicEnabled = !cinematicEnabled;
                motor.CinematicEnabled = cinematicEnabled;
            }

            // 4) Follow management (external or input driven)
            if (follower != null)
            {
                if (cmd.clearFollow)
                    follower.ClearTargetAndDisable();

                if (cmd.followTarget != null)
                {
                    follower.SetTarget(cmd.followTarget);
                    follower.SetEnabled(true);
                }
            }

            // 5) Release follow on movement (edge-triggered)
            bool moveStartedThisFrame =
                previousMove.sqrMagnitude <= 0.0001f &&
                cmd.planarMove.sqrMagnitude > 0.0001f;

            previousMove = cmd.planarMove;

            if (ignoreReleaseFrames > 0)
            {
                ignoreReleaseFrames--;
            }
            else if (stopFollowOnMoveInput &&
                     follower != null &&
                     follower.Enabled &&
                     moveStartedThisFrame)
            {
                follower.ClearTargetAndDisable();
            }

            // 6) Apply behavior: Follow overrides Free
            if (follower != null && follower.Enabled && follower.Target != null)
            {
                follower.TickFollow(Time.deltaTime, motor, cmd);
            }
            else
            {
                motor.TickFree(Time.deltaTime, cmd);
            }

            // 7) Bounds
            if (bounds != null && bounds.Enabled)
                motor.ClampPosition(bounds);
        }

        private bool TryPickFollowTarget()
        {
            Camera cam =
                motor.TargetCamera != null ? motor.TargetCamera : Camera.main;

            if (cam == null)
                return false;

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    maxPickDistance,
                    followLayerMask,
                    QueryTriggerInteraction.Ignore))
                return false;

            CamFlowFollowable followable =
                hit.collider.GetComponentInParent<CamFlowFollowable>();

            if (followable == null || follower == null)
                return false;

            Transform t =
                followable.FollowTransform != null
                    ? followable.FollowTransform
                    : followable.transform;

            follower.SetTarget(t);
            follower.SetEnabled(true);

            return true;
        }
    }
}
