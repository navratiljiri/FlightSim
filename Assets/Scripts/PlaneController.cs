using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;
using Klareh;
using System.Collections;
using System;
using UnityEngine.Audio;

public class PlaneController : MonoBehaviour
{
    [SerializeField] private ZeroController zeroController;
    [SerializeField] private Transform plane;
    [SerializeField] private Camera mainCamera;
    [SerializeField] TextMeshProUGUI hud;
    [SerializeField] private float rotateDuration = 1f;
    [SerializeField] private float pushForce = 100f;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private GameObject deathScreen;
    public GameObject warningUI;
    [Header("Vlasnosti letadla")]
    [Tooltip("Jak moc rychle bude letadlo zrychlovat.")]
    public float throttleIncrement = 0.1f;
    [Tooltip("Jak moc letadlo bude rychle.")]
    public float maxThrottle = 680f;
    [Tooltip("Jak moc bude letadlo citlive.")]
    public float responsiveness = 12f;
    [Tooltip("Kolik sily pot�ebuje letadlo vygenerovat, aby se mohlo vznest.")]
    public float lift = 135f;
    private Coroutine blinkCoroutine;
    private Rigidbody rb;
    private AudioSource engineSound;
    private AudioSource warningSound;
    private AudioSource explosionSound;
    private float throttle;
    private float roll;
    private float pitch;
    private float yaw;
    private float adjustedThrottle;
    private bool isRotating = false;
    private bool deathStatus;
    public bool DeathStatus
    {
        get { return deathStatus; }
    }
    private float responseModifier
    { 
        get
        {
            return ((rb.mass / 10f) * responsiveness);
        }
    }
    private bool IsGroundedPlane()
    {
   
        if (Physics.Raycast(plane.position, -plane.up, 10f) || Physics.Raycast(plane.position, plane.up,10f))
        { 
            return true;
        }
        return false;
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        var sounds = GetComponents<AudioSource>();
        engineSound = sounds[0];
        warningSound = sounds[1];
        explosionSound = sounds[2];
        
    }
    private void HandleInputs()
    {
        if(!pauseMenu.isPaused && !deathStatus)
        {
            roll = Input.GetAxis("Roll");
            pitch = Input.GetAxis("Pitch");
            yaw = Input.GetAxis("Yaw");
            if (Input.GetKeyDown(KeyCode.X))
            {
                zeroController.Wheels = !zeroController.Wheels;
            }
            if (IsGroundedPlane() && !zeroController.Wheels)
            {
                throttle -= throttleIncrement;
                throttle = Mathf.Clamp(throttle, 0f, 100f);
                return;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                throttle += throttleIncrement * 0.4f;
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                throttle -= throttleIncrement * 0.4f;
            }

            throttle = Mathf.Clamp(throttle, 0f, 100f);
        }   
        else
        {
            engineSound.Stop(); 
            warningSound.Stop();
        }
    }
    void Start()
    {
        warningSound.Stop();
        engineSound.Stop();
        explosionSound.Stop();
        warningUI.SetActive(false);  
    }

    void Update()
    {
        HandleInputs();
        updateHUD();
    }
    private void FixedUpdate()
    {
        if (throttle > 0f)
        {
            zeroController.Engine = true;
        }
        else
        {
            zeroController.Engine = false;
        }
      
        rb.AddTorque(transform.up * yaw * responseModifier * 9);
        rb.AddTorque(transform.right * pitch * responseModifier * 5) ;
        rb.AddTorque(-transform.forward * roll * responseModifier * 9);

        Vector3 thrustDirection = transform.forward;

        // Aktuální rychlost
        float airSpeed = rb.linearVelocity.magnitude;

        if(airSpeed <= 0f )
        {          
                if (engineSound.isPlaying)
                {
                    engineSound.Stop();
                }                  
        }

        if (IsGroundedPlane() && zeroController.Wheels)
        {
            adjustedThrottle = maxThrottle * (throttle - 5f);

            engineSound.volume = Mathf.Lerp(0.3f, 1f, throttle / 100f);
            engineSound.pitch = Mathf.Lerp(0.8f, 1.5f, throttle / 100f);


            // Tah motoru
            rb.AddForce(thrustDirection * adjustedThrottle);
        }
        else
        {
            if (!engineSound.isPlaying)
            {
                engineSound.Play();
            }
            // Úhel směru tahu vůči směru nahoru
            float angleToUp = Vector3.Dot(thrustDirection.normalized, Vector3.up);
            float verticalPenalty = Mathf.Clamp01(angleToUp);
            adjustedThrottle = maxThrottle * throttle * (1 - verticalPenalty);

            // Střemhlavý pád
            float fallFactor = Mathf.Clamp01(Vector3.Dot(rb.linearVelocity.normalized, Vector3.down));
            float gravityBoost = Mathf.Lerp(1f, 4f, fallFactor);
            Vector3 extraGravity = Physics.gravity * (gravityBoost - 1f);
            rb.AddForce(extraGravity, ForceMode.Acceleration);

            // Odpor vzduchu
            float dragCoefficient = Mathf.Lerp(0.05f, 0.005f, fallFactor); // menší odpor při střemhlavém letu
            Vector3 dragForce = -rb.linearVelocity.normalized * airSpeed * airSpeed * dragCoefficient;
            rb.AddForce(dragForce);

            // Tah motoru
            rb.AddForce(thrustDirection * adjustedThrottle);
        }
    

        // Vztlak
        float clampedLift = Mathf.Clamp(airSpeed, 0f, 150f);
        float liftFactor = Mathf.Clamp01(Vector3.Dot(transform.up, Vector3.up));
        rb.AddForce(Vector3.up * clampedLift * lift * liftFactor);

        float autoRollFactor = 2.0f;
        float autoRoll = -yaw * autoRollFactor;
        rb.AddTorque(-transform.forward * autoRoll * responseModifier);

        float sideForceMultiplier = 50f;
        rb.AddForce(transform.right * yaw * sideForceMultiplier);

        float farClipPlane = Mathf.Min(3000f + (transform.position.y * 10), 15000f);

        if (farClipPlane >= 3000)
        {
            mainCamera.farClipPlane = farClipPlane;
        }
        else
        {
            mainCamera.farClipPlane = 3000f;
       }

    }
    private void updateHUD()
    {
        float positionY = transform.position.y - 2f;
        hud.text = "Plyn: " + throttle.ToString("F0") + "%\n";
        hud.text += "Rychlost:" + (rb.linearVelocity.magnitude * 3.6f).ToString("F0") + "km/h\n";
        hud.text += "Vyska: " + (positionY > 0 ? positionY : 0).ToString("F0") + " m";
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Earth"))
        {
            engineSound.Stop();
            warningSound.Stop();
            death();
        }
        if (other.CompareTag("RotatePlane"))
        {
            AutoRotateAndPush();
        }
    }

    private void death()
    {
        deathStatus = true;
        StartCoroutine(PlayExplosionAndThenEndScreen());

    }
    IEnumerator PlayExplosionAndThenEndScreen()
    {
        if (explosionEffect != null)
        {
            explosionEffect.Play();      
            throttle = 0;
            turnOffPlane();
            explosionSound.Play();
            yield return new WaitForSeconds(explosionEffect.main.duration);

         
            deathScreen.SetActive(true);
            Time.timeScale = 0;

        }
    }
    private void turnOffPlane()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        mainCamera.transform.parent = null;
        GameObject[] planeModels = GameObject.FindGameObjectsWithTag("planeModel");
        foreach (GameObject model in planeModels)
        {
            model.SetActive(false);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("WarningZone"))
        {
            if (blinkCoroutine == null && !IsLookingAtCenterZone())
            {
                if (!warningSound.isPlaying)
                {
                    warningSound.Play();
                }
                blinkCoroutine = StartCoroutine(BlinkText());
            }
            else if (blinkCoroutine != null && IsLookingAtCenterZone())
            {
                warningSound.Stop();
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
                warningUI.SetActive(false);
            }
        }      
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WarningZone"))
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
            warningUI.SetActive(false);
        }
    }
    private void AutoRotateAndPush()
    {
        if (!isRotating)
        {
            StartCoroutine(SmoothRotateAndPush());
        }
    }

    private bool IsLookingAtCenterZone()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (var hit in hits)
        {      
            if (hit.transform.CompareTag("WarningZone"))
            {
              continue;
          }
     
            if (hit.transform.CompareTag("CenterZone"))
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator SmoothRotateAndPush()
    {
        isRotating = true;

        //Rotace
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, 180, 0)); // otočení o 180° kolem Y

        float elapsed = 0f;

        while (elapsed < rotateDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / rotateDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
   
        Vector3 pushDir = -transform.forward;
        rb.AddForce(pushDir * pushForce, ForceMode.Impulse);

        isRotating = false;
    }

    IEnumerator BlinkText()
    {
        while (true)     
        {     
            warningUI.SetActive(!warningUI.activeSelf);
            yield return new WaitForSeconds(0.5f);
        }
    }
}   

