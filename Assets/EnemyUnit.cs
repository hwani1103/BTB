using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 2.5f;

    [Header("전투 스탯")]
    public int health = 100;
    public int maxHealth = 100;
    public int attackDamage = 30;
    public int defense = 5;
    public float attackCooldown = 1.5f;

    [Header("UI 설정")]
    public GameObject healthBarPrefab;

    [Header("전투 설정")]
    public float combatRange = 1.0f;
    public float detectionRange = 5.0f;

    private Vector3[] waypoints;
    private int currentWaypointIndex = 1;
    private bool isMoving = true;
    private bool isInCombat = false;
    private bool isChasing = false;
    private Rigidbody2D rb;
    private GameObject healthBarInstance;
    private HealthBar healthBarScript;
    private float lastAttackTime = 0f;
    private GameObject heroTarget = null;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.25f;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("Enemy waypoints가 설정되지 않았습니다!");
            DestroyEnemy();
            return;
        }

        transform.position = waypoints[0];
        CreateHealthBar();
    }

    void Update()
    {
        if (!isMoving || waypoints == null) return;

        DetectAndUpdateState();
    }

    void DetectAndUpdateState()
    {
        GameObject hero = GameObject.FindWithTag("Player");

        if (hero != null)
        {
            float distanceToHero = Vector3.Distance(transform.position, hero.transform.position);

            if (distanceToHero <= combatRange)
            {
                // 전투 모드
                isInCombat = true;
                isChasing = false;
                heroTarget = hero;
                rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;

                HeroController heroScript = hero.GetComponent<HeroController>();
                if (heroScript != null)
                {
                    AttackTarget(heroScript);
                }
            }
            else if (!isChasing && distanceToHero <= detectionRange)
            {
                // 추격 시작
                isChasing = true;
                isInCombat = false;
                heroTarget = hero;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            else if (isChasing && distanceToHero > detectionRange * 1.4f) // 추격 포기
            {
                isChasing = false;
                isInCombat = false;
                heroTarget = null;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            else if (isChasing)
            {
                // 추격 유지
                isInCombat = false;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            else
            {
                // 평상시
                isChasing = false;
                isInCombat = false;
                heroTarget = null;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
        else
        {
            // Hero가 없으면 평상시 모드
            isChasing = false;
            isInCombat = false;
            heroTarget = null;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void FixedUpdate()
    {
        if (isMoving && !isInCombat && waypoints != null)
        {
            if (isChasing && heroTarget != null)
            {
                ChaseHero();
            }
            else
            {
                MoveToNextWaypoint();
            }
        }

        transform.rotation = Quaternion.identity;
    }

    void ChaseHero()
    {
        if (heroTarget == null) return;

        Vector3 direction = (heroTarget.transform.position - transform.position).normalized;
        rb.MovePosition(rb.position + (Vector2)direction * chaseSpeed * Time.fixedDeltaTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isInCombat = true;
            isChasing = false;
            HeroController hero = collision.gameObject.GetComponent<HeroController>();
            if (hero != null)
            {
                AttackTarget(hero);
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HeroController hero = collision.gameObject.GetComponent<HeroController>();
            if (hero != null)
            {
                AttackTarget(hero);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isInCombat = false;
        }
    }

    void AttackTarget(HeroController target)
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            target.TakeDamage(attackDamage);
            lastAttackTime = Time.time;
        }
    }

    void MoveToNextWaypoint()
    {
        if (currentWaypointIndex >= waypoints.Length)
        {
            ReachedFinalTarget();
            return;
        }

        Vector3 targetPoint = waypoints[currentWaypointIndex];
        Vector3 direction = (targetPoint - transform.position).normalized;

        rb.MovePosition(rb.position + (Vector2)direction * moveSpeed * Time.fixedDeltaTime);

        float distance = Vector3.Distance(transform.position, targetPoint);
        if (distance < 0.3f)
        {
            currentWaypointIndex++;
        }
    }

    void ReachedFinalTarget()
    {
        DestroyEnemy();
    }

    void CreateHealthBar()
    {
        if (healthBarPrefab != null)
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas를 찾을 수 없습니다!");
                return;
            }

            healthBarInstance = Instantiate(healthBarPrefab, canvas.transform);
            healthBarScript = healthBarInstance.GetComponent<HealthBar>();

            if (healthBarScript != null)
            {
                healthBarScript.Initialize(transform);
                healthBarScript.SetHealth(health, maxHealth);
            }
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarScript != null)
        {
            healthBarScript.SetHealth(health, maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(1, damage - defense);
        health -= actualDamage;
        health = Mathf.Max(0, health);

        UpdateHealthBar();

        if (health <= 0)
        {
            DestroyEnemy();
        }
    }

    void DestroyEnemy()
    {
        if (healthBarInstance != null)
        {
            healthBarInstance.SetActive(false);
            Destroy(healthBarInstance);
        }

        Destroy(gameObject);
    }

    public void Initialize(Vector3[] pathWaypoints)
    {
        waypoints = pathWaypoints;
        currentWaypointIndex = 1;

        if (waypoints != null && waypoints.Length > 0)
        {
            transform.position = waypoints[0];
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, combatRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}