using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SphereCollider))]
public class Pickup : MonoBehaviour
{
    private enum Type { Keycard, Gun };
    [SerializeField] private Type type;
    public UnityEvent extraEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (type)
            {
                case Type.Keycard:
                    if (Player.Instance.hasKeycard) return;
                    Player.Instance.hasKeycard = true;
                    Player.Instance.keycardPrompt.enabled = true;
                    break;

                case Type.Gun:
                    if (Player.Instance.hasGun) return;
                    Player.Instance.hasGun = true;
                    if (!ReplayRecordingManager.Instance.isReplay)
                    {
                        Player.Instance.SwitchHand();
                    }
                    break;

                default: break;
            }

            extraEvent?.Invoke();
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if (Camera.main == null) return;

        Vector3 direction = transform.position - Camera.main.transform.position;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
