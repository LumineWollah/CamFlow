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

        // NOTE: These are "Forward/Backward" but the Input System composite uses labels Up/Down.
        [SerializeField] private string moveForwardBinding = "<Keyboard>/w";
        [SerializeField] private string moveBackwardBinding = "<Keyboard>/s";
        [SerializeField] private string moveLeftBinding = "<Keyboard>/a";
        [SerializeField] private string moveRightBinding = "<Keyboard>/d";

        [Header("Vertical Movement (World Up/Down)")]
        // As requested: Q goes UP, E goes DOWN.
        [SerializeField] private string moveUpBinding = "<Keyboard>/q";
        [SerializeField] private string moveDownBinding = "<Keyboard>/e";

        [Header("Look / Zoom")]
        [SerializeField] private string lookBinding = "<Mouse>/delta";
        [SerializeField] private string zoomBinding = "<Mouse>/scroll";
        [SerializeField] private string enableLookButton = "<Mouse>/rightButton";

        [Header("Speed Modifiers")]
        [SerializeField] private string fastBinding = "<Keyboard>/leftShift";
        [SerializeField] private string slowBinding = "<Keyboard>/leftCtrl";

        [Header("Toggles")]
        [SerializeField] private string toggleCinematicBinding = "<Keyboard>/c";

        private InputAction moveAction;            // 2D (x=strafe, y=forward)
        private InputAction verticalMoveAction;    // 1D (up/down world)
        private InputAction lookAction;
        private InputAction zoomAction;
        private InputAction fastAction;
        private InputAction slowAction;
        private InputAction toggleCinematicAction;
        private InputAction lookEnableAction;

        private bool togglePressedBuffered;

        /// <summary>
        /// X = strafe, Y = forward (not world up/down).
        /// </summary>
        public Vector2 Move => moveAction?.ReadValue<Vector2>() ?? Vector2.zero;

        /// <summary>
        /// +1 when pressing Q (up), -1 when pressing E (down).
        /// </summary>
        public float VerticalMove => verticalMoveAction?.ReadValue<float>() ?? 0f;

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
            // Composite uses labels Up/Down, but semantically that's Forward/Backward for our camera.
            moveAction = new InputAction("CamFlow.Move", InputActionType.Value);
            var composite = moveAction.AddCompositeBinding(moveComposite);
            composite.With("Up", moveForwardBinding);
            composite.With("Down", moveBackwardBinding);
            composite.With("Left", moveLeftBinding);
            composite.With("Right", moveRightBinding);

            // VERTICAL MOVE (Q/E 1D axis): Q = up (+), E = down (-)
            verticalMoveAction = new InputAction("CamFlow.VerticalMove", InputActionType.Value);
            verticalMoveAction.AddCompositeBinding("1DAxis")
                .With("Positive", moveUpBinding)
                .With("Negative", moveDownBinding);

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
            verticalMoveAction?.Enable();
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
            verticalMoveAction?.Disable();
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
        public InputAction GetVerticalMoveAction() => verticalMoveAction;
        public InputAction GetLookAction() => lookAction;
        public InputAction GetZoomAction() => zoomAction;
        public InputAction GetToggleCinematicAction() => toggleCinematicAction;

        public CameraCommand ReadCommand()
        {
            return new CameraCommand
            {
                planarMove = Move,
                verticalMove = VerticalMove,
                lookDelta = LookDelta,
                zoomDelta = ZoomDelta,
                fast = Fast,
                slow = Slow,
                toggleCinematic = ConsumeToggleCinematicPressed(),
                requestFollow = false,
                followTarget = null,
                clearFollow = false
            };
        }
    }
}
