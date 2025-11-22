using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;

/// <summary>
/// 특정 지점에서 원뿔 형태로 객체를 감지하는 기능을 수행하는 클래스.
/// OverlapSphere를 이용한 광역 탐색 후, 벡터 내적을 통해 정밀 필터링을 수행합니다.
/// </summary>
public class ConeDetector : MonoBehaviour
{
    [Header("Cone Parameters")]
    [SerializeField] 
    private RayInteractor _rayInteractor; // Ray Interactor 가져오기.
    [Tooltip("원뿔의 감지 범위 (반지름)")]
    public float range = 10f;

    [Tooltip("원뿔의 전체 각도 (Degrees)")]
    [Range(0, 360)]
    public float angle = 90f;

    [Header("Targeting")]
    [Tooltip("감지할 객체의 레이어 마스크")]
    public LayerMask targetLayer;

    private List<GameObject> _detectedObjects = new List<GameObject>();
    private Vector3 _origin; // 감지 시작점
    private Vector3 _forward; // 감지 방향
    public IReadOnlyList<GameObject> DetectedObjects => _detectedObjects;
    
    void Start()
    {
        _origin = _rayInteractor.Origin;
        _forward = _rayInteractor.Forward;
    }
    void Update()
    {
        FindObjectsInCone();
    }

    /// <summary>
    /// 원뿔 범위 내의 객체를 탐색하고 리스트를 업데이트합니다.
    /// </summary>
    public void FindObjectsInCone()
    {
        _detectedObjects.Clear();

        // 1. 광역 단계: OverlapSphere로 잠재적 객체 수집
        Collider[] colliders = Physics.OverlapSphere(_origin, range, targetLayer);

        if (colliders.Length == 0) return;
        
        // 원뿔의 절반 각도를 미리 계산 (코사인 계산용)
        float halfAngleRad = (angle / 2f) * Mathf.Deg2Rad;
        float coneDotThreshold = Mathf.Cos(halfAngleRad);

        // 원뿔의 정면 방향 벡터
        Vector3 coneDirection = _forward;

        foreach (Collider col in colliders)
        {
            // 2. 정밀 단계: 벡터 내적을 이용한 필터링
            Vector3 vectorToTarget = (col.transform.position - _origin).normalized;

            // 내적 값이 임계치보다 크면 원뿔 내에 존재
            if (Vector3.Dot(coneDirection, vectorToTarget) > coneDotThreshold)
            {
                // 필요하다면, Raycast를 통해 시야를 가리는 장애물이 없는지 추가로 확인할 수 있습니다.
                // if (!Physics.Raycast(transform.position, vectorToTarget, Vector3.Distance(transform.position, col.transform.position), obstacleLayer))
                // {
                //    _detectedObjects.Add(col.gameObject);
                // }
                _detectedObjects.Add(col.gameObject);
                Debug.LogWarning("Detected: " + col.gameObject.name);
            }
        }
    }

    /// <summary>
    /// 에디터에서 원뿔 범위를 시각적으로 표시하기 위한 Gizmo
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_origin, range);

        Vector3 forward = _forward;
        float halfAngle = angle / 2f;

        Vector3 leftRayRotation = Quaternion.AngleAxis(-halfAngle, transform.up) * forward;
        Vector3 rightRayRotation = Quaternion.AngleAxis(halfAngle, transform.up) * forward;
        Vector3 upRayRotation = Quaternion.AngleAxis(-halfAngle, transform.right) * forward;
        Vector3 downRayRotation = Quaternion.AngleAxis(halfAngle, transform.right) * forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_origin, leftRayRotation * range);
        Gizmos.DrawRay(_origin, rightRayRotation * range);
        Gizmos.DrawRay(_origin, upRayRotation * range);
        Gizmos.DrawRay(_origin, downRayRotation * range);

        // 감지된 객체들을 표시
        Gizmos.color = Color.red;
        if (Application.isPlaying && _detectedObjects != null)
        {
            foreach (GameObject obj in _detectedObjects)
            {
                if (obj != null)
                {
                    Gizmos.DrawLine(_origin, obj.transform.position);
                }
            }
        }
    }
}
