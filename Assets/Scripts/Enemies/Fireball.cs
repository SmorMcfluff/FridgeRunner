using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 5f;
    public float damage = 1f;

    private Vector3 direction;

    public void Setup(Vector3 shootDirection, float projectileSpeed, float customDamage)
    {
        direction = shootDirection.normalized;
        speed = projectileSpeed;
        damage = customDamage;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        float distanceThisFrame = speed * Time.deltaTime;
        Ray ray = new Ray(transform.position, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, distanceThisFrame))
        {
            OnHit(hit.collider);
            return;
        }

        transform.position += direction * distanceThisFrame;
        BillboardSprite();
    }

    private void OnHit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player.Instance.heat.ChangeTemp(damage, 0.5f);
        }

        if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }


    private void BillboardSprite()
    {
        if (transform.childCount == 0 || Camera.main == null) return;
        Transform spriteChild = transform.GetChild(0);
        spriteChild.forward = Camera.main.transform.forward;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player.Instance.heat.ChangeTemp(damage, 0.5f);
        }

        if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
