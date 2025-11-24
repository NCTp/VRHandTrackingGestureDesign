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
    private MoonObjectStatus _status;
    private Renderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
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
                    _renderer.material = selMat;
                }
                break;
            case MoonObjectStatus.Unselected:
                if(_renderer != null)
                {
                    _renderer.material = unSelMat;
                }
                break;
        }
    }

    void SetOutline(bool input)
    {
        
    }

    void SetOutlineColor(Color newColor)
    {
        
    }

    public void SetStatus(MoonObjectStatus newStatus)
    {
        _status = newStatus;
    }
}
