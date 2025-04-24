using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("Pozice kamery")]
    [SerializeField] private Transform[] povs;
    [Tooltip("Rychlost pohybu kamery")]
    [SerializeField] private float speed = 5f;
    [Tooltip("Rychlost rotace myší")]
    [SerializeField] private float rotationSpeed = 3f;
    [Tooltip("Cíl, kolem kterého se kamera otáèí")]
    [SerializeField] private Transform target;
    [Tooltip("Vzdálenost od cíle")]
    [SerializeField] private float distance = 10f;
    [SerializeField] private PauseMenu pauseMenu;

    private int index = 1;
    private bool isRotating = false;
    private Vector3 previousPosition;
    private float currentX;
    private float currentY;

    void Start()
    {
        // Inicializace úhlù na základì aktuální pozice
        Vector3 offset = transform.position - target.position;
        currentX = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        currentY = Mathf.Asin(offset.y / offset.magnitude) * Mathf.Rad2Deg;
    }

    void Update()
    {
        HandleKeyboardInput();
        HandleMouseRotation();
    }

    private void FixedUpdate()
    {
        // Pohyb kamery mezi pøeddefinovanými body
        transform.position = Vector3.MoveTowards(transform.position, povs[index].position, Time.deltaTime * speed);
        if (!isRotating)
        {
            transform.forward = povs[index].forward;
        }
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) index = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) index = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) index = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) index = 3;
    }

    private void HandleMouseRotation()
    {
        if(!pauseMenu.isPaused)
        {
            if (Input.GetMouseButtonDown(0))
            {
                previousPosition = Input.mousePosition;
                isRotating = true;
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 direction = previousPosition - Input.mousePosition;

                currentX += direction.x * rotationSpeed * 0.1f;
                currentY += direction.y * rotationSpeed * 0.1f;
                currentY = Mathf.Clamp(currentY, 5f, 80f); // Omezení vertikální rotace

                previousPosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isRotating = false;
            }

            if (isRotating)
            {
                UpdateCameraOrbit();
            }
        }
    }

    private void UpdateCameraOrbit()
    {
        // Výpoèet nové pozice kamery kolem cíle
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = new Vector3(0, 0, -distance * 3);
        transform.position = target.position + rotation * direction;
        transform.LookAt(target);
    }
}