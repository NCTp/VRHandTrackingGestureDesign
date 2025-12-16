using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;
using System.Linq;

public class RayDetector : MonoBehaviour
{
    [Header("Ray Interactor")]
    [SerializeField] private RayInteractor _rayInteractor;

    [Header("감지 설정")]
    public float detectionRange = 50.0f; 
    public LayerMask targetMask; 

    [Header("감지 결과")]
    [SerializeField] private GameObject _detectedTarget = null;
    
    // [추가] 이전 프레임에 감지되었던 MoonObject를 기억할 변수
    private MoonObject _prevMoonObject = null; 

    private Vector3 _rayOrigin;
    private Vector3 _rayForward;

    public void DetectTargetsWithRay()
    {
        _rayOrigin = _rayInteractor.Origin;
        _rayForward = _rayInteractor.Forward;

        RaycastHit hit;
        if (Physics.Raycast(_rayOrigin, _rayForward, out hit, detectionRange, targetMask))
        {
            GameObject currentTarget = hit.collider.gameObject;

            // [변경] 감지된 대상이 바뀌었는지 확인
            if (_detectedTarget != currentTarget)
            {
                // 1. 기존에 감지되고 있던 객체가 있다면 상태 초기화 (Exit 로직)
                if (_prevMoonObject != null)
                {
                    // Unselected 또는 Normal 등 기본 상태로 되돌리는 enum 값을 사용하세요
                    _prevMoonObject.SetStatus(MoonObject.MoonObjectStatus.Unselected); 
                }

                _detectedTarget = currentTarget;
                
                // 2. 새로 감지된 객체 상태 변경 (Enter 로직)
                MoonObject currentMoonObject = _detectedTarget.GetComponent<MoonObject>();
                if (currentMoonObject)
                {
                    currentMoonObject.SetStatus(MoonObject.MoonObjectStatus.Detected);
                    _prevMoonObject = currentMoonObject; // 현재 객체를 '이전 객체'로 기억
                }
                else
                {
                    // MoonObject 컴포넌트가 없는 물체라면 _prev는 비워둠
                    _prevMoonObject = null;
                }
                
                //Debug.Log("Enter: " + _detectedTarget.name);
            }
            
            // (같은 오브젝트를 계속 가리키고 있을 때는 아무것도 하지 않음 -> 성능 최적화)

            Debug.DrawRay(_rayOrigin, _rayForward * hit.distance, Color.green);
        }
        else
        {
            // [변경] Ray가 아무것도 감지하지 못했을 때 (Exit 로직)
            if (_prevMoonObject != null)
            {
                // 기존 객체 상태 초기화
                _prevMoonObject.SetStatus(MoonObject.MoonObjectStatus.Unselected);
                _prevMoonObject = null; // 기억 초기화
                
                //Debug.Log("Exit");
            }

            _detectedTarget = null;
            Debug.DrawRay(_rayOrigin, _rayForward * detectionRange, Color.red);
        }
    }

    // ... (Start 함수 생략)

    void Update()
    {
        // [안전 장치 추가] RayInteractor 자체가 꺼지거나 손이 사라질 때를 대비
        if (!_rayInteractor) return;

        switch(_rayInteractor.State)
        {
            case InteractorState.Normal:
            case InteractorState.Hover:
                DetectTargetsWithRay();
                break;

            case InteractorState.Select:
                // 선택 상태 로직 유지
                if(_detectedTarget)
                {
                    MoonObject moonObject = _detectedTarget.GetComponent<MoonObject>();
                    // null 체크 추가 (안전성 강화)
                    if(moonObject && moonObject.IsTarget()) 
                    {
                        MoonObjectSpawner.Instance.RecordTCT(true);
                        MoonObjectSpawner.Instance.ReGenerateObjects();
                        // 선택 후 로직에 따라 _prevMoonObject를 초기화할지 결정 필요
                        // 보통 재생성되면 기존 참조는 의미가 없어지므로 초기화 추천:
                        _prevMoonObject = null; 
                        _detectedTarget = null;
                    }
                    else
                    {
                        MoonObjectSpawner.Instance.RecordTCT(false);
                        MoonObjectSpawner.Instance.ReGenerateObjects();
                        // 선택 후 로직에 따라 _prevMoonObject를 초기화할지 결정 필요
                        // 보통 재생성되면 기존 참조는 의미가 없어지므로 초기화 추천:
                        _prevMoonObject = null; 
                        _detectedTarget = null;
                    }
                }
                break;
            
            // [추가] Disabled나 다른 상태로 갔을 때 하이라이트가 남는 것을 방지
            default:
                if (_prevMoonObject != null)
                {
                    _prevMoonObject.SetStatus(MoonObject.MoonObjectStatus.Unselected);
                    _prevMoonObject = null;
                    _detectedTarget = null;
                }
                break;
        }
    }
}