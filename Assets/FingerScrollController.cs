using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;

public class FingerScrollController : MonoBehaviour
{
    public bool isRightHanded = true;
    [Header("Hand Settings")]
    [SerializeField, Interface(typeof(IHand))]
    private UnityEngine.Object Hand;
    private IHand _hand;

    [SerializeField] private MWidget _mWidget;

    [Header("Settings")]
    [Tooltip("이 각도 이상 손가락을 올리면 스크롤 업 (예: 20도)")]
    [SerializeField] private float _upThreshold = 30.0f;

    [Tooltip("이 각도 이하로 손가락을 내리면 스크롤 다운 (예: -20도)")]
    [SerializeField] private float _downThreshold = -30.0f;

    [Tooltip("다시 동작을 인식하기 위해 돌아와야 하는 중립 범위 (예: 10도)")]
    [SerializeField] private float _resetThreshold = 10.0f;

    // --- 내부 상태 변수 ---
    private enum ScrollState { Neutral, ScrolledUp, ScrolledDown }
    private ScrollState _currentState = ScrollState.Neutral;
    
    private float _currentFingerAngle; // 디버깅용 현재 각도

    void Awake()
    {
        _hand = Hand as IHand;
    }

    void Update()
    {
        if (_hand == null || !_hand.IsTrackedDataValid) return;

        // 1. 세 손가락(중지, 약지, 소지)의 평균 각도 계산
        float middleAngle = GetFingerAngle(HandJointId.HandMiddle1, HandJointId.HandMiddle3);
        float ringAngle = GetFingerAngle(HandJointId.HandRing1, HandJointId.HandRing3);
        float pinkyAngle = GetFingerAngle(HandJointId.HandPinky1, HandJointId.HandPinky3);

        // 세 손가락의 평균 각도
        if(isRightHanded)
        {
            _currentFingerAngle = (middleAngle + ringAngle + pinkyAngle) / 3.0f;
        }
        else if (!isRightHanded)
        {
            _currentFingerAngle = -(middleAngle + ringAngle + pinkyAngle) / 3.0f;
        }

        // 2. 상태 머신을 통한 동작 감지
        HandleScrollLogic(_currentFingerAngle);

        //Debug.LogWarning(gameObject.name + " 현재 손가락 각도는 : " + _currentFingerAngle);
    }

    private void HandleScrollLogic(float angle)
    {
        switch (_currentState)
        {
            case ScrollState.Neutral:
                // 중립 상태일 때만 트리거 체크
                if (angle >= _upThreshold)
                {
                    ScrollUp();
                    _currentState = ScrollState.ScrolledUp; // 상태 변경 (중복 호출 방지)
                }
                else if (angle <= _downThreshold)
                {
                    ScrollDown();
                    _currentState = ScrollState.ScrolledDown; // 상태 변경 (중복 호출 방지)
                }
                break;

            case ScrollState.ScrolledUp:
                // 이미 위로 올린 상태 -> 손가락을 다시 내려서 중립 범위로 와야 리셋
                if (angle < _upThreshold - _resetThreshold) 
                {
                    _currentState = ScrollState.Neutral;
                    Debug.LogWarning("State Reset: Neutral");
                }
                break;

            case ScrollState.ScrolledDown:
                // 이미 아래로 내린 상태 -> 손가락을 다시 펴서 중립 범위로 와야 리셋
                if (angle > _downThreshold + _resetThreshold)
                {
                    _currentState = ScrollState.Neutral;
                    Debug.LogWarning("State Reset: Neutral");
                }
                break;
        }
    }

    // --- 각도 계산 헬퍼 함수 ---
    // 손바닥 평면을 기준으로 손가락이 위(+)로 갔는지 아래(-)로 갔는지 계산
    private float GetFingerAngle(HandJointId knuckleId, HandJointId tipId)
    {
        if (!_hand.GetJointPose(HandJointId.HandWristRoot, out Pose wristPose) ||
            !_hand.GetJointPose(knuckleId, out Pose knucklePose) ||
            !_hand.GetJointPose(tipId, out Pose tipPose))
        {
            return 0.0f;
        }

        // 1. 손목의 기준 방향 계산 (손목 -> 중지 기저부 방향을 Hand Forward로 가정)
        // (단순화를 위해 손목의 로컬 좌표계를 사용합니다)
        Vector3 handForward = wristPose.forward; // Oculus Hand의 forward는 보통 손가락 끝 방향
        Vector3 handRight = wristPose.right;     // 회전축 (오른손 기준, 왼손이면 -right 고려 필요)
        
        // 왼손일 경우 회전축 반전
        if (_hand.Handedness == Handedness.Left) handRight = -handRight;

        // 2. 해당 손가락의 벡터 (기저부 -> 끝)
        Vector3 fingerVector = (tipPose.position - knucklePose.position).normalized;

        // 3. 부호 있는 각도 계산 (Signed Angle)
        // 손목의 Forward 벡터와 손가락 벡터 사이의 각도. 회전축은 손의 옆면(Right).
        // 결과: 손등 쪽으로 젖히면 (+), 손바닥 쪽으로 굽히면 (-)
        float angle = Vector3.SignedAngle(handForward, fingerVector, handRight);

        // Oculus Hand 좌표계 특성상 보정 필요시 여기에 오프셋 추가
        // 보통 쫙 폈을 때 0도가 아닐 수 있으므로, 초기값을 0으로 맞추는 오프셋을 줄 수 있음.
        // 여기서는 상대적 변화가 중요하므로 Raw Angle을 사용.
        
        return angle;
    }

    // --- 실행할 메서드 ---
    private void ScrollUp()
    {
        Debug.LogWarning($"[Action] Scroll Up! (Avg Angle: {_currentFingerAngle:F1})");
        // 여기에 실제 스크롤 올리는 코드 작성
        _mWidget.ScrollUp();
        
    }

    private void ScrollDown()
    {
        Debug.LogWarning($"[Action] Scroll Down! (Avg Angle: {_currentFingerAngle:F1})");
        // 여기에 실제 스크롤 내리는 코드 작성
        _mWidget.ScrollDown();
    }
}