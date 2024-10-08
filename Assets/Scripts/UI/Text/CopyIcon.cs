using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CopyIcon : MonoBehaviour
{
    public Image iconToCopy;
    private Image thisIcon;
    
    private void Awake()
    {
        thisIcon = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if(thisIcon.sprite != iconToCopy.sprite) 
            thisIcon.sprite = iconToCopy.sprite;
    }
}
