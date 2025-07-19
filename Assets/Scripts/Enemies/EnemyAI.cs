using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyType
    {
        Regular,
        Fast,
        Heavy,
        Burst
    }
    public EnemyType type = EnemyType.Regular;

    [Header("Audio")]
    private AudioSource source;
    public AudioClip[] detectedSounds;
    public AudioClip fireSound;

    [Header("Player Target")]
    public Transform player;

    [Header("Wander Settings")]
    private float wanderRadius = 50f;
    private float wanderTimer = 2f;
    private float wanderCooldown;


    [Header("Ranges")]
    public float followRange = 10f;
    public float attackRange = 1.5f;
    public float minAttackDelay = 1f;
    public float maxAttackDelay = 3f;

    [Header("Projectile")]
    public GameObject projectilePrefab;

    private NavMeshAgent agent;
    private Health health;
    private EnemySprite sprite;
    private bool isAttacking;
    private bool playerWasVisible = false;

    [Header("Replay stuff")]
    private System.Random random;
    [SerializeField]
    private int randomSeed;
    public int RandomSeed => randomSeed;
    public string enemyId;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        source = GetComponent<AudioSource>();
        sprite = GetComponentInChildren<EnemySprite>();

        ApplyEnemyTypeSettings();

        if (WaveManager.Instance != null)
        {
            enemyId += WaveManager.Instance.waveNumber.ToString() + "_";
        }

        string compactName = Regex.Match(gameObject.name, @"\d+").Value;
        if (compactName == string.Empty)
        {
            compactName = "0";
        }
        gameObject.name = compactName;
        enemyId += gameObject.name;
    }

    void Start()
    {
        if (player == null && Player.Instance != null)
        {
            player = Player.Instance.transform;
        }

        if (ReplayRecordingManager.Instance.isReplay)
        {
            randomSeed = ReplayEnemyTracker.Instance.LoadEnemy(enemyId);
            random = new System.Random(randomSeed);
        }
        else
        {
            randomSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            random = new System.Random(randomSeed);
            ReplayEnemyTracker.Instance.RegisterEnemy(enemyId, RandomSeed);
        }

        wanderCooldown = wanderTimer;
    }

    void Update()
    {
        if (health.IsDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool playerInRange = distance <= followRange;

        bool playerVisible = CanHitPlayer();

        if (playerVisible && !playerWasVisible)
        {
            OnPlayerSpotted();
        }

        playerWasVisible = playerVisible;

        if (playerVisible)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            if (!isAttacking)
            {
                StartCoroutine(AttackRoutine());
            }
        }
        else
        {
            Wander();
        }
    }

    private void Wander()
    {
        agent.isStopped = false;
        agent.autoBraking = true;

        wanderCooldown -= Time.deltaTime;
        if (wanderCooldown <= 0f)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            wanderCooldown = wanderTimer;
        }
    }


    void ApplyEnemyTypeSettings()
    {
        switch (type)
        {
            case EnemyType.Regular:
                agent.speed = 4f;
                agent.stoppingDistance = 8f;
                followRange = 20f;
                attackRange = 15f;
                health.maxHealth = 5;
                if (sprite) sprite.defaultColor = Color.white;
                break;

            case EnemyType.Fast:
                agent.speed = 8f;
                agent.stoppingDistance = 15f;
                followRange = 40f;
                attackRange = 40f;
                health.maxHealth = 2;
                if (sprite) sprite.defaultColor = Color.yellow;
                break;

            case EnemyType.Heavy:
                agent.speed = 3f;
                agent.stoppingDistance = 0f;
                followRange = 20f;
                attackRange = 20f;
                health.maxHealth = 10;
                if (sprite) sprite.defaultColor = Color.green;
                break;

            case EnemyType.Burst:
                agent.speed = 4f;
                agent.stoppingDistance = 8f;
                followRange = 20f;
                attackRange = 40f;
                health.maxHealth = 5;
                if (sprite) sprite.defaultColor = Color.blue;
                break;
        }
    }

    private void OnPlayerSpotted()
    {
        if (health.IsDead) return;
        PlayRandomSound();
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        while (!health.IsDead && CanHitPlayer())
        {
            if (type == EnemyType.Burst)
            {
                for (int i = 0; i < 3; i++)
                {
                    Attack();
                    yield return new WaitForSeconds(0.15f);
                }
            }
            else
            {
                Attack();
            }

            float delay = (float)(random.NextDouble() * (maxAttackDelay - minAttackDelay) + minAttackDelay);
            yield return new WaitForSeconds(delay);
        }

        isAttacking = false;
    }

    private void Attack()
    {
        if (health.IsDead || projectilePrefab == null || player == null) return;

        source?.PlayOneShot(fireSound);

        Vector3 shootDir = (player.position - transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Fireball projectile = proj.GetComponent<Fireball>();
        if (projectile != null)
        {
            float baseSpeed = 10f;
            float speedMult = 1f;
            float scaleMult = 1f;
            float damage = 0.5f;

            switch (type)
            {
                case EnemyType.Fast:
                    speedMult = 1.5f;
                    scaleMult = 0.5f;
                    break;
                case EnemyType.Heavy:
                    speedMult = 0.75f;
                    scaleMult = 2f;
                    damage = 1f;
                    break;
            }

            proj.transform.localScale *= scaleMult;
            projectile.Setup(shootDir, baseSpeed * speedMult, damage);
        }
    }

    private void PlayRandomSound()
    {
        if (detectedSounds == null || detectedSounds.Length == 0 || source == null) return;
        source.Stop();

        int index = random.Next(detectedSounds.Length);
        AudioClip clip = detectedSounds[index];

        source.pitch = (float)(random.NextDouble() * 0.3 + 0.9);
        source.clip = clip;
        source.Play();
    }

    private bool CanHitPlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 direction = (player.position - origin).normalized;
        float distance = Vector3.Distance(origin, player.position);

        RaycastHit hit;
        if (Physics.SphereCast(origin, 0.5f, direction, out hit, distance))
        {
            if (hit.transform.CompareTag("Player"))
            {
                Debug.DrawRay(origin, direction * distance, Color.green, 0.2f);
                return true;
            }

            Debug.DrawLine(origin, hit.point, Color.red, 0.2f);
        }

        return false;
    }

    public Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = SeededInsideUnitSphere() * dist;
        randDirection += origin;

        if (NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, dist, layermask))
        {
            return navHit.position;
        }

        return origin;
    }


    private Vector3 SeededInsideUnitSphere()
    {
        // Generate a random point in a unit sphere using System.Random
        float x = (float)(random.NextDouble() * 2 - 1);
        float y = (float)(random.NextDouble() * 2 - 1);
        float z = (float)(random.NextDouble() * 2 - 1);
        Vector3 point = new Vector3(x, y, z);

        // If the point is outside the unit sphere, resample (rejection sampling)
        while (point.sqrMagnitude > 1f)
        {
            x = (float)(random.NextDouble() * 2 - 1);
            y = (float)(random.NextDouble() * 2 - 1);
            z = (float)(random.NextDouble() * 2 - 1);
            point = new Vector3(x, y, z);
        }

        return point;
    }
}
