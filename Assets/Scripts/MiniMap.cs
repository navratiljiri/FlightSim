using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public Transform plane;
    void LateUpdate()
    {
        Vector3 newPosition = plane.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;

        transform.rotation = Quaternion.Euler(90F, plane.eulerAngles.y, 0f);
    }
}
