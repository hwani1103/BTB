using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("���� ����")]
    public GameObject enemyPrefab;          // ������ �� ������
    public float spawnInterval = 3f;        // ���� ���� (��)
    public Transform spawnPoint;            // ���� ��ġ

    [Header("üũ����Ʈ ���")]
    public Transform[] waypointTransforms;  // ������ ���� ��� (Transform �迭)

    [Header("���� ����")]
    public bool isSpawning = true;          // ���� Ȱ��ȭ ����
    public int enemiesSpawned = 0;          // ������ �� �� (����)

    private float nextSpawnTime;
    private Vector3[] waypoints;            // Transform�� Vector3 �迭�� ��ȯ�ؼ� ����

    void Start()
    {
        // ù ���� �ð� ����
        nextSpawnTime = Time.time + spawnInterval;

        // Transform �迭�� Vector3 �迭�� ��ȯ
        ConvertWaypointsToVector3();

        // ���� ����Ʈ�� �������� �ʾ����� �⺻�� ���
        if (spawnPoint == null)
        {
            spawnPoint = transform; // �ڱ� �ڽ��� ��ġ ���
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
            Debug.LogError("Enemy Prefab�� �������� �ʾҽ��ϴ�!");
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("Waypoints�� �������� �ʾҽ��ϴ�!");
            return;
        }

        // �� ����
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        // ������ ��� ���� - �����ϰ� �� ����!
        EnemyUnit enemyScript = newEnemy.GetComponent<EnemyUnit>();
        if (enemyScript != null)
        {
            enemyScript.Initialize(waypoints);  // ��� ������ �� ���� ó��
        }
        else
        {
            Debug.LogError("Enemy Prefab�� EnemyUnit ��ũ��Ʈ�� �����ϴ�!");
            Destroy(newEnemy);
            return;
        }

        enemiesSpawned++;
        Debug.Log($"�� ����! �� {enemiesSpawned}����");
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
                    Debug.LogWarning($"Waypoint {i}�� �������� �ʾҽ��ϴ�!");
                }
            }
        }
        else
        {
            Debug.LogError("Waypoint Transforms�� �������� �ʾҽ��ϴ�!");
            // ��� ��� ���� - �����Ϳ��� �ݵ�� �����ϵ��� ����
            waypoints = null;
        }
    }

    // �ܺο��� ���� ����
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