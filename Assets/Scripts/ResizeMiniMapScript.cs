using UnityEngine;

public class ResizeMiniMapScript : MonoBehaviour
{
    [SerializeField] GameObject miniMap;
    [SerializeField] GameObject bigMap;
    [SerializeField] Camera cameraMiniMap;
    private float cameraViewTemp;
    public bool activatedFullScreenMap = false;

    private void Start()
    {
        cameraViewTemp = cameraMiniMap.orthographicSize;
        bigMap.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            activatedFullScreenMap = !activatedFullScreenMap;

            if (activatedFullScreenMap)
            {
                miniMap.SetActive(false);
                bigMap.SetActive(true);
                cameraMiniMap.orthographicSize = 3500;
                return;
            }       
            miniMap.SetActive(true);
            bigMap.SetActive(false);
            cameraMiniMap.orthographicSize = cameraViewTemp;
        }
    }
}
