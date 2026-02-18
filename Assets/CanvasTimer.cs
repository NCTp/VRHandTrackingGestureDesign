using UnityEngine;
using TMPro;

public class CanvasTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;

// 텍스트 업데이트
    public void SetTimerText(float input)
    {
        // Mathf.CeilToInt: 2.1초 -> 3초로 올림 처리 (카운트다운에 적합)
        int count = Mathf.CeilToInt(input);

        // 숫자 + 줄바꿈(\n) + 안내 문구
        // $"" (문자열 보간) 기능을 사용하면 깔끔하게 합칠 수 있습니다.
        timerText.text = $"{count}\n<size=50%>선이 빨간색이 되도록 \n 손 제스처를 해제해주세요.</size>";
    }

    // 타이머 UI 켜기/끄기
    public void SetVisible(bool isVisible)
    {
        if(timerText != null)
            timerText.gameObject.SetActive(isVisible);
    }
}