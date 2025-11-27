using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 특정 지점에서 원뿔 형태로 객체를 감지하는 기능을 수행하는 클래스.
/// OverlapSphere를 이용한 광역 탐색 후, 벡터 내적을 통해 정밀 필터링을 수행합니다.
/// </summary>
public class ConeDetector : MonoBehaviour
{
    [Header("Ray Interactor")]
    [SerializeField] private RayInteractor _rayInteractor;
// --- 설정 가능한 파라미터 ---
    [Header("감지 설정")]
    public float detectionRadius = 10f; // 원뿔의 최대 반지름 (OverlapSphere의 반지름)
    public float detectionAngle = 5f;  // 원뿔의 반각 (전체 각도는 이 값의 2배)
    public LayerMask targetMask;        // 감지할 오브젝트들이 속한 레이어 마스크

    // --- 결과 ---
    [Header("감지 결과")]
    public List<GameObject> detectedTargets = new List<GameObject>();

    // --- 디버그 시각화 (선택 사항) ---
    [Header("디버그 시각화")]
    public Color coneColor = new Color(1f, 0.5f, 0f, 0.3f); // 원뿔 색상
    public bool drawGizmos = true;

    private Vector3 _rayOrigin;
    private Vector3 _rayForward;
    
    // 머티리얼 변경을 위한 변수
    private Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();
    private Material _redMaterial;

    void Awake()
    {
        // 감지된 오브젝트에 적용할 공유 빨간색 머티리얼 생성
        _redMaterial = new Material(Shader.Find("Standard"));
        _redMaterial.color = Color.red;
    }

    void Start()
    {
        
    }
    /// <summary>
    /// 전방 원뿔 범위 내의 모든 게임 오브젝트를 감지합니다.
    /// </summary>
    public void DetectTargetsInCone()
    {
        _rayOrigin = _rayInteractor.Origin;
        _rayForward = _rayInteractor.Forward;

        // 현재 프레임에서 감지된 모든 타겟을 찾습니다.
        var currentDetections = new List<GameObject>();
        Collider[] hitColliders = Physics.OverlapSphere(_rayOrigin, detectionRadius, targetMask);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == gameObject) continue;

            Vector3 directionToTarget = (hitCollider.transform.position - _rayOrigin).normalized;
            if (_rayForward == Vector3.zero || Vector3.Angle(_rayForward, directionToTarget) <= detectionAngle)
            {
                currentDetections.Add(hitCollider.gameObject);
            }
        }

        // 이전 프레임과 비교하여 새로 감지된 타겟과 더 이상 감지되지 않는 타겟을 찾습니다.
        var newlyDetected = currentDetections.Except(detectedTargets).ToList();
        var noLongerDetected = detectedTargets.Except(currentDetections).ToList();

        // 더 이상 감지되지 않는 타겟의 머티리얼을 원래대로 되돌립니다.
        foreach (var obj in noLongerDetected)
        {
            MoonObject mObject = obj.GetComponent<MoonObject>();
            if (mObject != null) mObject.SetStatus(MoonObject.MoonObjectStatus.Unselected);
        }

        // 새로 감지된 타겟의 머티리얼을 빨간색으로 변경합니다.
        foreach (var obj in newlyDetected)
        {
            MoonObject mObject = obj.GetComponent<MoonObject>();
            if (mObject != null) mObject.SetStatus(MoonObject.MoonObjectStatus.Selected);
        }

        // 감지된 타겟 리스트를 현재 상태로 업데이트합니다.
        detectedTargets = currentDetections;
    }
    
    /// <summary>
    /// 지정된 게임 오브젝트의 머티리얼을 빨간색으로 변경합니다.
    /// </summary>
    void ApplyRedMaterial(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return;

        // 원본 머티리얼을 아직 저장하지 않았다면 저장합니다.
        if (!_originalMaterials.ContainsKey(renderer))
        {
            _originalMaterials[renderer] = renderer.materials;
        }

        // 모든 머티리얼을 빨간색으로 교체하기 위한 새 배열을 생성합니다.
        var newMaterials = new Material[renderer.materials.Length];
        for (int i = 0; i < newMaterials.Length; i++)
        {
            newMaterials[i] = _redMaterial;
        }
        renderer.materials = newMaterials;
    }

    /// <summary>
    /// 지정된 게임 오브젝트의 머티리얼을 원래 상태로 되돌립니다.
    /// </summary>
    void RevertMaterial(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && _originalMaterials.ContainsKey(renderer))
        {
            renderer.materials = _originalMaterials[renderer];
            _originalMaterials.Remove(renderer);
        }
    }

    // 스크립트가 비활성화되거나 오브젝트가 파괴될 때 호출됩니다.
    void OnDisable()
    {
        // 모든 감지된 타겟의 머티리얼을 원래대로 되돌립니다.
        foreach (var obj in detectedTargets)
        {
            RevertMaterial(obj);
        }
        detectedTargets.Clear();
        _originalMaterials.Clear();
    }


    // 예시: Update 함수에서 매 프레임 감지 함수를 호출
    void Update()
    {
        switch(_rayInteractor.State)
        {
            case InteractorState.Normal:
                DetectTargetsInCone();
                break;
            case InteractorState.Hover:
                DetectTargetsInCone();
                break;
            default:
                break;
        }
        // 감지된 오브젝트 리스트를 활용하는 코드를 여기에 작성합니다.
        // 예: Debug.Log($"감지된 오브젝트 수: {detectedTargets.Count}");
    }
    
    // --- Unity 에디터 시각화를 위한 Gizmos (선택 사항) ---
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Vector3 origin = _rayOrigin;
        Vector3 forward = _rayForward;

        if (forward == Vector3.zero)
        {
            // Play 모드가 아닐 때 Editor에서 기본 값을 사용하도록 설정
            if (!Application.isPlaying)
            {
                origin = transform.position;
                forward = transform.forward;
            }
            else
            {
                 return;
            }
        }

        // 1. OverlapSphere의 경계 그리기 (디버깅용)
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(origin, detectionRadius);

        // 2. 원뿔 범위 시각화
        Gizmos.color = coneColor;

        // forward 벡터로부터 회전을 만들고, 그 회전을 바탕으로 up과 right 벡터를 구합니다.
        // 이렇게 하면 transform.up과 transform.right에 대한 의존성이 사라집니다.
        Quaternion orientation = Quaternion.LookRotation(forward, Vector3.up);
        Vector3 up = orientation * Vector3.up;
        Vector3 right = orientation * Vector3.right;
        
        float angle = detectionAngle;

        Vector3 upDir = Quaternion.AngleAxis(angle, right) * forward;
        Vector3 downDir = Quaternion.AngleAxis(-angle, right) * forward;
        Vector3 leftDir = Quaternion.AngleAxis(-angle, up) * forward;
        Vector3 rightDir = Quaternion.AngleAxis(angle, up) * forward;

        Gizmos.DrawRay(origin, upDir * detectionRadius);
        Gizmos.DrawRay(origin, downDir * detectionRadius);
        Gizmos.DrawRay(origin, leftDir * detectionRadius);
        Gizmos.DrawRay(origin, rightDir * detectionRadius);

        // 원뿔의 밑면에 원 그리기 (Editor 전용 코드)
#if UNITY_EDITOR
        Handles.color = coneColor;
        Vector3 coneBaseCenter = origin + forward * detectionRadius;
        float coneBaseRadius = detectionRadius * Mathf.Tan(angle * Mathf.Deg2Rad);
        Handles.DrawWireDisc(coneBaseCenter, forward, coneBaseRadius);
#endif

        // 3. 감지된 타겟 표시
        Gizmos.color = Color.red;
        foreach (GameObject target in detectedTargets)
        {
            if (target != null)
            {
                Gizmos.DrawSphere(target.transform.position, 0.5f);
            }
        }
    }
}
