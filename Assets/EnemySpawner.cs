using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject enemyPrefab;          // 생성할 적 프리팹
    public float spawnInterval = 3f;        // 생성 간격 (초)
    public Transform spawnPoint;            // 생성 위치

    [Header("체크포인트 경로")]
    public Transform[] waypointTransforms;  // 적들이 따라갈 경로 (Transform 배열)

    [Header("게임 상태")]
    public bool isSpawning = true;          // 스폰 활성화 여부
    public int enemiesSpawned = 0;          // 생성된 적 수 (통계용)

    private float nextSpawnTime;
    private Vector3[] waypoints;            // Transform을 Vector3 배열로 변환해서 저장

    void Start()
    {
        // 첫 스폰 시간 설정
        nextSpawnTime = Time.time + spawnInterval;

        // Transform 배열을 Vector3 배열로 변환
        ConvertWaypointsToVector3();

        // 스폰 포인트가 설정되지 않았으면 기본값 사용
        if (spawnPoint == null)
        {
            spawnPoint = transform; // 자기 자신의 위치 사용
        }
    }

    void Update()
    {
        if (isSpawning && Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab이 설정되지 않았습니다!");
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("Waypoints가 설정되지 않았습니다!");
            return;
        }

        // 적 생성
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        // 적에게 경로 설정 - 간단하게 한 번만!
        EnemyUnit enemyScript = newEnemy.GetComponent<EnemyUnit>();
        if (enemyScript != null)
        {
            enemyScript.Initialize(waypoints);  // 모든 설정을 한 번에 처리
        }
        else
        {
            Debug.LogError("Enemy Prefab에 EnemyUnit 스크립트가 없습니다!");
            Destroy(newEnemy);
            return;
        }

        enemiesSpawned++;
        Debug.Log($"적 생성! 총 {enemiesSpawned}마리");
    }

    void ConvertWaypointsToVector3()
    {
        if (waypointTransforms != null && waypointTransforms.Length > 0)
        {
            waypoints = new Vector3[waypointTransforms.Length];
            for (int i = 0; i < waypointTransforms.Length; i++)
            {
                if (waypointTransforms[i] != null)
                {
                    waypoints[i] = waypointTransforms[i].position;
                }
                else
                {
                    Debug.LogWarning($"Waypoint {i}가 설정되지 않았습니다!");
                }
            }
        }
        else
        {
            Debug.LogError("Waypoint Transforms가 설정되지 않았습니다!");
            // 백업 경로 제거 - 에디터에서 반드시 설정하도록 강제
            waypoints = null;
        }
    }

    // 외부에서 스폰 제어
    public void StartSpawning()
    {
        isSpawning = true;
        nextSpawnTime = Time.time + spawnInterval;
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void SetSpawnInterval(float newInterval)
    {
        spawnInterval = newInterval;
    }
}