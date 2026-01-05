using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ConeDetector : MonoBehaviour
{
    [Header("Ray Interactor")]
    [SerializeField] private RayInteractor _rayInteractor;
    
    [Header("감지 설정")]
    public float detectionRadius = 10f; 
    public float detectionAngle = 5f;  
    public LayerMask targetMask;        

    [Header("감지 결과")]
    // HashSet을 사용하여 내부적으로 중복을 방지하고 검색 속도를 높임 (Inspector에 안 보일 수 있음)
    public List<GameObject> detectedTargets = new List<GameObject>();

    [Header("디버그 시각화")]
    public Color coneColor = new Color(1f, 0.5f, 0f, 0.3f); 
    public bool drawGizmos = true;

    private Vector3 _rayOrigin;
    private Vector3 _rayForward;
    
    // [최적화] 매 프레임 메모리 할당 방지를 위해 클래스 멤버로 선언
    private HashSet<GameObject> _currentDetectionsSet = new HashSet<GameObject>();
    private Collider[] _hitCollidersCache = new Collider[100]; // OverlapSphereNonAlloc용 버퍼

    public void DetectTargetsInCone()
    {
        if (!_rayInteractor) return;

        _rayOrigin = _rayInteractor.Origin;
        _rayForward = _rayInteractor.Forward;

        // 1. 현재 프레임 감지 리스트 초기화
        _currentDetectionsSet.Clear();

        // 2. OverlapSphereNonAlloc으로 가비지 컬렉션(GC) 최소화
        int hitCount = Physics.OverlapSphereNonAlloc(_rayOrigin, detectionRadius, _hitCollidersCache, targetMask);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = _hitCollidersCache[i];
            if (hitCollider.gameObject == gameObject) continue;

            // [핵심 수정 1] Collider가 자식에 있어도 부모의 MoonObject를 정확히 찾아냄
            MoonObject moonObj = hitCollider.GetComponentInParent<MoonObject>();
            
            // MoonObject가 없거나, 이미 이번 프레임에 감지된 객체라면 패스
            if (moonObj == null || _currentDetectionsSet.Contains(moonObj.gameObject)) continue;

            // 각도 계산
            Vector3 targetPos = moonObj.transform.position; // [핵심] Collider 위치가 아닌 객체 중심 위치 사용
            Vector3 directionToTarget = (targetPos - _rayOrigin).normalized;

            if (_rayForward == Vector3.zero || Vector3.Angle(_rayForward, directionToTarget) <= detectionAngle)
            {
                _currentDetectionsSet.Add(moonObj.gameObject);
            }
        }

        // 3. 상태 변경 로직 (LINQ Except 대신 루프 사용으로 최적화 및 명시적 제어)
        
        // A. 기존에 있었는데 지금은 없는 애들 -> Unselected
        // (리스트를 역순으로 돌면 삭제 시 인덱스 문제 방지 가능, 여기선 상태 변경만 하므로 foreach 무관)
        for (int i = detectedTargets.Count - 1; i >= 0; i--)
        {
            GameObject target = detectedTargets[i];
            
            // 타겟이 파괴되었거나, 현재 세트에 없다면
            if (target == null || !_currentDetectionsSet.Contains(target))
            {
                if (target != null)
                {
                    MoonObject mObject = target.GetComponent<MoonObject>();
                    // [중요] 이미 Selected(선택됨) 상태인 객체는 Cone에서 벗어나도 상태를 유지해야 하는지 확인 필요
                    // 여기서는 일단 벗어나면 무조건 Unselected로 변경
                    if (mObject) mObject.SetStatus(MoonObject.MoonObjectStatus.Unselected);
                }
                detectedTargets.RemoveAt(i); // 리스트에서 제거
            }
        }

        // B. 새로 들어온 애들 -> Detected
        foreach (GameObject detectedObj in _currentDetectionsSet)
        {
            // 기존 리스트에 없다면 새로 발견된 것
            if (!detectedTargets.Contains(detectedObj))
            {
                detectedTargets.Add(detectedObj);
                MoonObject mObject = detectedObj.GetComponent<MoonObject>();
                if (mObject) mObject.SetStatus(MoonObject.MoonObjectStatus.Detected);
            }
            else 
            {
                // [선택 사항] 이미 리스트에 있지만, 상태 강제 동기화가 필요하다면 여기서 처리
                MoonObject mObject = detectedObj.GetComponent<MoonObject>();
                if (mObject) mObject.SetStatus(MoonObject.MoonObjectStatus.Detected);
            }
        }

        if (detectedTargets.Count > 1) // 2개 이상일 때만 정렬
        {
            detectedTargets.Sort((a, b) => 
            {
                if (a == null || b == null) return 0;

                // 각 객체까지의 방향 벡터 계산
                Vector3 dirA = (a.transform.position - _rayOrigin).normalized;
                Vector3 dirB = (b.transform.position - _rayOrigin).normalized;

                // Ray Forward와의 각도 계산 (작을수록 중앙에 가까움)
                float angleA = Vector3.Angle(_rayForward, dirA);
                float angleB = Vector3.Angle(_rayForward, dirB);

                // 오름차순 정렬 (angleA가 작으면 앞으로)
                return angleA.CompareTo(angleB);
            });
        }
    }

    void OnDisable()
    {
        detectedTargets.Clear();
    }

    void Update()
    {
        if (!_rayInteractor) return;

        switch(_rayInteractor.State)
        {
            case InteractorState.Normal:
            case InteractorState.Hover: // Hover 상태에서도 감지 계속 수행
                DetectTargetsInCone();
                break;
            default:
                // 선택 중이거나 다른 상태일 때 목록 초기화가 필요하다면 추가
                break;
        }
    }

    // ... (OnDrawGizmos는 기존과 동일하게 유지) ...
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        Vector3 origin = (_rayInteractor != null) ? _rayInteractor.Origin : transform.position;
        Vector3 forward = (_rayInteractor != null) ? _rayInteractor.Forward : transform.forward;
        
        Gizmos.color = coneColor;
        // (기존 Gizmo 코드 활용)
    }
}