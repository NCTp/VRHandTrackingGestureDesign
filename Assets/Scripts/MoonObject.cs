using System;
using UnityEditor;
using UnityEngine;

public class MoonObject : MonoBehaviour
{
    public enum MoonObjectStatus
    {
        Unselected, // 감지되지고, 선택되지도 않았을 때.
        Detected, // 감지되었지만, 선택되지 않았을 때.
        Selected // 선태되었을 때.
    }
    public Material selMat;
    public Material unSelMat;
    public Material targetMat;
    private MoonObjectStatus _status;
    private Renderer _renderer;

    private bool _isTarget = false;
    private bool _isSelected = false;
    [Header("Outline Properties")]
    private Outline _outline;
    public Color _detectedOutlineColor = Color.yellow;
    public Color _selectedOutlineColor = Color.red;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _outline = GetComponent<Outline>();
        if (_outline != null) _outline.enabled = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _status = MoonObjectStatus.Unselected;
    }

    // Update is called once per frame
    void Update()
    {
        
        switch(_status)
        {
            case MoonObjectStatus.Selected:
                if(_renderer != null)
                {
                    //_renderer.material = selMat;
                    _outline.enabled = true;
                    SetOutlineColor(_selectedOutlineColor);
                }
                break;
            case MoonObjectStatus.Detected:
                if(_renderer != null)
                {
                    //_renderer.material = selMat;
                    _outline.enabled = true;
                    if(_isSelected) SetOutlineColor(_selectedOutlineColor);
                    else SetOutlineColor(_detectedOutlineColor);
                }
                break;
            case MoonObjectStatus.Unselected:
                if(_renderer != null)
                {
                    //_renderer.material = unSelMat;
                    _outline.enabled = false;
                }
                break;
        }
        
        
    }

    public void SetOutline(bool input)
    {
        if(_outline)
        {
            _outline.enabled = input;
        }
    }

    public void SetTargetMat()
    {
        _renderer.material = targetMat;
    }

    void SetOutlineColor(Color newColor)
    {
        _outline.OutlineColor = newColor;
    }

    public void SetStatus(MoonObjectStatus newStatus)
    {
        _status = newStatus;
    }

    public void SetIsSelected(bool input)
    {
        _isSelected = input;
    }
    public bool IsSelected()
    {
        return _isSelected;
    }
    public void SetIsTarget(bool input)
    {
        _isTarget = input;
    }
    public bool IsTarget()
    {
        return _isTarget;
    }
}
