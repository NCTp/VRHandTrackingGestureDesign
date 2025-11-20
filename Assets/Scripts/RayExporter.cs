using Oculus.Interaction;
using UnityEngine;


public class RayExporter : MonoBehaviour
{
    [SerializeField] 
    private RayInteractor _rayInteractor;
    private Ray _ray;
    private RaycastHit _raycastHit;
    void Start()
    {
        this.AssertField(_rayInteractor, nameof(_rayInteractor));
    }
    void Update()
    {
        //Debug.LogError(_rayInteractor.Origin);
        if (Physics.Raycast(_rayInteractor.Origin, _rayInteractor.Forward, out _raycastHit))
        {
            Debug.LogWarning( _raycastHit.transform.gameObject.name + " is in " + _raycastHit.point);
        }
    
    }
}