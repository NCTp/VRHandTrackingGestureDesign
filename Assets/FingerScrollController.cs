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
    [SerializeField] private float _upThreshold = 60.0f;
    [SerializeField] private float _downThreshold = 20.0f;
    [SerializeField] private float _resetThreshold = 10.0f;

    [Header("Time Settings")]
    [Tooltip("기본 쿨타임 (연타 방지용, 아주 짧게)")]
    public float scrollCooldown = 0.1f;

    [Tooltip("반동 방지 시간 (반대 방향 입력 무시 시간)")]
    public float antiReboundTime = 0.4f; // 0.3 ~ 0.5초 추천

    // --- 내부 상태 변수 ---
    private enum ScrollState { Neutral, ScrolledUp, ScrolledDown }
    private ScrollState _currentState = ScrollState.Neutral;
    
    // 마지막으로 수행한 동작을 기억 (반동 체크용)
    private ScrollState _lastAction = ScrollState.Neutral;

    private float _currentFingerAngle;
    private float _cooldownEndTime = 0.0f; // 기본 쿨타임 끝나는 시간
    private float _reboundBlockTime = 0.0f; // 반동 방지 끝나는 시간

    void Awake()
    {
        _hand = Hand as IHand;
    }

    void Update()
    {
        if (_hand == null || !_hand.IsTrackedDataValid) return;

        float middleAngle = GetFingerAngle(HandJointId.HandMiddle1, HandJointId.HandMiddle3);
        float ringAngle = GetFingerAngle(HandJointId.HandRing1, HandJointId.HandRing3);
        float pinkyAngle = GetFingerAngle(HandJointId.HandPinky1, HandJointId.HandPinky3);

        _currentFingerAngle = (middleAngle + ringAngle + pinkyAngle) / 3.0f;

        HandleScrollLogic(_currentFingerAngle);

        if(Input.GetKeyDown(KeyCode.Space)) Debug.Log("Now 각도 is : " + _currentFingerAngle);
    }

    private void HandleScrollLogic(float angle)
    {
        switch (_currentState)
        {
            case ScrollState.Neutral:
                // 1. 기본 쿨타임 체크 (모든 입력 차단)
                if (Time.time < _cooldownEndTime) return;

                // 2. 스크롤 업 체크
                if (angle >= _upThreshold)
                {
                    // 반동 방지: 방금 '다운'을 했고, 아직 반동 차단 시간 내라면 '업' 무시
                    if (_lastAction == ScrollState.ScrolledDown && Time.time < _reboundBlockTime)
                    {
                        return;
                    }

                    ScrollUp();
                    _currentState = ScrollState.ScrolledUp;
                    _lastAction = ScrollState.ScrolledUp; // 마지막 동작 기록
                }
                // 3. 스크롤 다운 체크
                else if (angle <= _downThreshold)
                {
                    // 반동 방지: 방금 '업'을 했고, 아직 반동 차단 시간 내라면 '다운' 무시
                    // (가장 흔한 케이스: 손가락 튕긴 후 반동으로 내려가는 것 방지)
                    if (_lastAction == ScrollState.ScrolledUp && Time.time < _reboundBlockTime)
                    {
                         // 디버깅용: 반동으로 인식되어 무시됨을 확인하고 싶다면 주석 해제
                         // Debug.Log("Rebound Ignored (Anti-Rebound Active)");
                        return;
                    }

                    ScrollDown();
                    _currentState = ScrollState.ScrolledDown;
                    _lastAction = ScrollState.ScrolledDown; // 마지막 동작 기록
                }
                break;

            case ScrollState.ScrolledUp:
                if (angle < _upThreshold - _resetThreshold)
                {
                    EnterNeutralState();
                }
                break;

            case ScrollState.ScrolledDown:
                if (angle > _downThreshold + _resetThreshold)
                {
                    EnterNeutralState();
                }
                break;
        }
    }

    private void EnterNeutralState()
    {
        _currentState = ScrollState.Neutral;
        
        // 중립 복귀 시점부터 타이머 시작
        float now = Time.time;
        _cooldownEndTime = now + scrollCooldown;     // 아주 짧은 전체 쿨타임
        _reboundBlockTime = now + antiReboundTime;   // 반대 방향 입력을 막는 긴 쿨타임
    }

    // --- 각도 계산 함수 (기존과 동일) ---
    private float GetFingerAngle(HandJointId knuckleId, HandJointId tipId)
    {
        if (!_hand.GetJointPose(HandJointId.HandWristRoot, out Pose wristPose) ||
            !_hand.GetJointPose(knuckleId, out Pose knucklePose) ||
            !_hand.GetJointPose(tipId, out Pose tipPose)) return 0.0f;

        Vector3 handRight = wristPose.right;

        Vector3 fingerVector = (tipPose.position - knucklePose.position).normalized;
        return Vector3.SignedAngle(wristPose.forward, fingerVector, handRight);
    }

    private void ScrollUp()
    {
        Debug.LogWarning($"[Action] Scroll Up!");
        if (_mWidget != null) _mWidget.ScrollUp();
    }

    private void ScrollDown()
    {
        Debug.LogWarning($"[Action] Scroll Down!");
        if (_mWidget != null) _mWidget.ScrollDown();
    }
}