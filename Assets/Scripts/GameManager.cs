using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("���� ���� ����")]
    public GameObject heroSelectionIndicator;
    public float heroOffscreenThreshold = 0.3f; // ������ ȭ�鿡�� �̸�ŭ ����� (0-1, 0.5 = ȭ�� ����)
    public float deselectTime = 1.0f;           // �� �ð����� ��� �־�� ���� ����

    private HeroController selectedHero;
    private CameraController cameraController;
    private Camera cam;                        // ī�޶� ����
    private bool isHeroSelected = false;

    // ���� ������ ���� Ÿ�̸�
    private bool isHeroFarAway = false;
    private float heroAwayTimer = 0f;

    // �̱��� ����
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // ī�޶� ��Ʈ�ѷ� ã��
        cameraController = FindAnyObjectByType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CameraController�� ã�� �� �����ϴ�!");
        }

        // ���� ī�޶� ã��
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main Camera�� ã�� �� �����ϴ�!");
        }

        // ���� ã��
        GameObject heroObject = GameObject.FindWithTag("Player");
        if (heroObject != null)
        {
            selectedHero = heroObject.GetComponent<HeroController>();
        }

        if (selectedHero == null)
        {
            Debug.LogError("������ ã�� �� �����ϴ�!");
        }
    }

    void Update()
    {
        // ����Ű 1 �Է� üũ
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleHeroSelection();
        }

        // ������ ���õ� ���¿����� ���� ���� ���� üũ
        if (isHeroSelected)
        {
            CheckDeselectCondition();
        }
    }

    void CheckDeselectCondition()
    {
        bool heroCurrentlyOffscreen = IsHeroFarFromView();

        if (heroCurrentlyOffscreen)
        {
            if (!isHeroFarAway)
            {
                // ������ ��� �þ߿��� �־����� ����
                isHeroFarAway = true;
                heroAwayTimer = 0f;
                Debug.Log("������ ī�޶� �þ߿��� �־������ϴ�. Ÿ�̸� ����...");
            }
            else
            {
                // ������ ��� �ָ� ���� - Ÿ�̸� ����
                heroAwayTimer += Time.deltaTime;

                if (heroAwayTimer >= deselectTime)
                {
                    Debug.Log($"������ {deselectTime}�� ���� �þ߿��� �־��� ���� ����!");
                    DeselectHero();
                }
            }
        }
        else
        {
            // ������ �ٽ� �þ� ������ ���� - Ÿ�̸� ����
            if (isHeroFarAway)
            {
                isHeroFarAway = false;
                heroAwayTimer = 0f;
                Debug.Log("������ �ٽ� �þ� ������ ����. Ÿ�̸� ����.");
            }
        }
    }

    bool IsHeroFarFromView()
    {
        if (selectedHero == null || cam == null) return false;

        // ������ ���� ��ǥ�� ����Ʈ ��ǥ�� ��ȯ (0-1 ����)
        Vector3 viewportPoint = cam.WorldToViewportPoint(selectedHero.transform.position);

        // ȭ�� ���� ���� ���� (0�� 1 ������ ������ ����)
        float safeMargin = 1.0f - heroOffscreenThreshold; // ��: threshold=0.1�̸� safeMargin=0.9
        float minSafe = (1.0f - safeMargin) / 2.0f;       // ��: 0.05 (ȭ���� 5% ����)
        float maxSafe = (1.0f + safeMargin) / 2.0f;       // ��: 0.95 (ȭ���� 95% ����)

        // ������ ���� ������ ������� üũ
        bool isOutsideSafeZone = viewportPoint.x < minSafe || viewportPoint.x > maxSafe ||
                                viewportPoint.y < minSafe || viewportPoint.y > maxSafe ||
                                viewportPoint.z <= 0; // z�� 0 ���ϸ� ī�޶� �ڿ� ����

        // ����� �α� (�׽�Ʈ��)
        if (Time.frameCount % 30 == 0) // 0.5�ʸ��� ���
        {
            Debug.Log($"���� ����Ʈ: ({viewportPoint.x:F2}, {viewportPoint.y:F2}), ��������: {minSafe:F2}~{maxSafe:F2}, ���: {isOutsideSafeZone}");
        }

        return isOutsideSafeZone;
    }

    void ToggleHeroSelection()
    {
        if (selectedHero == null) return;

        if (isHeroSelected)
        {
            // �̹� ���õ� ���¸� �ε巴�� �������� ��Ŀ��
            FocusCameraOnHero();

            // Ÿ�̸ӵ� �����ؼ� �ٷ� �������� �ʵ���
            isHeroFarAway = false;
            heroAwayTimer = 0f;
        }
        else
        {
            // ���� ����
            SelectHero();
        }
    }

    void SelectHero()
    {
        if (selectedHero == null) return;

        isHeroSelected = true;

        // Ÿ�̸� ����
        isHeroFarAway = false;
        heroAwayTimer = 0f;

        // ī�޶� ���� - ���� ���� Ȱ��ȭ�ϵ� ���콺 �̵��� ���
        if (cameraController != null)
        {
            cameraController.SetHeroTarget(selectedHero.transform);
            cameraController.EnableHeroTracking(true);
        }

        // �������� ���õǾ��ٰ� �˸�
        selectedHero.SetSelected(true);

        // ���� ǥ�� Ȱ��ȭ
        ShowSelectionIndicator(true);

        Debug.Log("������ ���õǾ����ϴ�!");
    }

    public void DeselectHero()
    {
        if (!isHeroSelected) return;

        isHeroSelected = false;

        // Ÿ�̸� ����
        isHeroFarAway = false;
        heroAwayTimer = 0f;

        // ī�޶� ���� ���� ��Ȱ��ȭ
        if (cameraController != null)
        {
            cameraController.EnableHeroTracking(false);
        }

        // �������� ���� �����Ǿ��ٰ� �˸�
        if (selectedHero != null)
        {
            selectedHero.SetSelected(false);
        }

        // ���� ǥ�� ��Ȱ��ȭ
        ShowSelectionIndicator(false);

        Debug.Log("���� ������ �����Ǿ����ϴ�!");
    }

    void FocusCameraOnHero()
    {
        // ī�޶� ���� ��ġ�� ��� �̵�
        if (selectedHero != null && cameraController != null)
        {
            cameraController.FocusOnHero();
        }
    }

    void ShowSelectionIndicator(bool show)
    {
        if (heroSelectionIndicator != null)
        {
            heroSelectionIndicator.SetActive(show);

            if (show && selectedHero != null)
            {
                heroSelectionIndicator.transform.position = selectedHero.transform.position;
                heroSelectionIndicator.transform.SetParent(selectedHero.transform);
            }
        }
    }

    // �ܺο��� ���� ���� ���� Ȯ��
    public bool IsHeroSelected()
    {
        return isHeroSelected;
    }

    public HeroController GetSelectedHero()
    {
        return isHeroSelected ? selectedHero : null;
    }
}