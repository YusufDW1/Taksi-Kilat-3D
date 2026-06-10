using UnityEngine;

public class TaxiController : MonoBehaviour
{
    [Header("Referensi Fisika (Wheel Collider)")]
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    [Header("Referensi Visual Ban (Mesh)")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearWheelsCombinedMesh; 

    // ---- TAMBAHKAN BARIS INI (BARU) ----
    [Header("Pusat Gravitasi")]
    [Tooltip("Masukkan objek CenterOfMass ke sini")]
    public Transform centerOfMass; 
    // ------------------------------------

    [Header("Pengaturan Performa")]
    public float motorForce = 1500f;
    public float breakForce = 3000f;
    public float decelerationForce = 400f; // Gaya deselerasi otomatis saat gas dilepas
    public float maxSteerAngle = 30f;

    private float horizontalInput;
    private float verticalInput;
    private bool isBraking;

    // ---- TAMBAHKAN FUNGSI START INI (BARU) ----
    private void Start()
    {
        // Menyuruh Rigidbody menggunakan titik berat yang baru
        if (centerOfMass != null)
        {
            GetComponent<Rigidbody>().centerOfMass = centerOfMass.localPosition;
        }
    }
    // -------------------------------------------

    private void Update()
    {
        GetInput();
    }

    private void FixedUpdate()
    {
        // 2. Terapkan logika mesin dan roda (harus di FixedUpdate)
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        // Deteksi tombol W/S atau Panah Atas/Bawah
        verticalInput = Input.GetAxis("Vertical"); 
        
        // Deteksi tombol A/D atau Panah Kiri/Kanan
        horizontalInput = Input.GetAxis("Horizontal"); 
        
        // Deteksi tombol Spasi
        isBraking = Input.GetKey(KeyCode.Space); 
    }

    private void HandleMotor()
    {
        // Taksi berpenggerak roda depan (lebih stabil untuk game casual)
        if (Mathf.Abs(verticalInput) > 0.05f)
        {
            frontLeftCollider.motorTorque = verticalInput * motorForce;
            frontRightCollider.motorTorque = verticalInput * motorForce;
        }
        else
        {
            frontLeftCollider.motorTorque = 0f;
            frontRightCollider.motorTorque = 0f;
        }

        // Terapkan rem jika spasi ditekan, atau deselerasi otomatis jika gas dilepas
        float currentBreakForce = 0f;
        if (isBraking)
        {
            currentBreakForce = breakForce;
        }
        else if (Mathf.Abs(verticalInput) < 0.05f)
        {
            currentBreakForce = decelerationForce;
        }
        
        ApplyBraking(currentBreakForce);
    }

    private void ApplyBraking(float force)
    {
        // Rem menahan keempat roda
        frontLeftCollider.brakeTorque = force;
        frontRightCollider.brakeTorque = force;
        rearLeftCollider.brakeTorque = force;
        rearRightCollider.brakeTorque = force;
    }

    private void HandleSteering()
    {
        // Roda depan membelok sesuai input horizontal
        float currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftCollider.steerAngle = currentSteerAngle;
        frontRightCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        // Samakan rotasi visual ban depan
        if (frontLeftMesh != null) UpdateSingleWheel(frontLeftCollider, frontLeftMesh);
        if (frontRightMesh != null) UpdateSingleWheel(frontRightCollider, frontRightMesh);
        
        // Samakan rotasi visual ban belakang (yang menyatu)
        if (rearWheelsCombinedMesh != null) UpdateCombinedWheels(rearLeftCollider, rearRightCollider, rearWheelsCombinedMesh);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);

        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    private void UpdateCombinedWheels(WheelCollider leftCol, WheelCollider rightCol, Transform combinedMesh)
    {
        Vector3 posL, posR;
        Quaternion rotL, rotR;

        leftCol.GetWorldPose(out posL, out rotL);
        rightCol.GetWorldPose(out posR, out rotR);

        // Putar ban belakang mengikuti putaran fisika roda kiri
        combinedMesh.rotation = rotL;

        // Posisikan ban belakang tepat di tengah-tengah antara collider kiri dan kanan
        combinedMesh.position = (posL + posR) / 2f;
    }
}