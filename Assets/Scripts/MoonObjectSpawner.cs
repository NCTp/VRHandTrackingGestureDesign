using UnityEngine;
using System.Collections.Generic;

public class MoonObjectSpawner : Singleton<MoonObjectSpawner>
{
    public enum SpawnDensity
    {
        Low,
        Normal,
        High
    }
    [Header("Spawn Settings")]
    public GameObject moonObject; 
    private int _objectNumber = 50; 
    private float radius = 5f; 
    private int _spawnCount = 30;

    [Header("Distance Constraints")]
    private float _minDistanceBetweenObjects = 1f; 
    private float _maxDistanceBetweenObjects = 2f; 

    [Header("Generation")]
    public int maxPlacementAttempts = 100; 

    private List<Vector3> spawnedPositions = new List<Vector3>();
    public List<GameObject> _moonObjects = new List<GameObject>(); 
    public MoonObject _targetMoonObject; 

    // --- [추가됨] 시간 측정을 위한 변수 ---
    private float _generationStartTime; 
    // ------------------------------------

    void Start()
    {
        //ReGenerateObjects();
        switch(ExperimentManager.Instance.spawnDensity)
        {
            case SpawnDensity.Low: 
                _objectNumber = 50;
                _minDistanceBetweenObjects = 1.0f;
                _maxDistanceBetweenObjects = 1.5f;
                break;
            case SpawnDensity.Normal:
                _objectNumber = 75;
                _minDistanceBetweenObjects = 1.0f;
                _maxDistanceBetweenObjects = 1.25f;
                break;
            case SpawnDensity.High:
                _objectNumber = 100;
                _minDistanceBetweenObjects = 0.75f;
                _maxDistanceBetweenObjects = 1.25f;
                break;
            default:
                break;
        }

        _spawnCount = ExperimentManager.Instance.spawnCount;

    }

    void Update()
    {
    }

    public void GenerateObjects()
    {
        // 기존에 스폰된 오브젝트들 삭제 (안전장치)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        spawnedPositions.Clear();

        if (_objectNumber <= 0 || moonObject == null)
        {
            Debug.LogWarning("스폰할 오브젝트나 개수가 설정되지 않았습니다.");
            return;
        }
        
        if (_minDistanceBetweenObjects > _maxDistanceBetweenObjects)
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
        for (int i = 1; i < _objectNumber; i++)
        {
            bool positionFound = false;
            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                int anchorIndex = Random.Range(0, spawnedPositions.Count);
                Vector3 anchorPos = spawnedPositions[anchorIndex];

                float distance = Random.Range(_minDistanceBetweenObjects, _maxDistanceBetweenObjects);
                Vector3 candidatePos = anchorPos + Random.onUnitSphere * distance;

                // 유효성 검사
                if (Vector3.Distance(candidatePos, transform.position) > radius) continue;

                bool respectsMinDistance = true;
                foreach (Vector3 pos in spawnedPositions)
                {
                    if (Vector3.Distance(candidatePos, pos) < _minDistanceBetweenObjects)
                    {
                        respectsMinDistance = false;
                        break;
                    }
                }

                if (!respectsMinDistance) continue;

                positionFound = true;
                spawnedPositions.Add(candidatePos);
                GameObject temp = Instantiate(moonObject, candidatePos, Quaternion.identity, transform);
                temp.gameObject.name = _moonObjects.Count.ToString();
                _moonObjects.Add(temp);
                break; 
            }

            if (!positionFound)
            {
                Debug.LogWarning($"오브젝트 {i + 1}의 유효한 위치를 찾지 못했습니다. 스폰을 중단합니다.");
            }
        }

        // --- [추가됨] 생성 로직이 끝난 직후 시간을 기록합니다. ---
        _generationStartTime = Time.time;
        //Debug.LogWarning($"[Time Check] Objects Generated at: {_generationStartTime}");
        // --------------------------------------------------------
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, radius);
    }

    public void ReGenerateObjects()
    {
        
        if(_spawnCount > 0)
        {
            ClearObjects();
            GenerateObjects();
            SetTargetObject();
            //_spawnCount -= 1;
        }
        else
        {
            ClearObjects();
            ExperimentManager.Instance.EndExperiment();
            //Debug.Log("Experiment End!");
        }
    }

    void SetTargetObject()
    {
        if(_moonObjects.Count > 0)
        {
            int temp = Random.Range(0, _moonObjects.Count);
            _targetMoonObject = _moonObjects[temp].GetComponent<MoonObject>();
            if(_targetMoonObject)
            {
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
        //Debug.LogWarning("Objects Cleared!");
    }

    public void RecordTCT()
    {
        // --- [추가됨] 오브젝트가 존재했다면, 현재 시간과 생성 시간의 차이를 계산하여 출력합니다. ---
        if (_moonObjects.Count > 0)
        {
            float duration = Time.time - _generationStartTime;
            //Debug.LogWarning($"[Result] Task Duration (Generate to Clear): {duration:F4} seconds");
            ExperimentManager.Instance.SaveTaskCompletionTimeEachTrial(duration);
            _spawnCount -= 1;
        }
    }

    public void ClearObjectsStatus()
    { 
        for(int i = 0; i < _moonObjects.Count; i++)
        {
            MoonObject temp = _moonObjects[i].GetComponent<MoonObject>();
            if(temp) temp.SetStatus(MoonObject.MoonObjectStatus.Unselected);
        }
        // _moonObjects.Clear(); // (이전 리뷰 의견: 단순히 상태 초기화라면 이 줄은 지우는 것이 좋습니다)
        //Debug.LogWarning("Objects Status Cleared!");
    }
}