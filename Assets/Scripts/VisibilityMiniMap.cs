using System;
using UnityEngine;

public class VisibilityMiniMap : MonoBehaviour
{
    [SerializeField] GameObject miniMap;
    public bool activated = true;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {     
            if (miniMap != null)
            {
                activated = !activated;               
                miniMap.SetActive(activated);
            }
        }
    }
}
