using UnityEngine;
using System.Collections.Generic;

public class MoonObjectSpawner : Singleton<MoonObjectSpawner>
{
    private enum SpawnDensity
    {
        Low,
        Normal,
        High
    }
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
    public List<GameObject> _moonObjects = new List<GameObject>(); // 스폰된 MoonObject 리스트
    public MoonObject _targetMoonObject; // 이번에 타겟으로 지정된 MoonObject

    void Start()
    {
        ReGenerateObjects();
    }

    void Update()
    {
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
        GameObject firstObject = Instantiate(moonObject, firstPos, Quaternion.identity, transform);
        firstObject.gameObject.name = _moonObjects.Count.ToString();
        _moonObjects.Add(firstObject);

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

                // 모든 검사를 통과하면 위치 확정
                positionFound = true;
                spawnedPositions.Add(candidatePos);
                GameObject temp = Instantiate(moonObject, candidatePos, Quaternion.identity, transform);
                temp.gameObject.name = _moonObjects.Count.ToString();
                _moonObjects.Add(temp);
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
    public void ReGenerateObjects()
    {
        ClearObjects();
        GenerateObjects();
        SetTargetObject();
    }
    void SetTargetObject()
    {
        if(_moonObjects.Count > 0)
        {
            int temp = Random.Range(0, _moonObjects.Count);
            _targetMoonObject = _moonObjects[temp].GetComponent<MoonObject>();
            if(_targetMoonObject)
            {
                //_targetMoonObject.SetOutline(true);
                _targetMoonObject.SetTargetMat();
                _targetMoonObject.SetIsTarget(true);
            }
        }
    }
    void ClearObjects()
    { 
        for(int i = 0; i < _moonObjects.Count; i++)
        {
            Destroy(_moonObjects[i].gameObject);
        }
        _moonObjects.Clear();
        Debug.LogWarning("Objects Cleared!");
    }

    public void ClearObjectsStatus()
    { 
        for(int i = 0; i < _moonObjects.Count; i++)
        {
            MoonObject temp = _moonObjects[i].GetComponent<MoonObject>();
            if(temp) temp.SetStatus(MoonObject.MoonObjectStatus.Unselected);
        }
        _moonObjects.Clear();
        Debug.LogWarning("Objects Cleared!");
    }
}
