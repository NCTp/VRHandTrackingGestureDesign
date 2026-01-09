using UnityEngine;
using TMPro;

public class CanvasTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    // 텍스트 업데이트 (소수점 없이 정수로 표시: "3", "2", "1")
    public void SetTimerText(float input)
    {
        // "F0": 소수점 없이 반올림, "F1": 소수점 한 자리 (취향에 따라 변경)
        // CeilToInt를 쓰면 2.1초도 3초로 표시되어 카운트다운 느낌이 더 자연스럽습니다.
        timerText.text = Mathf.CeilToInt(input).ToString(); 
    }

    // 타이머 UI 켜기/끄기
    public void SetVisible(bool isVisible)
    {
        if(timerText != null)
            timerText.gameObject.SetActive(isVisible);
    }
}