using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("영웅 선택 설정")]
    public GameObject heroSelectionIndicator;
    public float heroOffscreenThreshold = 0.3f; // 영웅이 화면에서 이만큼 벗어나면 (0-1, 0.5 = 화면 절반)
    public float deselectTime = 1.0f;           // 이 시간동안 벗어나 있어야 선택 해제

    private HeroController selectedHero;
    private CameraController cameraController;
    private Camera cam;                        // 카메라 참조
    private bool isHeroSelected = false;

    // 선택 해제를 위한 타이머
    private bool isHeroFarAway = false;
    private float heroAwayTimer = 0f;

    // 싱글톤 패턴
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
        // 카메라 컨트롤러 찾기
        cameraController = FindAnyObjectByType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CameraController를 찾을 수 없습니다!");
        }

        // 메인 카메라 찾기
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main Camera를 찾을 수 없습니다!");
        }

        // 영웅 찾기
        GameObject heroObject = GameObject.FindWithTag("Player");
        if (heroObject != null)
        {
            selectedHero = heroObject.GetComponent<HeroController>();
        }

        if (selectedHero == null)
        {
            Debug.LogError("영웅을 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // 숫자키 1 입력 체크
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleHeroSelection();
        }

        // 영웅이 선택된 상태에서만 선택 해제 조건 체크
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
                // 영웅이 방금 시야에서 멀어지기 시작
                isHeroFarAway = true;
                heroAwayTimer = 0f;
                Debug.Log("영웅이 카메라 시야에서 멀어졌습니다. 타이머 시작...");
            }
            else
            {
                // 영웅이 계속 멀리 있음 - 타이머 증가
                heroAwayTimer += Time.deltaTime;

                if (heroAwayTimer >= deselectTime)
                {
                    Debug.Log($"영웅이 {deselectTime}초 동안 시야에서 멀어져 선택 해제!");
                    DeselectHero();
                }
            }
        }
        else
        {
            // 영웅이 다시 시야 안으로 들어옴 - 타이머 리셋
            if (isHeroFarAway)
            {
                isHeroFarAway = false;
                heroAwayTimer = 0f;
                Debug.Log("영웅이 다시 시야 안으로 들어옴. 타이머 리셋.");
            }
        }
    }

    bool IsHeroFarFromView()
    {
        if (selectedHero == null || cam == null) return false;

        // 영웅의 월드 좌표를 뷰포트 좌표로 변환 (0-1 범위)
        Vector3 viewportPoint = cam.WorldToViewportPoint(selectedHero.transform.position);

        // 화면 안전 영역 정의 (0과 1 사이의 안전한 범위)
        float safeMargin = 1.0f - heroOffscreenThreshold; // 예: threshold=0.1이면 safeMargin=0.9
        float minSafe = (1.0f - safeMargin) / 2.0f;       // 예: 0.05 (화면의 5% 지점)
        float maxSafe = (1.0f + safeMargin) / 2.0f;       // 예: 0.95 (화면의 95% 지점)

        // 영웅이 안전 영역을 벗어났는지 체크
        bool isOutsideSafeZone = viewportPoint.x < minSafe || viewportPoint.x > maxSafe ||
                                viewportPoint.y < minSafe || viewportPoint.y > maxSafe ||
                                viewportPoint.z <= 0; // z가 0 이하면 카메라 뒤에 있음

        // 디버그 로그 (테스트용)
        if (Time.frameCount % 30 == 0) // 0.5초마다 출력
        {
            Debug.Log($"영웅 뷰포트: ({viewportPoint.x:F2}, {viewportPoint.y:F2}), 안전영역: {minSafe:F2}~{maxSafe:F2}, 벗어남: {isOutsideSafeZone}");
        }

        return isOutsideSafeZone;
    }

    void ToggleHeroSelection()
    {
        if (selectedHero == null) return;

        if (isHeroSelected)
        {
            // 이미 선택된 상태면 부드럽게 영웅으로 포커스
            FocusCameraOnHero();

            // 타이머도 리셋해서 바로 해제되지 않도록
            isHeroFarAway = false;
            heroAwayTimer = 0f;
        }
        else
        {
            // 영웅 선택
            SelectHero();
        }
    }

    void SelectHero()
    {
        if (selectedHero == null) return;

        isHeroSelected = true;

        // 타이머 리셋
        isHeroFarAway = false;
        heroAwayTimer = 0f;

        // 카메라 설정 - 영웅 추적 활성화하되 마우스 이동도 허용
        if (cameraController != null)
        {
            cameraController.SetHeroTarget(selectedHero.transform);
            cameraController.EnableHeroTracking(true);
        }

        // 영웅에게 선택되었다고 알림
        selectedHero.SetSelected(true);

        // 선택 표시 활성화
        ShowSelectionIndicator(true);

        Debug.Log("영웅이 선택되었습니다!");
    }

    public void DeselectHero()
    {
        if (!isHeroSelected) return;

        isHeroSelected = false;

        // 타이머 리셋
        isHeroFarAway = false;
        heroAwayTimer = 0f;

        // 카메라 영웅 추적 비활성화
        if (cameraController != null)
        {
            cameraController.EnableHeroTracking(false);
        }

        // 영웅에게 선택 해제되었다고 알림
        if (selectedHero != null)
        {
            selectedHero.SetSelected(false);
        }

        // 선택 표시 비활성화
        ShowSelectionIndicator(false);

        Debug.Log("영웅 선택이 해제되었습니다!");
    }

    void FocusCameraOnHero()
    {
        // 카메라를 영웅 위치로 즉시 이동
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

    // 외부에서 영웅 선택 상태 확인
    public bool IsHeroSelected()
    {
        return isHeroSelected;
    }

    public HeroController GetSelectedHero()
    {
        return isHeroSelected ? selectedHero : null;
    }
}