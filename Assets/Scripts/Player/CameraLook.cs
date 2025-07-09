using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public static CameraLook Instance;
    public float sensitivity = 100f;
    public float smoothTime = 0.05f;

    private float yRotation;
    private float currentYRotation;
    private float yRotationVelocity;
    private float targetYRotation;


    private void Awake()
    {
        Instance = this;
    }


    void Update()
    {
        if (GameManager.Instance.isCutscene || ReplayRecordingManager.Instance.isReplay) return;

        if (ReplayRecordingManager.Instance != null && ReplayRecordingManager.Instance.isReplay)
        {
            currentYRotation = Mathf.SmoothDampAngle(currentYRotation, targetYRotation, ref yRotationVelocity, smoothTime);
            transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);
        }
        else
        {
            // Normal live input path (unchanged)
            float mouseX = Input.GetAxisRaw("Mouse X");

            yRotation += mouseX * sensitivity * Time.deltaTime;
            currentYRotation = Mathf.SmoothDamp(currentYRotation, yRotation, ref yRotationVelocity, smoothTime);

            transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);
        }
    }
    public void SetReplayRotation(float rotationY)
    {
        targetYRotation = rotationY;
    }

    public void ForceSetCurrentRotation(float rotationY)
    {
        yRotation = rotationY;
        currentYRotation = rotationY;
        yRotationVelocity = 0f;
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
    }
}
