using UnityEngine;
using System.Collections.Generic;
using System.Collections; // [필수 추가] IEnumerator 사용을 위해 필요
using System.Linq;

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
    private float _partialOcclusionOffset = 0.1f;
    private float _offset = 0.1f;
    // [설정] 생성 대기 시간
    [Header("Time Settings")]
    public float spawnDelay = 3.0f; // 3초 딜레이

    [Header("UI")]
    public CanvasTimer canvasTimer; // [할당 필요] 에디터에서 CanvasTimer가 붙은 오브젝트를 연결하세요.

    void Start()
    {
        //ReGenerateObjects();
        switch(ExperimentManager.Instance.spawnDensity)
        {
            case SpawnDensity.Low: 
                _offset = Random.Range(0.1f, 0.15f);
                SetSpawnVariables(50,2.5f,1.0f,1.5f, _offset);
                break;
            case SpawnDensity.Normal:
                _offset = Random.Range(0.075f, 0.1f);
                SetSpawnVariables(75,2.5f,0.75f,1.25f, _offset);
                break;
            case SpawnDensity.High:
                _offset = Random.Range(0.05f, 0.1f);
                SetSpawnVariables(100,2.5f,0.75f,1.0f, _offset);
                break;
            default:
                break;
        }

        _spawnCount = ExperimentManager.Instance.spawnCount;

    }

    private void SetSpawnVariables(int objectNumber, float radius, float minDistanceBetweenObjects, float maxDistanceBetweenObjects, float occlusionOffset)
    {
        _objectNumber = objectNumber;
        _radius = radius;
        _minDistanceBetweenObjects = minDistanceBetweenObjects;
        _maxDistanceBetweenObjects = maxDistanceBetweenObjects;
        _partialOcclusionOffset = occlusionOffset;
    }

    void Update()
    {
        
    }
    // 기존 MoonObjectSpawner 내부에 추가
    public List<Vector3> GeneratePoissonPoints3D(float radius, float minDistance, int targetCount, int k = 30)
    {
        // 1. 반환할 포인트 리스트
        List<Vector3> points = new List<Vector3>();
        
        // 2. 활성 리스트 (새로운 점을 낳을 수 있는 부모 점들)
        List<Vector3> activeList = new List<Vector3>();

        // 3. 첫 번째 점 추가 (구의 중심 혹은 랜덤)
        Vector3 firstPoint = Random.insideUnitSphere * radius; 
        // 혹은 중심에서 시작하려면: Vector3 firstPoint = Vector3.zero;
        
        points.Add(firstPoint);
        activeList.Add(firstPoint);

        // 4. 루프 시작 (활성 리스트가 빌 때까지 혹은 목표 개수 도달 시)
        // *주의: Poisson은 원래 꽉 채우는게 목적이나, 성능을 위해 targetCount의 2배 정도면 멈추도록 설정 가능
        while (activeList.Count > 0)
        {
            int randIndex = Random.Range(0, activeList.Count);
            Vector3 center = activeList[randIndex];
            bool found = false;

            for (int i = 0; i < k; i++)
            {
                // 반지름 r ~ 2r 사이의 랜덤한 점 생성 (도넛 모양)
                Vector3 randomDir = Random.onUnitSphere;
                float distance = Random.Range(minDistance, 2 * minDistance);
                Vector3 candidate = center + randomDir * distance;

                // [검증 1] 전체 생성 구역(Radius) 안에 있는가? (transform.position 기준 로컬 좌표라 가정하고 크기만 체크)
                if (candidate.magnitude > radius) continue;

                // [검증 2] 기존 점들과 너무 가깝지 않은가?
                // (최적화를 위해 Grid를 쓰기도 하지만, N < 500 일 땐 이중 루프도 충분히 빠릅니다)
                bool farEnough = true;
                foreach (var p in points)
                {
                    if (Vector3.SqrMagnitude(candidate - p) < minDistance * minDistance)
                    {
                        farEnough = false;
                        break;
                    }
                }

                // 유효한 점 발견!
                if (farEnough)
                {
                    points.Add(candidate);
                    activeList.Add(candidate);
                    found = true;
                    break; // 한 번 찾으면 바로 다음 부모로 넘어감 (Bridson 방식 변형)
                }
            }

            // k번 시도해도 실패하면 이 점 근처는 꽉 찬 것임 -> 활성 리스트에서 제거
            if (!found)
            {
                activeList.RemoveAt(randIndex);
            }
        }

        return points;
    }

    public void GenerateObjects()
    {
        ClearObjects(); // 기존 삭제

        // 1. Poisson Disk로 후보 위치들을 넉넉하게 생성 (목표 개수보다 여유 있게 뽑힘)
        // targetCount 인자는 루프 조기 종료용으로만 쓰거나 생략해도 됩니다.
        List<Vector3> candidates = GeneratePoissonPoints3D(_radius, _minDistanceBetweenObjects, _objectNumber * 2);

        // 2. 생성된 포인트가 목표 개수보다 적다면 경고 (밀도 설정 오류)
        if (candidates.Count < _objectNumber)
        {
            Debug.LogWarning($"설정된 밀도(MinDist: {_minDistanceBetweenObjects})가 너무 높아 " +
                            $"목표 개수({_objectNumber})를 채우지 못하고 {candidates.Count}개만 생성되었습니다.");
        }

        // 3. 무작위로 섞음 (Shuffle) - 앞쪽부터 잘라 쓰기 위해
        // (System.Linq가 없으면 직접 Swap 셔플 구현 필요)
        var shuffledPositions = candidates.OrderBy(x => Random.value).Take(_objectNumber).ToList();

        // 4. 오브젝트 인스턴스화
        for (int i = 0; i < shuffledPositions.Count; i++)
        {
            // 로컬 좌표로 계산했으므로 transform.position을 더해줌
            Vector3 worldPos = transform.position + shuffledPositions[i];
            
            GameObject temp = Instantiate(moonObject, worldPos, Quaternion.identity, transform);
            temp.name = i.ToString();
            _moonObjects.Add(temp);
        }

        _generationStartTime = Time.time;
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
            // 실행 중인 코루틴이 있다면 중복 실행 방지를 위해 정지 (선택 사항)
            StopAllCoroutines(); 
            
            // 3초 뒤에 생성하는 시퀀스 시작
            StartCoroutine(SpawnSequence());
        }
        else
        {
            ClearObjects();
            ExperimentManager.Instance.EndExperiment();
        }
    }
    // [추가됨] 3초 대기 후 생성을 담당하는 코루틴
    IEnumerator SpawnSequence()
    {
        // 1. 화면 비우기
        ClearObjects(); 

        // 2. 타이머 UI 켜기
        if(canvasTimer != null) canvasTimer.SetVisible(true);

        // 3. 카운트다운 로직
        float remainingTime = spawnDelay;
        
        while(remainingTime > 0)
        {
            // UI 갱신
            if(canvasTimer != null) 
            {
                canvasTimer.SetTimerText(remainingTime);
            }

            // 시간 감소
            remainingTime -= Time.deltaTime;

            // 다음 프레임까지 대기
            yield return null; 
        }

        // 4. 대기 종료 후 타이머 UI 끄기
        if(canvasTimer != null) canvasTimer.SetVisible(false);

        // 5. 오브젝트 생성 로직 실행
        GenerateObjects(); 
        SetTargetObject(); 
        EnsureTargetOcclusion();
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
        Vector3 finalPos = baseBlockerPos + (perpendicularDir * _partialOcclusionOffset);

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