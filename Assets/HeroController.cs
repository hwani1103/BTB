using UnityEngine;

public class HeroController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;

    [Header("전투 스탯")]
    public int health = 200;
    public int maxHealth = 200;
    public int attackDamage = 50;
    public int defense = 10;
    public float attackCooldown = 1.0f;

    [Header("UI 설정")]
    public GameObject healthBarPrefab;

    [Header("전투 설정")]
    public float combatRange = 1.0f;
    public float idleTimeForAutoAttack = 0.5f;

    [Header("선택 상태")]
    public GameObject selectionIndicator; // 선택되었을때 표시될 UI Object를 담을 변수 (현재는 Circle)
    public Color selectedColor = Color.yellow; // 선택시 영웅 스프라이트 색상.

    private Rigidbody2D rb;
    private Vector2 movement; // 현재 입력된 방향 표시용
    private bool isInCombat = false; // 현재 전투중인지 여부
    private float lastAttackTime = 0f; // 마지막 공격 시간 (쿨다운 계산용)
    private GameObject healthBarInstance; // 생성된 체력바 인스턴스.
    private HealthBar healthBarScript; // 체력바 스크립트 참조를 위한 변수선언
    private bool isSelected = false; // GameManager에 의해 영웅이 선택된 상태
    private SpriteRenderer spriteRenderer; // 색상 변경에 필요한 Hero Sprite Renderer
    private Color originalColor; // 원래색상 저장 (선택안되었을때의 색상)

    // 플레이어 입력 추적
    private bool hasPlayerInput = false; // 현재 플레이어가 조작중인지. (전투중에도 wasd로 이동을 명령하면 이동 우선하기위해)
    private float lastMovementTime = 0f; // 마지막 이동시간
    private EnemyUnit currentTarget = null; // 현재 공격중인 적 (공격중인 적을 계속 공격하기 위함)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        // Rigidbody2D, 물리기반 이동을위해 반드시 필요한 컴포넌트. Inspector에서 붙여놨으면 가져오고
        // 없으면 자동으로 추가하기. 중력은 0, 회전은 X.

        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning("Hero에 Collider2D가 없습니다!");
        }
        // Collider2D 찾아서 null이면 경고 로그 출력

        // 스프라이트 렌더러 설정
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        // SpriteRenderer를 가져온다음, originalColor 즉 선택 안되었을때, 영웅이 처음 생성되는 기본색깔 할당!
        CreateHealthBar(); // HealthBar 만들기. 해당 메서드 위치에 주석
    }

    void Update()
    {
        HandleInput();

        // 플레이어 입력이 없고 일정 시간이 지나면 자동 전투 (선택 상태 관계없이)
        if (!hasPlayerInput && Time.time - lastMovementTime > idleTimeForAutoAttack)
        {
            CheckAndAttackNearbyEnemies(); // 주변의 적들을 체크하고 공격하는메서드
        }
        else if (hasPlayerInput) // 플레이어 입력이 있으면,
        {
            currentTarget = null; // 타겟하는 적을 null로 하고, 전투중을 false
            isInCombat = false;
        }
    }

    void CheckAndAttackNearbyEnemies()
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, combatRange);
        // 주변 적들 찾아서 배열에 담기 . 이때 순서는 보장되지않지만 보통 첫번째로 발견된 적을 공격함
        foreach (var enemy in nearbyEnemies)
        { // 
            if (enemy.CompareTag("Enemy")) // Enemy인지 태그확인하고
            {
                EnemyUnit enemyScript = enemy.GetComponent<EnemyUnit>(); // EnemyUnit 스크립트 참조를 얻고
                if (enemyScript != null)
                {
                    currentTarget = enemyScript; // 현재타겟으로 설정 (enemyScript가 타겟인이유는 메서드호출위해서)
                    isInCombat = true; // 전투중 True
                    AttackTarget(enemyScript); // AttackTarget에 현재 타겟 enemyscript같이보내기.
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
    {   // 현재 유져의 WASD입력에 의한 이동은 물리 계산임 (벽은 통과할수 없으니깐)
        // 그래서 FixedUpdate()에서 호출.
        if (hasPlayerInput)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
            // MovePosition은 충돌 감지하면서 이동(벽에 막히면 멈춤)
        }
        else if (!isInCombat) // 입력이 없고 전투중이 아닐경우
        {
            rb.linearVelocity = Vector2.zero; // 바로 멈춤.
        }
    }

    void HandleInput()
    {
        // 선택되지 않았으면 입력을 받지 않음
        if (!isSelected) // Hero가 선택되지 않았다면
        {
            movement = Vector2.zero; // 방향 없음
            hasPlayerInput = false; // 인풋 false
            return; // HandleInput() 종료
        }

        float horizontal = 0f; // 선택되었다면 horizontal = 0, vertical = 0.
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W))
            vertical = 1f;
        else if (Input.GetKey(KeyCode.S))
            vertical = -1f;

        if (Input.GetKey(KeyCode.A))
            horizontal = -1f;
        else if (Input.GetKey(KeyCode.D))
            horizontal = 1f; // wasd이동 로직

        movement = new Vector2(horizontal, vertical).normalized; // .normalized는, 대각이동시 방향반환및 벡터크기 1로조정

        if (movement.magnitude > 0) // .magnitude는 벡터의 크기임. 이 조건문의 의미는 "플레이어가 이동 입력을 했는지?"
        {                           // 이값은 .normalized후에는 0 아니면 1임. 사실상 입력이 있었는가? 체크
            hasPlayerInput = true;
            lastMovementTime = Time.time;
        }
        else
        {
            hasPlayerInput = false;
        }
    }

    public void TakeDamage(int damage) // Hero가 데미지를 받는 메서드
    {
        int actualDamage = Mathf.Max(1, damage - defense); // 실제 데미지 30, 방어력 10이면 Max(1, 20)
                                                           // 이경우 20반환. 1은 최솟값. 방어력이 높아도 최소 1만큼 공격
        health -= actualDamage; // 체력에서 실제 입은 피해 누적
        health = Mathf.Max(0, health); // 체력이 0 미만이 되는 오류 방지 코드

        UpdateHealthBar(); // Slider 최신화

        if (health <= 0) // 체력이 0이하면
        {
            Die(); // Die..
        }
    }

    void AttackTarget(EnemyUnit target) // EnemyScript를 매개변수로 받아서
    {
        if (Time.time - lastAttackTime >= attackCooldown) // 현재시간 - 마지막공격시간이 >= attackCooldown이면
        {                                                   // 즉 너무빨리공격하는걸 막는코드
            target.TakeDamage(attackDamage); // Script의 takeDamage호출. Enemy에게 공격을 가하는 코드
            lastAttackTime = Time.time; // 마지막공격시간을 현재시간으로 설정. 쿨다운 계산용
        }
    }

    public void Heal(int healAmount)
    {
        health = Mathf.Min(health + healAmount, maxHealth);
        UpdateHealthBar();
    }

    void CreateHealthBar()
    {
        if (healthBarPrefab != null) // healthBarPrefab은 Inspector에서 붙여뒀으니 null이 아님
        {
            Canvas canvas = FindAnyObjectByType<Canvas>(); // Canvas를 찾음.
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

    void UpdateHealthBar() // 영웅이 공격받았거나 heal을받아서 체력의 변화가 생겼을경우 호출
    {
        if (healthBarScript != null) // 먼저 healthBarScript를 체크하고
        {
            healthBarScript.SetHealth(health, maxHealth); // 현재 health, maxHealth를 넣고 호출함으로써
                                                          // Slider비율 조정
        }
    }

    void Die()
    {
        gameObject.SetActive(false); // HeroObject SetActive false. 즉 오브젝트 비활성화!
    }

    // 영웅 선택 관련
    public void SetSelected(bool selected) // GameManager에서 호출. bool selected는 실제 영웅 선택 / 해제시 bool값 넣음
    {
        isSelected = selected; // 그 bool값으로 isSelected에 할당

        if (selectionIndicator != null) 
        {
            selectionIndicator.SetActive(selected); // 영웅 Indicator 활성화 / 비활성화. 
        }

        // 스프라이트 색깔 변경. 여기도 마찬가지로 활성/비활성에따른 색상 변경
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