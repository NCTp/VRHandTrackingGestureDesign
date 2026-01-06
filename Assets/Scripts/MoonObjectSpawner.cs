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
    private float _radius = 2.0f; 
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

    [Header("Occlusion Settings")]
    public Transform userHead; // [추가됨] VR 카메라(CenterEyeAnchor)를 여기에 할당하세요.
    [Range(0.1f, 1.0f)]
    public float occlusionDistance = 0.5f; // [추가됨] 타겟보다 얼마나 앞에 배치할지 (단위: 미터)

    // [추가됨] 부분 차폐를 위한 오프셋 (단위: 미터)
    // 오브젝트의 크기(반지름)에 따라 조절하세요. 
    // 예: 오브젝트 지름이 0.2라면, 0.1 정도 주면 절반 정도 겹침.
    [Range(0.01f, 0.5f)]
    public float partialOcclusionOffset = 0.1f;

    void Start()
    {
        //ReGenerateObjects();
        switch(ExperimentManager.Instance.spawnDensity)
        {
            case SpawnDensity.Low: 
                SetSpawnVariables(50,2.5f,1.0f,1.5f);
                break;
            case SpawnDensity.Normal:
                SetSpawnVariables(75,2.5f,1.0f,1.25f);
                break;
            case SpawnDensity.High:
                SetSpawnVariables(100,2.5f,0.75f,1.0f);
                break;
            default:
                break;
        }

        _spawnCount = ExperimentManager.Instance.spawnCount;

    }

    private void SetSpawnVariables(int objectNumber, float radius, float minDistanceBetweenObjects, float maxDistanceBetweenObjects)
    {
        _objectNumber = objectNumber;
        _radius = radius;
        _minDistanceBetweenObjects = minDistanceBetweenObjects;
        _maxDistanceBetweenObjects = maxDistanceBetweenObjects;
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
        Vector3 firstPos = transform.position + Random.insideUnitSphere * _radius;
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
                if (Vector3.Distance(candidatePos, transform.position) > _radius) continue;

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
        Gizmos.DrawSphere(transform.position, _radius);
    }

    public void ReGenerateObjects()
    {
        if(_spawnCount > 0)
        {
            ClearObjects();
            GenerateObjects();
            SetTargetObject(); 
            // Target 설정 후 가리는 로직 실행
            EnsureTargetOcclusion(); // [추가됨] 여기서 가리기 실행
        }
        else
        {
            ClearObjects();
            ExperimentManager.Instance.EndExperiment();
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

    // [추가됨] 타겟을 가리는 핵심 함수
    void EnsureTargetOcclusion()
    {
        // 1. 안전 장치
        if (_targetMoonObject == null || _moonObjects.Count < 2 || userHead == null) 
        {
            Debug.LogWarning("가리기(Occlusion) 조건 부족");
            return;
        }

        // 2. 방해꾼(Blocker) 선정
        GameObject blocker = null;
        int safetyCount = 0;
        while(blocker == null || blocker == _targetMoonObject.gameObject)
        {
            int rndIdx = Random.Range(0, _moonObjects.Count);
            blocker = _moonObjects[rndIdx];
            
            safetyCount++;
            if(safetyCount > 100) break; // 무한루프 방지
        }
        
        if (blocker == null) return;

        // 3. 위치 계산 (벡터 수학)
        Vector3 headPos = userHead.position;
        Vector3 targetPos = _targetMoonObject.transform.position;
        
        // (A) 시선 방향 벡터 (눈 -> 타겟)
        Vector3 directionToTarget = (targetPos - headPos).normalized;
        
        // (B) 기본 차폐 위치 (타겟 바로 앞)
        Vector3 baseBlockerPos = targetPos - (directionToTarget * occlusionDistance);

        // (C) [핵심] 시선에 수직인 랜덤한 방향 구하기
        // 무작위 벡터를 시선 벡터가 만드는 평면에 투영시켜서 수직 성분만 뽑아냄
        Vector3 randomDir = Random.onUnitSphere;
        Vector3 perpendicularDir = Vector3.ProjectOnPlane(randomDir, directionToTarget).normalized;

        // (D) 최종 위치: 기본 위치에서 수직 방향으로 살짝 이동
        // 이렇게 하면 타겟이 Blocker 뒤에서 살짝 '빼꼼' 하고 보이게 됩니다.
        Vector3 finalPos = baseBlockerPos + (perpendicularDir * partialOcclusionOffset);

        // 4. 방해꾼 이동
        blocker.transform.position = finalPos;
        
        // (선택) 방해꾼이 타겟을 바라보게 하면 더 자연스러움
        // blocker.transform.LookAt(headPos); 
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

    public void RecordTCT(bool input)
    {
        // 선택이 틀렸다면
        if(_moonObjects.Count > 0 && input == false)
        {
            ExperimentManager.Instance.AddCount(input);
            _spawnCount -= 1;

            return;
        }
        // 만약 잘 선택했다면
        if (_moonObjects.Count > 0 && input == true)
        {
            float duration = Time.time - _generationStartTime;
            //Debug.LogWarning($"[Result] Task Duration (Generate to Clear): {duration:F4} seconds");
            ExperimentManager.Instance.SaveTaskCompletionTimeEachTrial(duration);
            ExperimentManager.Instance.AddCount(input);
            _spawnCount -= 1;

            return;
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