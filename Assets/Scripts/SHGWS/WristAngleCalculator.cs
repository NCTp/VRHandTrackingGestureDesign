using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;
using UnityEngine;

public class WristAngleCalculator : MonoBehaviour
{
    [Header("Meta Interaction Components")]
    [SerializeField] private RayInteractor _rayInteractor;

    [Header("Hand Settings")]
    [SerializeField, Interface(typeof(IHand))]
    private UnityEngine.Object Hand;
    private IHand _hand;

    [Header("SGFWS")]
    [SerializeField] private SingleHandGestureWidgetSelector _singleHandGestureWidgetSelector;

    // --- 각도 관련 변수 ---
    private float _wristAngle; // 현재 프레임의 손목 각도

    // --- 로직 제어 변수 ---
    private float _initialWristAngle; // Select 시작 시점의 기준 각도
    private bool _isTrackingRotation = false; // 현재 각도 변화를 추적 중인가?
    private const float ROTATION_THRESHOLD = 60.0f; // 반응할 각도 변화량 (60도)

    void Awake()
    {
        _hand = Hand as IHand;
    }
    void Update()
    {
        // 1. 현재 손목 각도 계산 (이전과 동일)
        Vector3 currentPalmNormal = GetPalmNormal();
        _wristAngle = Vector3.Angle(currentPalmNormal, Vector3.up);

        // 2. RayInteractor 상태 확인
        if (_rayInteractor != null && _rayInteractor.State == InteractorState.Select)
        {
            // [상태 A] Select 상태임
            if (!_isTrackingRotation)
            {
                // [진입 시점] 방금 Select가 됨 -> 기준 각도 저장
                _initialWristAngle = _wristAngle;
                _isTrackingRotation = true; 
                Debug.Log($"[Start] 기준 각도 저장됨: {_initialWristAngle:F1}°");
            }
            else
            {
                // [진행 중] 이미 Select 상태임 -> 변화량 체크
                float angleDifference = Mathf.Abs(_wristAngle - _initialWristAngle);

                // 45도 이상 차이가 나면?
                if (angleDifference >= ROTATION_THRESHOLD)
                {
                    OnWristRotatedOverThreshold(); // 특정 함수 호출
                    
                    // (선택 사항) 한 번 발동 후 기능을 끌 것인가?
                    // 끄지 않으면 45도 이상인 상태에서 매 프레임 함수가 호출됨.
                    // 여기서는 한 번 발동 후 추적을 중지(리셋)하도록 설정했습니다.
                    _isTrackingRotation = false; 
                }
            }
        }
        else
        {
            // [상태 B] Select 상태가 아님 (버튼을 뗌) -> 추적 상태 리셋
            if (_isTrackingRotation)
            {
                _isTrackingRotation = false;
                Debug.Log("[End] 추적 종료 (버튼 뗌)");
            }
        }
    }

    // --- 45도 이상 돌아갔을 때 실행될 함수 ---
    private void OnWristRotatedOverThreshold()
    {
        //Debug.LogWarning("!!! 손목이 50도 이상 돌아갔습니다! 기능 실행 !!!");
        if(_singleHandGestureWidgetSelector)
        {
            _singleHandGestureWidgetSelector.TurnWristToFace();
        }
        
        // 여기에 원하는 동작 코드를 넣으세요.
        // 예: UI 메뉴 열기, 무기 교체, 페이지 스크롤 등
    }


    // --- Helper Functions (이전과 동일) ---
    public float GetWristAngle() => _wristAngle;

    public Vector3 GetPalmNormal()
    {
        Pose wristPose = Pose.identity;
        Pose indexPose = Pose.identity;
        Pose pinkyPose = Pose.identity;

        if (_hand == null || !_hand.IsTrackedDataValid) return Vector3.up;

        bool hasData = _hand.GetJointPose(HandJointId.HandWristRoot, out wristPose) &&
                       _hand.GetJointPose(HandJointId.HandIndex1, out indexPose) &&
                       _hand.GetJointPose(HandJointId.HandPinky1, out pinkyPose);

        if (!hasData) return Vector3.up;

        Vector3 wristToIndex = (indexPose.position - wristPose.position).normalized;
        Vector3 wristToPinky = (pinkyPose.position - wristPose.position).normalized;

        if (_hand.Handedness == Handedness.Right)
            return Vector3.Cross(wristToIndex, wristToPinky).normalized;
        else
            return Vector3.Cross(wristToPinky, wristToIndex).normalized;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || _hand == null) return;
        
        // 시각적 디버깅: 추적 중일 때는 빨간색, 아닐 때는 초록색
        Color gizmoColor = _isTrackingRotation ? Color.red : Color.green;
        DrawHandGizmo(_hand, gizmoColor);
    }

    private void DrawHandGizmo(IHand hand, Color color)
    {
        if (hand == null || !hand.IsTrackedDataValid) return;
        hand.GetJointPose(HandJointId.HandWristRoot, out Pose wrist);
        Vector3 palmNormal = GetPalmNormal();
        Gizmos.color = color;
        Gizmos.DrawRay(wrist.position, palmNormal * 0.1f);
        Gizmos.DrawSphere(wrist.position + palmNormal * 0.1f, 0.01f);
    }
}
