using UnityEngine;
using System.Collections.Generic;

public class MoonObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject moonObject; // 스폰할 게임 오브젝트 프리팹
    public int objectNumber = 30; // 스폰할 오브젝트의 수
    public float radius = 5f; // 스폰될 구의 반지름

    [Header("Distance Constraints")]
    public float minDistanceBetweenObjects = 1f; // 오브젝트 간의 최소 거리
    public float maxDistanceBetweenObjects = 2f; // 오브젝트 간의 최대 거리

    [Header("Generation")]
    public int maxPlacementAttempts = 100; // 각 오브젝트 배치 시 최대 시도 횟수

    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Start()
    {
        GenerateObjects();
    }

    public void GenerateObjects()
    {
        // 기존에 스폰된 오브젝트들 삭제
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        spawnedPositions.Clear();

        if (objectNumber <= 0 || moonObject == null)
        {
            Debug.LogWarning("스폰할 오브젝트나 개수가 설정되지 않았습니다.");
            return;
        }
        
        if (minDistanceBetweenObjects > maxDistanceBetweenObjects)
        {
            Debug.LogError("최소 거리가 최대 거리보다 클 수 없습니다.");
            return;
        }

        // 1. 첫 번째 오브젝트 배치
        Vector3 firstPos = transform.position + Random.insideUnitSphere * radius;
        spawnedPositions.Add(firstPos);
        Instantiate(moonObject, firstPos, Quaternion.identity, transform);

        // 2. 나머지 오브젝트 배치
        for (int i = 1; i < objectNumber; i++)
        {
            bool positionFound = false;
            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                // 기존에 배치된 오브젝트 중 하나를 무작위로 선택하여 앵커로 삼음
                int anchorIndex = Random.Range(0, spawnedPositions.Count);
                Vector3 anchorPos = spawnedPositions[anchorIndex];

                // 앵커 주위의 구형 셸(spherical shell) 내에 새로운 위치 후보 생성
                float distance = Random.Range(minDistanceBetweenObjects, maxDistanceBetweenObjects);
                Vector3 candidatePos = anchorPos + Random.onUnitSphere * distance;

                // --- 후보 위치 유효성 검사 ---

                // a) 주 스폰 영역(구) 내에 있는지 확인
                if (Vector3.Distance(candidatePos, transform.position) > radius)
                {
                    continue; // 영역을 벗어나면 다시 시도
                }

                // b) 다른 모든 스폰된 오브젝트와의 최소 거리를 만족하는지 확인
                bool respectsMinDistance = true;
                foreach (Vector3 pos in spawnedPositions)
                {
                    if (Vector3.Distance(candidatePos, pos) < minDistanceBetweenObjects)
                    {
                        respectsMinDistance = false;
                        break;
                    }
                }

                if (!respectsMinDistance)
                {
                    continue; // 최소 거리를 만족하지 않으면 다시 시도
                }
                
                // c) 가장 가까운 이웃과의 거리가 최대 거리 제약을 만족하는지 확인
                // 이 로직은 후보 위치 생성 방식에 의해 암시적으로 처리될 수 있지만,
                // 복잡한 배치에서는 보장되지 않으므로 명시적으로 확인하는 것이 안전할 수 있습니다.
                // 하지만 현재 생성 방식에서는 이중 확인이 될 수 있으므로 생략합니다.
                /*
                float nearestDist = float.MaxValue;
                foreach(var pos in spawnedPositions)
                {
                    nearestDist = Mathf.Min(nearestDist, Vector3.Distance(candidatePos, pos));
                }
                if (nearestDist > maxDistanceBetweenObjects)
                {
                    continue;
                }
                */

                // 모든 검사를 통과하면 위치 확정
                positionFound = true;
                spawnedPositions.Add(candidatePos);
                Instantiate(moonObject, candidatePos, Quaternion.identity, transform);
                break; // 다음 오브젝트 배치로 넘어감
            }

            if (!positionFound)
            {
                Debug.LogWarning($"오브젝트 {i + 1}의 유효한 위치를 찾지 못했습니다. 스폰을 중단합니다.");
                // 모든 오브젝트를 스폰할 수 없는 경우, 여기서 멈추거나 다른 처리를 할 수 있습니다.
                // return; 
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // 스폰 영역을 기즈모로 표시
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
