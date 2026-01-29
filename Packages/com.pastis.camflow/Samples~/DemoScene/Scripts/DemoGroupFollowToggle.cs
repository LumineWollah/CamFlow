using UnityEngine;
using UnityEngine.UI;
using Pastis.CamFlow;

public sealed class DemoGroupFollowToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraController controller;
    [SerializeField] private CamFlowTargetGroup group;
    [SerializeField] private Button button;

    private bool isFollowingGroup;

    private void Reset()
    {
        button ??= GetComponent<Button>();
    }

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(Toggle);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(Toggle);
    }

    private void Toggle()
    {
        Debug.Log("[DemoGroupFollowToggle] Toggling group follow.");
        isFollowingGroup = !isFollowingGroup;

        if (isFollowingGroup)
        {
            controller.SetTargetGroup(group);
        }
        else
        {
            controller.ClearTargetGroup();
        }
    }
}
