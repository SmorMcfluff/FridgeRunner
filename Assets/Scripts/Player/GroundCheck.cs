using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    public bool IsGrounded()
    {
        Vector3 boxCenter = transform.position;
        Vector3 boxHalfExtents = transform.localScale / 2;
        return Physics.CheckBox(
            boxCenter,
            boxHalfExtents,
            Quaternion.identity,
            groundLayer
        );
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
#endif
}
