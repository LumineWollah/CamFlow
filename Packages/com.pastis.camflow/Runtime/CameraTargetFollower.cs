using UnityEngine;

namespace Pastis.CamFlow
{
    /// <summary>
    /// Simple follow module: provides desired position/rotation for the motor.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraTargetFollower : MonoBehaviour
    {
        [SerializeField] private bool enabledFollow = false;
        [SerializeField] private Transform target;

        [Header("Follow")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -8f);
        [SerializeField] private bool lookAtTarget = true;

        public bool Enabled => enabledFollow;
        public Transform Target => target;

        public void SetTarget(Transform t)
        {
            target = t;
            enabledFollow = (target != null);
        }

        public void SetEnabled(bool enabledValue) => enabledFollow = enabledValue;

        public void TickFollow(float dt, CameraMotor motor)
        {
            if (!enabledFollow || target == null) return;

            Vector3 desiredPos = target.position + offset;

            Quaternion desiredRot;
            if (lookAtTarget)
            {
                Vector3 dir = (target.position - desiredPos);
                desiredRot = dir.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(dir.normalized, Vector3.up) : motor.transform.rotation;
            }
            else
            {
                desiredRot = motor.transform.rotation;
            }

            motor.TickFollow(dt, desiredPos, desiredRot);
        }
    }
}
