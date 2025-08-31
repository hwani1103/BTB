using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("ī�޶� �̵� ����")]
    public float moveSpeed = 10f;
    public float edgeScrollWidth = 20f;
    public float heroFollowSpeed = 3f;
    public float heroFollowStrength = 0.7f;    // ���� ���� ���� (0-1)

    [Header("ī�޶� �̵� ����")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minY = -30f;
    public float maxY = 30f;

    private Camera cam;
    private bool isTrackingHero = false;       // ������ ���� ������
    private Transform heroTarget = null;       // ������ ����
    private Vector3 manualOffset = Vector3.zero; // ���� �̵����� ���� ������

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        Vector3 mouseMovement = HandleMouseMovement();

        if (isTrackingHero && heroTarget != null)
        {
            // ���� ���� + ���콺 �̵� ȥ��
            CombinedCameraMovement(mouseMovement);
        }
        else
        {
            // ���� ���콺 �̵���
            ApplyMouseMovement(mouseMovement);
        }
    }

    Vector3 HandleMouseMovement()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 moveDirection = Vector3.zero;

        // ȭ�� �����ڸ� ����
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
        // ���� ��ġ ���
        Vector3 heroPosition = heroTarget.position;
        heroPosition.z = transform.position.z;

        // ���콺 �̵��� ������ ������ ����
        if (mouseMovement.magnitude > 0.01f)
        {
            manualOffset += mouseMovement;
            Debug.Log($"���� ������ ����: {manualOffset}");
        }

        // ��ǥ ��ġ = ���� ��ġ + ���� ������
        Vector3 targetPosition = heroPosition + manualOffset;

        // ���� ���� (�ε巴��)
        Vector3 heroTrackingPosition = Vector3.Lerp(transform.position, targetPosition,
                                                   heroFollowSpeed * Time.deltaTime);

        // ���� ��ġ ����
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

    // GameManager�� ȣ���� �޼����
    public void SetHeroTarget(Transform target)
    {
        heroTarget = target;
    }

    public void EnableHeroTracking(bool enable)
    {
        isTrackingHero = enable;

        if (enable)
        {
            // ���� ���� ���� �� ������ ����
            manualOffset = Vector3.zero;
            Debug.Log("���� ���� ��� Ȱ��ȭ");
        }
        else
        {
            // ���� ���� �� ���� ��ġ ����
            Debug.Log("���� ���� ��� ��Ȱ��ȭ");
        }
    }

    public void FocusOnHero()
    {
        if (heroTarget != null)
        {
            // ��� �̵����� ���� �ε巯�� �̵� ����
            manualOffset = Vector3.zero; // ������ ����

            // ���� ������ ��Ȱ��ȭ�Ǿ� �ִٸ� Ȱ��ȭ
            if (!isTrackingHero)
            {
                EnableHeroTracking(true);
            }

            Debug.Log("ī�޶� �������� �ε巴�� ��Ŀ���˴ϴ�.");
        }
    }
}