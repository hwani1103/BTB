using UnityEngine;

public class HeroController : MonoBehaviour
{
    [Header("�̵� ����")]
    public float moveSpeed = 5f;

    [Header("���� ����")]
    public int health = 200;
    public int maxHealth = 200;
    public int attackDamage = 50;
    public int defense = 10;
    public float attackCooldown = 1.0f;

    [Header("UI ����")]
    public GameObject healthBarPrefab;

    [Header("���� ����")]
    public float combatRange = 1.0f;
    public float idleTimeForAutoAttack = 0.5f;

    [Header("���� ����")]
    public GameObject selectionIndicator; // ���õǾ����� ǥ�õ� UI Object�� ���� ���� (����� Circle)
    public Color selectedColor = Color.yellow; // ���ý� ���� ��������Ʈ ����.

    private Rigidbody2D rb;
    private Vector2 movement; // ���� �Էµ� ���� ǥ�ÿ�
    private bool isInCombat = false; // ���� ���������� ����
    private float lastAttackTime = 0f; // ������ ���� �ð� (��ٿ� ����)
    private GameObject healthBarInstance; // ������ ü�¹� �ν��Ͻ�.
    private HealthBar healthBarScript; // ü�¹� ��ũ��Ʈ ������ ���� ��������
    private bool isSelected = false; // GameManager�� ���� ������ ���õ� ����
    private SpriteRenderer spriteRenderer; // ���� ���濡 �ʿ��� Hero Sprite Renderer
    private Color originalColor; // �������� ���� (���þȵǾ������� ����)

    // �÷��̾� �Է� ����
    private bool hasPlayerInput = false; // ���� �÷��̾ ����������. (�����߿��� wasd�� �̵��� ����ϸ� �̵� �켱�ϱ�����)
    private float lastMovementTime = 0f; // ������ �̵��ð�
    private EnemyUnit currentTarget = null; // ���� �������� �� (�������� ���� ��� �����ϱ� ����)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        // Rigidbody2D, ������� �̵������� �ݵ�� �ʿ��� ������Ʈ. Inspector���� �ٿ������� ��������
        // ������ �ڵ����� �߰��ϱ�. �߷��� 0, ȸ���� X.

        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning("Hero�� Collider2D�� �����ϴ�!");
        }
        // Collider2D ã�Ƽ� null�̸� ��� �α� ���

        // ��������Ʈ ������ ����
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        // SpriteRenderer�� �����´���, originalColor �� ���� �ȵǾ�����, ������ ó�� �����Ǵ� �⺻���� �Ҵ�!
        CreateHealthBar(); // HealthBar �����. �ش� �޼��� ��ġ�� �ּ�
    }

    void Update()
    {
        HandleInput();

        // �÷��̾� �Է��� ���� ���� �ð��� ������ �ڵ� ���� (���� ���� �������)
        if (!hasPlayerInput && Time.time - lastMovementTime > idleTimeForAutoAttack)
        {
            CheckAndAttackNearbyEnemies(); // �ֺ��� ������ üũ�ϰ� �����ϴ¸޼���
        }
        else if (hasPlayerInput) // �÷��̾� �Է��� ������,
        {
            currentTarget = null; // Ÿ���ϴ� ���� null�� �ϰ�, �������� false
            isInCombat = false;
        }
    }

    void CheckAndAttackNearbyEnemies()
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, combatRange);
        // �ֺ� ���� ã�Ƽ� �迭�� ��� . �̶� ������ ������������� ���� ù��°�� �߰ߵ� ���� ������
        foreach (var enemy in nearbyEnemies)
        { // 
            if (enemy.CompareTag("Enemy")) // Enemy���� �±�Ȯ���ϰ�
            {
                EnemyUnit enemyScript = enemy.GetComponent<EnemyUnit>(); // EnemyUnit ��ũ��Ʈ ������ ���
                if (enemyScript != null)
                {
                    currentTarget = enemyScript; // ����Ÿ������ ���� (enemyScript�� Ÿ���������� �޼���ȣ�����ؼ�)
                    isInCombat = true; // ������ True
                    AttackTarget(enemyScript); // AttackTarget�� ���� Ÿ�� enemyscript���̺�����.
                    break;
                }
            }
        }

        if (currentTarget == null)
        {
            isInCombat = false;
        }
    }

    void FixedUpdate()
    {   // ���� ������ WASD�Է¿� ���� �̵��� ���� ����� (���� ����Ҽ� �����ϱ�)
        // �׷��� FixedUpdate()���� ȣ��.
        if (hasPlayerInput)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
            // MovePosition�� �浹 �����ϸ鼭 �̵�(���� ������ ����)
        }
        else if (!isInCombat) // �Է��� ���� �������� �ƴҰ��
        {
            rb.linearVelocity = Vector2.zero; // �ٷ� ����.
        }
    }

    void HandleInput()
    {
        // ���õ��� �ʾ����� �Է��� ���� ����
        if (!isSelected) // Hero�� ���õ��� �ʾҴٸ�
        {
            movement = Vector2.zero; // ���� ����
            hasPlayerInput = false; // ��ǲ false
            return; // HandleInput() ����
        }

        float horizontal = 0f; // ���õǾ��ٸ� horizontal = 0, vertical = 0.
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W))
            vertical = 1f;
        else if (Input.GetKey(KeyCode.S))
            vertical = -1f;

        if (Input.GetKey(KeyCode.A))
            horizontal = -1f;
        else if (Input.GetKey(KeyCode.D))
            horizontal = 1f; // wasd�̵� ����

        movement = new Vector2(horizontal, vertical).normalized; // .normalized��, �밢�̵��� �����ȯ�� ����ũ�� 1������

        if (movement.magnitude > 0) // .magnitude�� ������ ũ����. �� ���ǹ��� �ǹ̴� "�÷��̾ �̵� �Է��� �ߴ���?"
        {                           // �̰��� .normalized�Ŀ��� 0 �ƴϸ� 1��. ��ǻ� �Է��� �־��°�? üũ
            hasPlayerInput = true;
            lastMovementTime = Time.time;
        }
        else
        {
            hasPlayerInput = false;
        }
    }

    public void TakeDamage(int damage) // Hero�� �������� �޴� �޼���
    {
        int actualDamage = Mathf.Max(1, damage - defense); // ���� ������ 30, ���� 10�̸� Max(1, 20)
                                                           // �̰�� 20��ȯ. 1�� �ּڰ�. ������ ���Ƶ� �ּ� 1��ŭ ����
        health -= actualDamage; // ü�¿��� ���� ���� ���� ����
        health = Mathf.Max(0, health); // ü���� 0 �̸��� �Ǵ� ���� ���� �ڵ�

        UpdateHealthBar(); // Slider �ֽ�ȭ

        if (health <= 0) // ü���� 0���ϸ�
        {
            Die(); // Die..
        }
    }

    void AttackTarget(EnemyUnit target) // EnemyScript�� �Ű������� �޾Ƽ�
    {
        if (Time.time - lastAttackTime >= attackCooldown) // ����ð� - ���������ݽð��� >= attackCooldown�̸�
        {                                                   // �� �ʹ����������ϴ°� �����ڵ�
            target.TakeDamage(attackDamage); // Script�� takeDamageȣ��. Enemy���� ������ ���ϴ� �ڵ�
            lastAttackTime = Time.time; // ���������ݽð��� ����ð����� ����. ��ٿ� ����
        }
    }

    public void Heal(int healAmount)
    {
        health = Mathf.Min(health + healAmount, maxHealth);
        UpdateHealthBar();
    }

    void CreateHealthBar()
    {
        if (healthBarPrefab != null) // healthBarPrefab�� Inspector���� �ٿ������� null�� �ƴ�
        {
            Canvas canvas = FindAnyObjectByType<Canvas>(); // Canvas�� ã��.
            if (canvas == null)
            {
                Debug.LogError("Canvas�� ã�� �� �����ϴ�!");
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

    void UpdateHealthBar() // ������ ���ݹ޾Ұų� heal���޾Ƽ� ü���� ��ȭ�� ��������� ȣ��
    {
        if (healthBarScript != null) // ���� healthBarScript�� üũ�ϰ�
        {
            healthBarScript.SetHealth(health, maxHealth); // ���� health, maxHealth�� �ְ� ȣ�������ν�
                                                          // Slider���� ����
        }
    }

    void Die()
    {
        gameObject.SetActive(false); // HeroObject SetActive false. �� ������Ʈ ��Ȱ��ȭ!
    }

    // ���� ���� ����
    public void SetSelected(bool selected) // GameManager���� ȣ��. bool selected�� ���� ���� ���� / ������ bool�� ����
    {
        isSelected = selected; // �� bool������ isSelected�� �Ҵ�

        if (selectionIndicator != null) 
        {
            selectionIndicator.SetActive(selected); // ���� Indicator Ȱ��ȭ / ��Ȱ��ȭ. 
        }

        // ��������Ʈ ���� ����. ���⵵ ���������� Ȱ��/��Ȱ�������� ���� ����
        if (spriteRenderer != null)
        {
            if (selected)
            {
                spriteRenderer.color = selectedColor;
            }
            else
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, combatRange);
    }
}