using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public Slider healthSlider; // 체력슬라이더. 유니티 에디터에서 프리팹으로 만든다음 할당해뒀음.
    public Transform targetUnit; // 따라다닐 유닛. 즉 체력바 밑에있는 실제 오브젝트. initialize()에서 설정됨

    [Header("위치 설정")]
    public Vector3 offset = new Vector3(0, 1.4f, 0); // 체력바가 유닛 머리 위 어느정도에 있을지 정하기위한 Offset.
                                                    // 인스펙터에서도 수정가능. 
    [Header("체력바 설정")]
    public Color lowHealthColor = Color.red;
    public Color fullHealthColor = Color.green; // 체력에따른 색상변화설정. Green에서 Red로 그라데이션. 색상 자동 보정

    private Camera mainCamera; // 월드 좌표를 스크린 좌표로 변환하기위해 필요
    // 월드 좌표 : 게임세계에서의 위치. 오브젝트가 5,3,0에있으면 이게 월드 좌표. 월드좌표는 왼쪽아래가 0,0
    // 스크린 좌표 : 모니터 화면에서의 픽셀 위치. 화면 왼쪽위가 0,0 오른쪽위가 1920, 1080같은 픽셀좌표
    // 변환이 필요한 이유 : 체력바는 UI이므로 스크린 좌표계

    private Image fillImage; // 슬라이더 내부의 실제 색깔이 칠해지는 Image 컴포넌트
    private float targetHealth = 1f; // 슬라이더 초기상태 100%로 설정.

    void Start()
    {
        mainCamera = Camera.main; // 씬의 메인 카메라를 찾아서 참조. Camera.Main코드는 MainCamera 태그가 붙은 카메라를 찾음.

        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        if (healthSlider != null && healthSlider.fillRect != null)
        {
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        }
    }

    void Update()
    {
        if (targetUnit != null) // 타겟할 유닛이 설정되어 있으면
        { 
            FollowTarget(); // 그 유닛을 따라다니는 FollowTarget() 메서드 호출
        }

        SmoothHealthUpdate();
    }

    void FollowTarget()
    {
        Vector3 worldPosition = targetUnit.position + offset; // worldPosition변수에 타겟유닛의 위치에 + offset보정
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition); // 그 worldPosition을 MainCamera의 메서드로
                                                                               // 스크린좌표계로 전환
        transform.position = screenPosition; // 그 변환된 스크린 좌표계를 최종 위치로 결정. 즉 계속 변환하며 따라다님
    }

    void SmoothHealthUpdate()
    {   // 메서드 이름대로 부드러운 Slider크기/색상 보간 업데이트 메서드.
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealth, Time.deltaTime * 5f);
            // Slider의 Guage를 부드럽게 변경. 현재value, 목표value, 변화속도 배수 .
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthSlider.value);
            }   // fillImage의 color를 부드럽게 변경. lowcolor와 fullcolor가 있고, value에따라 그 사이값으로 RGB색상표현
        }
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0) return; // 나누기 0을하는 에러 방지

        targetHealth = currentHealth / maxHealth; // takeDamage, Heal등의 메서드에서 호출하는것. Slider비율계산
        targetHealth = Mathf.Clamp01(targetHealth); // 에러방지. Clamp01은 매개변수 0~1사이의 값 반환 보장 1->1 10->1 0.5->0.5 -10->0
        
    }

    public void Initialize(Transform unit) // EnemyUnit, HeroController에서 호출. 거기서 호출할때
    {                                      // CreateHealthBar()메서드에서 자기자신의 transform을 넣어주면서 호출함.
        targetUnit = unit;              // 이 HealthBar가 따라다닐 오브젝트의 transform을 받으면서 initialize됨.
    }
}