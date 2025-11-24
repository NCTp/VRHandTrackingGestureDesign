using UnityEngine;
using Oculus.Interaction;

public class MWidget : MonoBehaviour
{
    [SerializeField] private RayInteractor _rayInteractor; // 고정될 위치.
    [Header("Position Properties")]
    public Vector3 offSet = new Vector3(0f,0f,0f); // 고정될 위치로부터의 오프셋
    [Header("Scroll View")]
    public GameObject scrollView;
    public GameObject content;
    public GameObject itemPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
        
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (itemPrefab != null && content != null)
            {
                GameObject newItem = Instantiate(itemPrefab, content.transform);
                newItem.transform.localPosition = Vector3.zero;
                newItem.transform.localRotation = Quaternion.identity;
            }
        }

        if(_rayInteractor)
        {
            switch (_rayInteractor.State)
            {
                case InteractorState.Select:
                    if (scrollView != null)
                    {
                        scrollView.SetActive(true);
                    }
                    this.transform.position = _rayInteractor.Origin + offSet;
                    break;
                case InteractorState.Normal:
                case InteractorState.Disabled:
                default:
                    if (scrollView != null)
                    {
                        scrollView.SetActive(false);
                    }
                    break;
            }
        }
        else
        {
            if (scrollView != null)
            {
                scrollView.SetActive(false);
            }
        }
    }
}
