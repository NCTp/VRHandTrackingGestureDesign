using Oculus.Interaction;
using UnityEngine;

public class SingleHandGestureWidgetSelector : MonoBehaviour
{
    [SerializeField] 
    private RayInteractor _rayInteractor; // Ray Interactor 가져오기.
    private RaycastHit _raycastHit;
    void Start()
    {
        this.AssertField(_rayInteractor, nameof(_rayInteractor));
    }
    void Update()
    {
        switch (_rayInteractor.State) // 현재 선택중인지, 아닌지를 판별.
        {
            case InteractorState.Normal:
                //Debug.LogWarning("Released!_Normal");
                break;
            case InteractorState.Select:
                //Debug.LogWarning("Select!_Select");
                if (Physics.Raycast(_rayInteractor.Origin, _rayInteractor.Forward, out _raycastHit))
                {
                    if(_raycastHit.transform.gameObject)
                    {
                        
                    }
                    //Debug.LogWarning( _raycastHit.transform.gameObject.name + " is in " + _raycastHit.point);
                }
                break;
            case InteractorState.Disabled:
                //Debug.LogWarning("Released!_Disabled");
                break;
            default:
                break;
        }
    }
}
