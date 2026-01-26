using UnityEngine;
using UnityEngine.InputSystem;

namespace Pastis.CamFlow
{
    /// <summary>
    /// Input System-based provider. Actions are created in code so there is no asset dependency.
    /// Rebinding can be done via Input System's API (InteractiveRebindingExtensions).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraInputProvider : MonoBehaviour
    {
        [Header("Bindings (defaults)")]
        [SerializeField] private string moveComposite = "2DVector";
        [SerializeField] private string moveUpBinding = "<Keyboard>/w";
        [SerializeField] private string moveDownBinding = "<Keyboard>/s";
        [SerializeField] private string moveLeftBinding = "<Keyboard>/a";
        [SerializeField] private string moveRightBinding = "<Keyboard>/d";

        [SerializeField] private string lookBinding = "<Mouse>/delta";
        [SerializeField] private string zoomBinding = "<Mouse>/scroll";

        [SerializeField] private string fastBinding = "<Keyboard>/leftShift";
        [SerializeField] private string slowBinding = "<Keyboard>/leftCtrl";

        [SerializeField] private string toggleCinematicBinding = "<Keyboard>/c";
        [SerializeField] private string enableLookButton = "<Mouse>/rightButton";

        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction zoomAction;
        private InputAction fastAction;
        private InputAction slowAction;
        private InputAction toggleCinematicAction;
        private InputAction lookEnableAction;

        private bool togglePressedBuffered;

        public Vector2 Move => moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        public Vector2 LookDelta => (lookEnableAction != null && lookEnableAction.IsPressed())
            ? (lookAction?.ReadValue<Vector2>() ?? Vector2.zero)
            : Vector2.zero;

        public float ZoomDelta => zoomAction?.ReadValue<Vector2>().y ?? 0f;

        public bool Fast => fastAction != null && fastAction.IsPressed();
        public bool Slow => slowAction != null && slowAction.IsPressed();

        private void OnEnable()
        {
            BuildActionsIfNeeded();
            EnableAll();
        }

        private void OnDisable()
        {
            DisableAll();
        }

        private void BuildActionsIfNeeded()
        {
            if (moveAction != null) return;

            // MOVE (WASD 2D composite)
            moveAction = new InputAction("CamFlow.Move", InputActionType.Value);
            var composite = moveAction.AddCompositeBinding(moveComposite);
            composite.With("Up", moveUpBinding);
            composite.With("Down", moveDownBinding);
            composite.With("Left", moveLeftBinding);
            composite.With("Right", moveRightBinding);

            // LOOK (mouse delta), gated by RMB (default)
            lookAction = new InputAction("CamFlow.Look", InputActionType.Value, lookBinding);
            lookEnableAction = new InputAction("CamFlow.LookEnable", InputActionType.Button, enableLookButton);

            // ZOOM (scroll)
            zoomAction = new InputAction("CamFlow.Zoom", InputActionType.Value, zoomBinding);

            // SPEED modifiers
            fastAction = new InputAction("CamFlow.Fast", InputActionType.Button, fastBinding);
            slowAction = new InputAction("CamFlow.Slow", InputActionType.Button, slowBinding);

            // TOGGLE cinematic
            toggleCinematicAction = new InputAction("CamFlow.ToggleCinematic", InputActionType.Button, toggleCinematicBinding);
            toggleCinematicAction.performed += _ => togglePressedBuffered = true;
        }

        private void EnableAll()
        {
            moveAction?.Enable();
            lookAction?.Enable();
            zoomAction?.Enable();
            fastAction?.Enable();
            slowAction?.Enable();
            toggleCinematicAction?.Enable();
            lookEnableAction?.Enable();
        }

        private void DisableAll()
        {
            moveAction?.Disable();
            lookAction?.Disable();
            zoomAction?.Disable();
            fastAction?.Disable();
            slowAction?.Disable();
            toggleCinematicAction?.Disable();
            lookEnableAction?.Disable();
        }

        public bool ConsumeToggleCinematicPressed()
        {
            if (!togglePressedBuffered) return false;
            togglePressedBuffered = false;
            return true;
        }

        // Optional helpers if you want to expose actions for UI rebinding later.
        public InputAction GetMoveAction() => moveAction;
        public InputAction GetLookAction() => lookAction;
        public InputAction GetZoomAction() => zoomAction;
        public InputAction GetToggleCinematicAction() => toggleCinematicAction;
    }
}
