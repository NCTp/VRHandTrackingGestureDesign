using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class MWidget : MonoBehaviour
{
    [SerializeField] private RayInteractor _rayInteractor; // 고정될 위치.
    [SerializeField] private ConeDetector _coneDetector; // 고정될 위치.
    [Header("Position Properties")]
    public Vector3 offSet = new Vector3(0f,0f,0f); // 고정될 위치로부터의 오프셋
    [Header("Scroll View")]
    public GameObject scrollView;
    public GameObject content;
    public GameObject itemPrefab;
    [SerializeField] private Scrollbar scrollbar;

    private List<GameObject> _items = new List<GameObject>(); // 목록에 들어가는 게임오브젝트
    private GameObject _selectedItem;
    private bool _isItemListGenerated = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }



    // Update is called once per frame
    void Update()
    {
        //Debug.Log("_item count : " + _items.Count);
        if(_rayInteractor)
        {
            switch (_rayInteractor.State)
            {
                case InteractorState.Select:
                    GenerateItemList();
                    if (scrollView != null)
                    {
                        scrollView.SetActive(true);
                    }
                    if(scrollbar != null)
                    {
                        if(Input.GetKeyDown(KeyCode.K))
                        {
                            ScrollUp(0.1f);
                        }
                        else if (Input.GetKeyDown(KeyCode.L))
                        {
                            ScrollDown(0.1f);
                        }
                    }
                    this.transform.position = _rayInteractor.Origin + offSet;
                    break;
                case InteractorState.Normal:
                    if (scrollView != null)
                    {
                        scrollView.SetActive(false);
                    }
                    ClearItemList();
                    break;
                case InteractorState.Disabled:
                default:
                    if (scrollView != null)
                    {
                        scrollView.SetActive(false);
                    }
                    ClearItemList();
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
    // 위젯에 띄울 아이템 리스트를 생성
    void GenerateItemList()
    {
        if(_rayInteractor)
        {
            _items = _coneDetector.detectedTargets;
            //if(_items.Count == _coneDetector.detectedTargets.Count) _isItemListGenerated = true;
            if(!_isItemListGenerated)
            {
                for(int i = 0; i < _items.Count; i++)
                {
                    GameObject newItem = Instantiate(itemPrefab, content.transform);
                    newItem.transform.localPosition = Vector3.zero;
                    newItem.transform.localRotation = Quaternion.identity;
                }
            }
            if(_items.Count == _coneDetector.detectedTargets.Count) _isItemListGenerated = true;
        }
    }

    // 위젯에 띄운 오브젝트를 모두 제거하고, 리스트를 초기화.
    void ClearItemList()
    {
        int childCount = content.transform.childCount;
        GameObject[] childrenToDestroy = new GameObject[childCount];

        for(int i = 0; i < childCount; i++)
        {
            childrenToDestroy[i] = content.transform.GetChild(i).gameObject;
        }

        foreach (GameObject child in childrenToDestroy)
        {
            if(Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
        _items.Clear();
        _isItemListGenerated = false;
    }

    public void ScrollUp(float upValue)
    {
        scrollbar.value += upValue;
    }
    public void ScrollDown(float downValue)
    {
        scrollbar.value -= downValue;
    }

}
