using Oculus.Interaction;
using UnityEngine;

public class SingleHandGestureWidgetSelector : MonoBehaviour
{
    [SerializeField] private ThreeFingerAngleExporter _threeFingerAngleExporter;
    [SerializeField] private MWidget _mWidget;
    [Header("Scroll Properties")]
    public float scrollSpeed = 0.1f;
    public float scrollRate = 0.25f;
    private float _scrollTimer = 0.0f;
    private RaycastHit _raycastHit;
    void Start()
    {
    }
    void Update()
    {
        if(_threeFingerAngleExporter)
        {
            if(_scrollTimer >= scrollRate)
            {
                ScrollWithGesture(_threeFingerAngleExporter.GetMiddleAngle());
                _scrollTimer = 0.0f;
            }
            else
            {
                _scrollTimer += Time.deltaTime;
            }
            //Debug.LogWarning(_threeFingerAngleExporter.GetMiddleAngle());
        }
        
    }
    void ScrollWithGesture(float angle)
    {
        if(angle >= 0 && angle <= 50)
        {
            Debug.Log(gameObject.name + "위로 스크롤");
            _mWidget.ScrollUp(scrollSpeed);
        }
        else if (angle > 50 && angle < 130)
        {
            Debug.Log(gameObject.name + "커서 모드");
        }
        else if (angle >= 130 && angle <= 180)
        {
            Debug.Log(gameObject.name + "아래로 스크롤");
            _mWidget.ScrollDown(scrollSpeed);
        }
    }
}
