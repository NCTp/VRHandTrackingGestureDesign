using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoonItem : MonoBehaviour
{
    public TextMeshProUGUI text;
    public UnityEngine.UI.Outline outline;
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
            Debug.Log("Set Outline!");
        }
        else
        {
            Debug.Log("No Outline!");
        }
    }
}
