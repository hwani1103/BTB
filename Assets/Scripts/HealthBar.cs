using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI ������Ʈ")]
    public Slider healthSlider; // ü�½����̴�. ����Ƽ �����Ϳ��� ���������� ������� �Ҵ��ص���.
    public Transform targetUnit; // ����ٴ� ����. �� ü�¹� �ؿ��ִ� ���� ������Ʈ. initialize()���� ������

    [Header("��ġ ����")]
    public Vector3 offset = new Vector3(0, 1.4f, 0); // ü�¹ٰ� ���� �Ӹ� �� ��������� ������ ���ϱ����� Offset.
                                                    // �ν����Ϳ����� ��������. 
    [Header("ü�¹� ����")]
    public Color lowHealthColor = Color.red;
    public Color fullHealthColor = Color.green; // ü�¿����� ����ȭ����. Green���� Red�� �׶��̼�. ���� �ڵ� ����

    private Camera mainCamera; // ���� ��ǥ�� ��ũ�� ��ǥ�� ��ȯ�ϱ����� �ʿ�
    // ���� ��ǥ : ���Ӽ��迡���� ��ġ. ������Ʈ�� 5,3,0�������� �̰� ���� ��ǥ. ������ǥ�� ���ʾƷ��� 0,0
    // ��ũ�� ��ǥ : ����� ȭ�鿡���� �ȼ� ��ġ. ȭ�� �������� 0,0 ���������� 1920, 1080���� �ȼ���ǥ
    // ��ȯ�� �ʿ��� ���� : ü�¹ٴ� UI�̹Ƿ� ��ũ�� ��ǥ��

    private Image fillImage; // �����̴� ������ ���� ������ ĥ������ Image ������Ʈ
    private float targetHealth = 1f; // �����̴� �ʱ���� 100%�� ����.

    void Start()
    {
        mainCamera = Camera.main; // ���� ���� ī�޶� ã�Ƽ� ����. Camera.Main�ڵ�� MainCamera �±װ� ���� ī�޶� ã��.

        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        if (healthSlider != null && healthSlider.fillRect != null)
        {
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        }
    }

    void Update()
    {
        if (targetUnit != null) // Ÿ���� ������ �����Ǿ� ������
        { 
            FollowTarget(); // �� ������ ����ٴϴ� FollowTarget() �޼��� ȣ��
        }

        SmoothHealthUpdate();
    }

    void FollowTarget()
    {
        Vector3 worldPosition = targetUnit.position + offset; // worldPosition������ Ÿ�������� ��ġ�� + offset����
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition); // �� worldPosition�� MainCamera�� �޼����
                                                                               // ��ũ����ǥ��� ��ȯ
        transform.position = screenPosition; // �� ��ȯ�� ��ũ�� ��ǥ�踦 ���� ��ġ�� ����. �� ��� ��ȯ�ϸ� ����ٴ�
    }

    void SmoothHealthUpdate()
    {   // �޼��� �̸���� �ε巯�� Sliderũ��/���� ���� ������Ʈ �޼���.
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealth, Time.deltaTime * 5f);
            // Slider�� Guage�� �ε巴�� ����. ����value, ��ǥvalue, ��ȭ�ӵ� ��� .
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthSlider.value);
            }   // fillImage�� color�� �ε巴�� ����. lowcolor�� fullcolor�� �ְ�, value������ �� ���̰����� RGB����ǥ��
        }
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0) return; // ������ 0���ϴ� ���� ����

        targetHealth = currentHealth / maxHealth; // takeDamage, Heal���� �޼��忡�� ȣ���ϴ°�. Slider�������
        targetHealth = Mathf.Clamp01(targetHealth); // ��������. Clamp01�� �Ű����� 0~1������ �� ��ȯ ���� 1->1 10->1 0.5->0.5 -10->0
        
    }

    public void Initialize(Transform unit) // EnemyUnit, HeroController���� ȣ��. �ű⼭ ȣ���Ҷ�
    {                                      // CreateHealthBar()�޼��忡�� �ڱ��ڽ��� transform�� �־��ָ鼭 ȣ����.
        targetUnit = unit;              // �� HealthBar�� ����ٴ� ������Ʈ�� transform�� �����鼭 initialize��.
    }
}