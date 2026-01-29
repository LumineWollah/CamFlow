using UnityEngine;
using Pastis.CamFlow;

public class MyDriver : MonoBehaviour, ICamFlowDriver
{
    private CameraController controller;
    private float t = 0f;

    private void OnEnable()
    {
        controller = FindObjectOfType<CameraController>();
        if (controller != null)
            controller.SetDriver(this);
        else
            Debug.LogError("[MyDriver] CameraController not found.");
    }

    private void OnDisable()
    {
        if (controller != null)
            controller.ClearDriver();
    }

    public bool TryGetCommand(out CameraCommand command)
    {
        // Take control only while t < 1
        if (t >= 1f)
        {
            command = default;
            return false;
        }

        t += Time.deltaTime * 0.2f;

        // Example: move forward slowly + cinematic on, no look input
        command = new CameraCommand
        {
            planarMove = new Vector2(0f, 1f),
            verticalMove = 0f,
            lookDelta = Vector2.zero,
            zoomDelta = 0f,
            fast = false,
            slow = true,
            toggleCinematic = false,
            followTarget = null,
            clearFollow = true
        };

        return true;
    }
}
