using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    public float zOffset = 0f; // gradually animated externally

    void LateUpdate()
    {
        if (Camera.main != null && transform.parent != null)
        {
            Vector3 camPos = Camera.main.transform.position;
            Vector3 lookDirection = camPos - transform.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                Quaternion relativeToParent = Quaternion.Inverse(transform.parent.rotation) * lookRotation;

                // Apply gradual Z offset
                relativeToParent *= Quaternion.Euler(0f, 0f, zOffset);
                transform.localRotation = relativeToParent;
            }
        }
    }
}
