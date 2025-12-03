using System;
using UnityEditor;
using UnityEngine;

public class MoonObject : MonoBehaviour
{
    public enum MoonObjectStatus
    {
        Unselected,
        Selected
    }
    public Material selMat;
    public Material unSelMat;
    public Material targetMat;
    private MoonObjectStatus _status;
    private Renderer _renderer;
    private Outline _outline;

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
        
    }

    public void SetStatus(MoonObjectStatus newStatus)
    {
        _status = newStatus;
    }
}
