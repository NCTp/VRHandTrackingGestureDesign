using Oculus.Interaction;
using UnityEngine;

public class SingleHandGestureWidgetSelector : MonoBehaviour
{
    [SerializeField] private ThreeFingerAngleExporter _threeFingerAngleExporter;
    [SerializeField] private MWidget _mWidget;
    private RaycastHit _raycastHit;
    void Start()
    {
    }
    void Update()
    {
        
    }
    public void ThreeFingerUp()
    {
        _mWidget.ScrollUp();
    }
    public void ThreeFingerDown()
    {
        _mWidget.ScrollDown();
    }
}
