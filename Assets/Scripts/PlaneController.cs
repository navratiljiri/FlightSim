using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;
using Klareh;

public class PlainController : MonoBehaviour
{
    [SerializeField] private ZeroController zeroController;
    [SerializeField] private Transform[] wheelTransforms;
    [SerializeField] private Transform plane;
    [Header("Vlasnosti letadla")]
    [Tooltip("Jak moc rychle bude letadlo zrychlovat.")]
    public float throttleIncrement = 0.1f;
    [Tooltip("Jak moc letadlo bude rychle.")]
    public float maxThrottle = 420f;
    [Tooltip("Jak moc bude letadlo citlive.")]
    public float responsiveness = 10f;
    [Tooltip("Kolik sily pot�ebuje letadlo vygenerovat, aby se mohlo vznest.")]
    public float lift = 135f;
    private float enginePower = 0f;
    private float throttle;
    private float roll;
    private float pitch;
    private float yaw;
    private float adjustedThrottle;
    Rigidbody rb;
    private AudioSource engineSound;

    private float responseModifier
    { 
        get
        {
            return ((rb.mass / 10f) * responsiveness);
        }
    }

    private bool IsGrounded()
    {
        foreach (Transform wheel in wheelTransforms)
        {        
            if (Physics.Raycast(wheel.position, -wheel.up, 1f)) 
            {
                   return true;
            }
        }
        return false;
    }
    private bool IsGroundedPlane()
    {
   
            if (Physics.Raycast(plane.position, -plane.up, 5f))
            {
                return true;
            }
        
        return false;
    }

    [SerializeField] TextMeshProUGUI hud;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        engineSound = GetComponent<AudioSource>();
    }
    private void HandleInputs()
    {
        roll = Input.GetAxis("Roll");
        pitch = Input.GetAxis("Pitch");
        yaw = Input.GetAxis("Yaw");
        if (Input.GetKey(KeyCode.X))
        {
            zeroController.Wheels = !zeroController.Wheels;
        }
        if (IsGroundedPlane() && !zeroController.Wheels) {
            throttle -= throttleIncrement;
            throttle = Mathf.Clamp(throttle, 0f, 100f);
            return;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            throttle += throttleIncrement;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            throttle -= throttleIncrement;
        }

        throttle = Mathf.Clamp(throttle, 0f, 100f);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
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
      
        rb.AddTorque(transform.up * yaw * responseModifier * 4);
        rb.AddTorque(transform.right * pitch * responseModifier) ;
        rb.AddTorque(-transform.forward * roll * responseModifier * 4);

        Vector3 thrustDirection = transform.forward;

        // Aktuální rychlost
        float airSpeed = rb.linearVelocity.magnitude;

        if( airSpeed <= 0f )
        {
            if (engineSound.isPlaying)
            {
                engineSound.Stop();
            }
        }

        if (IsGrounded())
        {
            // Na zemi – plný tah podle plynu
            adjustedThrottle = maxThrottle * throttle;

            engineSound.volume = Mathf.Lerp(0.3f, 1f, throttle / 100f);
            engineSound.pitch = Mathf.Lerp(0.8f, 1.5f, throttle / 100f);
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


            // Střemhlavý pád – zesílená gravitace
            float fallFactor = Mathf.Clamp01(Vector3.Dot(rb.linearVelocity.normalized, Vector3.down));
            float gravityBoost = Mathf.Lerp(1f, 4f, fallFactor); // až 4x silnější gravitace při pádu
            Vector3 extraGravity = Physics.gravity * (gravityBoost - 1f);
            rb.AddForce(extraGravity, ForceMode.Acceleration);

            // Odpor vzduchu
            float dragCoefficient = Mathf.Lerp(0.05f, 0.005f, fallFactor); // menší odpor při střemhlavém letu
            Vector3 dragForce = -rb.linearVelocity.normalized * airSpeed * airSpeed * dragCoefficient;
            rb.AddForce(dragForce);

        }
        // Tah motoru
        rb.AddForce(thrustDirection * adjustedThrottle);

        // Vztlak – závislý na rychlosti a orientaci k nebi
        float clampedLift = Mathf.Clamp(airSpeed, 0f, 150f);
        float liftFactor = Mathf.Clamp01(Vector3.Dot(transform.up, Vector3.up));
        rb.AddForce(Vector3.up * clampedLift * lift * liftFactor);

        float autoRollFactor = 2.0f;
        float autoRoll = -yaw * autoRollFactor;
        rb.AddTorque(-transform.forward * autoRoll * responseModifier);

        float sideForceMultiplier = 50f;
        rb.AddForce(transform.right * yaw * sideForceMultiplier);

    }
    private void updateHUD()
    {
        hud.text = "Plyn: " + throttle.ToString("F0") + "%\n";
        hud.text += "Rychlost:" + (rb.linearVelocity.magnitude * 3.6f).ToString("F0") + "km/h\n";
        hud.text += "Vyska: " + transform.position.y.ToString("F0") + " m";
        
    }
}   

