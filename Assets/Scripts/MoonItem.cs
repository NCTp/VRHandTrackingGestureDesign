using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoonItem : MonoBehaviour
{
    public TextMeshProUGUI text;
    public UnityEngine.UI.Outline outline;
    public Image image;
    public Color targetColor;
    public bool isTargetItem = false;
    void Awake()
    {
        //_outline = GetComponent<Outline>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text.text = gameObject.name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetOutline(bool input)
    {
        if(outline)
        {
            outline.enabled = input;
        }
    }
    public void SetTarget()
    {
        image.color = targetColor; 
        isTargetItem = true;
    } 

    public void ButtonOnClick()
    {
        if(isTargetItem)
        {
            MoonObjectSpawner.Instance.RecordTCT(true);
            MoonObjectSpawner.Instance.ReGenerateObjects();
        }
        else
        {
            MoonObjectSpawner.Instance.RecordTCT(false);
            MoonObjectSpawner.Instance.ReGenerateObjects();
        }
    }   
}
