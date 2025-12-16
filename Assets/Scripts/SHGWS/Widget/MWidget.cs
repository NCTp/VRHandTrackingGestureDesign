using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine.UI;

public class MWidget : MonoBehaviour
{
    [SerializeField] private RayInteractor _rayInteractor;
    [SerializeField] private ConeDetector _coneDetector;
    
    [Header("Position Properties")]
    public Vector3 offSet = new Vector3(0f, 0f, 0f);

    [Header("Scroll View")]
    public GameObject scrollView;
    [SerializeField] private ScrollRect scrollRect;
    public GameObject content;
    public GameObject itemPrefab;


    private List<GameObject> _items = new List<GameObject>();
    private GameObject _selectedItem;
    private int _selectedItemIdx = 0;
    private bool _isItemListGenerated = false;

    void Update()
    {
        if (!_rayInteractor) return;

        switch (_rayInteractor.State)
        {
            case InteractorState.Select:
                if (!_isItemListGenerated) GenerateItemList();
                
                if (scrollView != null) 
                {
                    scrollView.SetActive(true);
                }
                
                this.transform.position = _rayInteractor.Origin + offSet;

                // 선택된 아이템이 유효한지 지속적으로 체크 (선택 중 대상이 사라질 수 있음)
                if (_selectedItem) SetItemOutline(_selectedItem, true);
                break;

            case InteractorState.Normal:
            case InteractorState.Disabled:
            default:
                if (scrollView != null) 
                {
                    scrollView.SetActive(false);
                    if(scrollRect) scrollRect.verticalNormalizedPosition = 1.0f;
                }
                ClearItemList();
                break;
        }
    }

    void GenerateItemList()
    {
        if (!_rayInteractor || _coneDetector == null) return;

        if (!_isItemListGenerated)
        {
            bool isTargetFounded = false;

            // [방어 코드] 감지된 타겟이 없으면 리턴
            if (_coneDetector.detectedTargets == null || _coneDetector.detectedTargets.Count == 0) return;

            for (int i = 0; i < _coneDetector.detectedTargets.Count; i++)
            {
                // [방어 코드] 타겟이 null인 경우 건너뜀 (이미 파괴된 객체 등)
                if (_coneDetector.detectedTargets[i] == null) continue;

                GameObject newItem = Instantiate(itemPrefab, content.transform);
                newItem.transform.localPosition = Vector3.zero;
                newItem.transform.localRotation = Quaternion.identity;
                newItem.name = _coneDetector.detectedTargets[i].name;
                _items.Add(newItem);

                if(isTargetFounded == false &&_coneDetector.detectedTargets[i].GetComponent<MoonObject>())
                {
                    MoonObject moonObject = _coneDetector.detectedTargets[i].GetComponent<MoonObject>();
                    if(moonObject.IsTarget())
                    {
                        if(newItem.GetComponent<MoonItem>())
                        {
                            MoonItem moonItem = newItem.GetComponent<MoonItem>();
                            moonItem.SetTarget();
                            isTargetFounded = true;
                        }
                    }
                }
            }

            _isItemListGenerated = true;
            _selectedItemIdx = 0; // 리스트 생성 시 인덱스 초기화

            if (_items.Count > 0)
            {
                _selectedItem = _items[0];
                
                // 첫 번째 아이템 선택 처리
                MoonObject moonObject = GetSafeMoonObject(0);
                if (moonObject) moonObject.SetStatus(MoonObject.MoonObjectStatus.Selected);
            }
        }
    }

    void ClearItemList()
    {
        // 기존 선택 상태 해제
        MoonObject moonObject = GetSafeMoonObject(_selectedItemIdx);
        if (moonObject) moonObject.SetStatus(MoonObject.MoonObjectStatus.Unselected);

        // UI 아이템 제거
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }

        _items.Clear();
        _selectedItemIdx = 0;
        _selectedItem = null;
        _isItemListGenerated = false;
    }

    // [핵심 개선] 안전하게 MoonObject를 가져오는 헬퍼 메서드
    private MoonObject GetSafeMoonObject(int index)
    {
        // 1. Detector 유효성 검사
        if (_coneDetector == null || _coneDetector.detectedTargets == null) return null;

        // 2. 인덱스 범위 검사
        if (index < 0 || index >= _coneDetector.detectedTargets.Count) return null;

        // 3. GameObject 존재 여부 검사
        GameObject target = _coneDetector.detectedTargets[index];
        if (target == null) return null;

        // 4. 컴포넌트 반환
        return target.GetComponent<MoonObject>();
    }

    public void ScrollUp()
    {
        if (_items.Count <= 0) return;

        // 이전 선택 해제
        MoonObject prevObj = GetSafeMoonObject(_selectedItemIdx);
        if (prevObj) prevObj.SetStatus(MoonObject.MoonObjectStatus.Detected);
        SetItemOutline(_selectedItem, false);

        // 인덱스 변경 및 클램핑(범위 제한)
        _selectedItemIdx++;
        if (_selectedItemIdx >= _items.Count) _selectedItemIdx = _items.Count - 1;

        // 새 선택 적용
        _selectedItem = _items[_selectedItemIdx];
        SetItemOutline(_selectedItem, true);
        
        MoonObject nextObj = GetSafeMoonObject(_selectedItemIdx);
        if (nextObj) nextObj.SetStatus(MoonObject.MoonObjectStatus.Selected);
    }

    public void ScrollDown()
    {
        if (_items.Count <= 0) return;

        // 이전 선택 해제
        MoonObject prevObj = GetSafeMoonObject(_selectedItemIdx);
        if (prevObj) prevObj.SetStatus(MoonObject.MoonObjectStatus.Detected);
        SetItemOutline(_selectedItem, false);

        // 인덱스 변경 및 클램핑
        _selectedItemIdx--;
        if (_selectedItemIdx < 0) _selectedItemIdx = 0;

        // 새 선택 적용
        _selectedItem = _items[_selectedItemIdx];
        SetItemOutline(_selectedItem, true);

        MoonObject nextObj = GetSafeMoonObject(_selectedItemIdx);
        if (nextObj) nextObj.SetStatus(MoonObject.MoonObjectStatus.Selected);
    }

    public void SelectItem()
    {
        //Debug.Log("Select Item!");
        if (_selectedItem)
        {
            // 인덱스 기반으로 안전하게 객체 가져오기
            MoonObject moonObject = GetSafeMoonObject(_selectedItemIdx);
            
            if (moonObject)
            {
                if (moonObject.IsTarget())
                {
                    //Debug.LogWarning(moonObject.gameObject.name + " is Selected, Correct Item!");
                    ClearItemList();
                    MoonObjectSpawner.Instance.RecordTCT(true);
                    MoonObjectSpawner.Instance.ReGenerateObjects();
                }
                else
                {
                    //Debug.LogWarning(moonObject.gameObject.name + " is Selected, Wrong Item!");
                    ClearItemList();
                    MoonObjectSpawner.Instance.RecordTCT(false);
                    MoonObjectSpawner.Instance.ReGenerateObjects();
                }
            }
            else
            {
                Debug.LogWarning("Target object lost or invalid!");
                // 상황에 따라 UI를 리프레시하거나 닫는 로직 추가 가능
                scrollView.SetActive(false);
            }
        }
    }

    void SetItemOutline(GameObject item, bool input)
    {
        if (item)
        {
            MoonItem moonItem = item.GetComponent<MoonItem>();
            if (moonItem) moonItem.SetOutline(input);
        }
    }
}