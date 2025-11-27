using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

public class ThreeFingerAngleExporter : MonoBehaviour
{
    // Shape Recognizer 컴포넌트를 Inspector에서 연결합니다.
    [Header("Meta Interaction Components")]
    [SerializeField] private ShapeRecognizer _threeFingerShapeRecognizer;
    [SerializeField] private RayInteractor _leftRayInteractor;
    [SerializeField] private RayInteractor _rightRayInteractor;

    // 해당 손의 OVRHand 컴포넌트를 연결합니다.
    //[SerializeField]
    [SerializeField, Interface(typeof(IHand))]
    private UnityEngine.Object LeftHand;
    [SerializeField, Interface(typeof(IHand))]
    private UnityEngine.Object RightHand;

    private IHand _leftHand;
    private IHand _rightHand;

    void Awake()
    {
        _leftHand = LeftHand as IHand;
        _rightHand = RightHand as IHand;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(_leftRayInteractor.State == InteractorState.Select)
        {
            ReportLeftFingerAngles();
        }
        if(_rightRayInteractor.State == InteractorState.Select)
        {
            ReportRightFingerAngles();
        }
    }
    private void ReportLeftFingerAngles()
    {
        // IHand의 IsTracked 속성을 사용합니다.
        if (_leftHand == null || !_leftHand.IsTrackedDataValid) return;

        // 1. 각 손가락의 방향 벡터를 가져옵니다. (MCP 관절에서 Tip 관절로)
        Vector3 middleVector = GetFingerDirection(HandJointId.HandMiddle3, HandJointId.HandMiddleTip, _leftHand);
        Vector3 ringVector = GetFingerDirection(HandJointId.HandRing3, HandJointId.HandRingTip, _leftHand);
        Vector3 pinkyVector = GetFingerDirection(HandJointId.HandPinky3, HandJointId.HandPinkyTip, _leftHand);

        // 2. 기준 벡터를 월드 좌표계의 위쪽 방향 (Y축)으로 설정합니다.
        Vector3 referenceVector = Vector3.up;

        // 3. 각도를 계산합니다. (Vector3.Angle 함수 사용)
        float middleAngle = Vector3.Angle(middleVector, referenceVector);
        float ringAngle = Vector3.Angle(ringVector, referenceVector);
        float pinkyAngle = Vector3.Angle(pinkyVector, referenceVector);

        // 4. 결과를 출력합니다.
        //string result = $"왼손 중지: {middleAngle:F1}°\n" +
                        //$"왼손 약지: {ringAngle:F1}°\n" +
                        //$"왼손 소지: {pinkyAngle:F1}°";
        string result = "아무 모드도 아님";
        if(middleAngle <= 60.0f && middleAngle > 0.0f)
        {
            result = "왼손 위로 스크롤";
        }
        else if (middleAngle > 60.0f && middleAngle < 120.0f)
        {
            result = "왼손 커서 모드";
        }
        else if (middleAngle >= 120.0f && middleAngle <= 180.0f)
        {
            result = "왼손 아래로 스크롤";
        }
        Debug.Log(result);
    }
    private void ReportRightFingerAngles()
    {
        // IHand의 IsTracked 속성을 사용합니다.
        if (_rightHand == null || !_rightHand.IsTrackedDataValid) return;

        // 1. 각 손가락의 방향 벡터를 가져옵니다. (MCP 관절에서 Tip 관절로)
        Vector3 middleVector = GetFingerDirection(HandJointId.HandMiddle3, HandJointId.HandMiddleTip, _rightHand);
        Vector3 ringVector = GetFingerDirection(HandJointId.HandRing3, HandJointId.HandRingTip, _rightHand);
        Vector3 pinkyVector = GetFingerDirection(HandJointId.HandPinky3, HandJointId.HandPinkyTip, _rightHand);

        // 2. 기준 벡터를 월드 좌표계의 위쪽 방향 (Y축)으로 설정합니다.
        Vector3 referenceVector = Vector3.up;

        // 3. 각도를 계산합니다. (Vector3.Angle 함수 사용)
        float middleAngle = Vector3.Angle(middleVector, referenceVector);
        float ringAngle = Vector3.Angle(ringVector, referenceVector);
        float pinkyAngle = Vector3.Angle(pinkyVector, referenceVector);

        // 4. 결과를 출력합니다.
        //string result = $"오른손 중지: {middleAngle:F1}°\n" +
                        //$"오른손 약지: {ringAngle:F1}°\n" +
                        //$"오른손 소지: {pinkyAngle:F1}°";
        string result = "아무 모드도 아님";
        if(middleAngle <= 60.0f && middleAngle > 0.0f)
        {
            result = "오른손 위로 스크롤";
        }
        else if (middleAngle > 60.0f && middleAngle < 120.0f)
        {
            result = "오른손 커서 모드";
        }
        else if (middleAngle >= 120.0f && middleAngle <= 180.0f)
        {
            result = "오른손 아래로 스크롤";
        }
        Debug.Log(result);
    }

    // IHand의 GetJointPose를 사용하여 두 관절 사이의 방향 벡터를 계산하는 헬퍼 함수
    private Vector3 GetFingerDirection(HandJointId startJointId, HandJointId endJointId, IHand hand)
    {
        Pose startPose, endPose;
        
        // IHand의 GetJointPose()를 사용하여 위치와 회전 정보를 Pose 구조체로 얻습니다.
        if (hand.GetJointPose(startJointId, out startPose) && hand.GetJointPose(endJointId, out endPose))
        {
            // 끝점 위치 - 시작점 위치 = 방향 벡터
            return (endPose.position - startPose.position).normalized;
        }
        
        return Vector3.zero;
    }
}
