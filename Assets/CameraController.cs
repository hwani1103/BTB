using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("카메라 이동 설정")]
    public float moveSpeed = 10f;
    public float edgeScrollWidth = 20f;
    public float heroFollowSpeed = 3f;
    public float heroFollowStrength = 0.7f;    // 영웅 추적 강도 (0-1)

    [Header("카메라 이동 제한")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minY = -30f;
    public float maxY = 30f;

    private Camera cam;
    private bool isTrackingHero = false;       // 영웅을 추적 중인지
    private Transform heroTarget = null;       // 추적할 영웅
    private Vector3 manualOffset = Vector3.zero; // 수동 이동으로 생긴 오프셋

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        Vector3 mouseMovement = HandleMouseMovement();

        if (isTrackingHero && heroTarget != null)
        {
            // 영웅 추적 + 마우스 이동 혼합
            CombinedCameraMovement(mouseMovement);
        }
        else
        {
            // 순수 마우스 이동만
            ApplyMouseMovement(mouseMovement);
        }
    }

    Vector3 HandleMouseMovement()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 moveDirection = Vector3.zero;

        // 화면 가장자리 감지
        if (mousePosition.x <= edgeScrollWidth)
        {
            moveDirection.x = -1f;
        }
        else if (mousePosition.x >= Screen.width - edgeScrollWidth)
        {
            moveDirection.x = 1f;
        }

        if (mousePosition.y <= edgeScrollWidth)
        {
            moveDirection.y = -1f;
        }
        else if (mousePosition.y >= Screen.height - edgeScrollWidth)
        {
            moveDirection.y = 1f;
        }

        return moveDirection * moveSpeed * Time.deltaTime;
    }

    void CombinedCameraMovement(Vector3 mouseMovement)
    {
        // 영웅 위치 계산
        Vector3 heroPosition = heroTarget.position;
        heroPosition.z = transform.position.z;

        // 마우스 이동이 있으면 오프셋 누적
        if (mouseMovement.magnitude > 0.01f)
        {
            manualOffset += mouseMovement;
            Debug.Log($"수동 오프셋 증가: {manualOffset}");
        }

        // 목표 위치 = 영웅 위치 + 수동 오프셋
        Vector3 targetPosition = heroPosition + manualOffset;

        // 영웅 추적 (부드럽게)
        Vector3 heroTrackingPosition = Vector3.Lerp(transform.position, targetPosition,
                                                   heroFollowSpeed * Time.deltaTime);

        // 최종 위치 적용
        Vector3 finalPosition = heroTrackingPosition;
        finalPosition.x = Mathf.Clamp(finalPosition.x, minX, maxX);
        finalPosition.y = Mathf.Clamp(finalPosition.y, minY, maxY);

        transform.position = finalPosition;
    }

    void ApplyMouseMovement(Vector3 mouseMovement)
    {
        if (mouseMovement.magnitude > 0.01f)
        {
            Vector3 newPosition = transform.position + mouseMovement;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
            newPosition.z = transform.position.z;

            transform.position = newPosition;
        }
    }

    // GameManager가 호출할 메서드들
    public void SetHeroTarget(Transform target)
    {
        heroTarget = target;
    }

    public void EnableHeroTracking(bool enable)
    {
        isTrackingHero = enable;

        if (enable)
        {
            // 영웅 추적 시작 시 오프셋 리셋
            manualOffset = Vector3.zero;
            Debug.Log("영웅 추적 모드 활성화");
        }
        else
        {
            // 추적 해제 시 현재 위치 유지
            Debug.Log("영웅 추적 모드 비활성화");
        }
    }

    public void FocusOnHero()
    {
        if (heroTarget != null)
        {
            // 즉시 이동하지 말고 부드러운 이동 시작
            manualOffset = Vector3.zero; // 오프셋 리셋

            // 영웅 추적이 비활성화되어 있다면 활성화
            if (!isTrackingHero)
            {
                EnableHeroTracking(true);
            }

            Debug.Log("카메라가 영웅에게 부드럽게 포커스됩니다.");
        }
    }
}