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

    [Header("Pusat Gravitasi")]
    [Tooltip("Masukkan objek CenterOfMass ke sini")]
    public Transform centerOfMass; 

    [Header("Pengaturan Performa")]
    public float motorForce = 1500f;
    public float breakForce = 3000f;
    public float decelerationForce = 400f; 
    public float maxSteerAngle = 30f;
 
    [Header("Pengaturan Setir Advanced")]
    public float steeringSpeed = 150f;  // Kecepatan putar roda (derajat/detik) agar tidak kaku
    
    [Header("Downforce & Stabilitas")]
    public float downforceValue = 100f; // Gaya rekat ke jalan agar mobil tidak melayang saat kencang

    [Header("Kontrol Mobile Android")]
    public MobileButton tombolGas;
    public MobileButton tombolMundur;
    public MobileButton tombolKiri;
    public MobileButton tombolKanan;
    public MobileButton tombolRemTangan;

    // ---- BAGIAN BARU UNTUK AUDIO INTERNAL (MESIN) ----
    [Header("Pengaturan Audio Internal Mobil")]
    public AudioSource mesinAudioSource; // Tarik Audio Source Mesin dari Taksi ke Sini
    public AudioSource sfxDecitBan;      // Tarik Audio Source khusus decit ban ke sini
    public float pitchMinimal = 0.8f;    // Nada mesin pas berhenti
    public float pitchMaksimal = 2.2f;   // Nada mesin pas ngebut maksimal
    
    [Tooltip("Detik mulai loop (skip intro/hening di awal audio)")]
    public float loopStartSeconds = 0.15f; 
    [Tooltip("Detik selesai loop (skip outro/hening di akhir audio). Set 0 untuk otomatis.")]
    public float loopEndSeconds = 0f;
    // --------------------------------------------------

    private float horizontalInput;
    private float verticalInput;
    private bool isBraking;
    private Rigidbody rb;
    private bool wasDrifting; // Melacak apakah mobil sedang berdecit di frame sebelumnya
    private float currentSteerAngle = 0f; // Sudut setir saat ini untuk smoothing
    private float smoothSpeedRatio = 0f;  // Untuk menghaluskan perubahan pitch mesin
    private float engineMaxVolume = 1f;   // Menyimpan volume maksimal bawaan mesin untuk micro-fade

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Menyuruh Rigidbody menggunakan titik berat yang baru
        if (centerOfMass != null)
        {
            rb.centerOfMass = centerOfMass.localPosition;
        }

        // Pastikan suara mesin diatur agar me-looping dan langsung menyala sejak awal
        if (mesinAudioSource != null)
        {
            engineMaxVolume = mesinAudioSource.volume; // Simpan volume bawaan awal
            mesinAudioSource.loop = true;

            // Jika batas akhir loop belum ditentukan, potong 0.15 detik sebelum durasi total habis
            if (mesinAudioSource.clip != null && loopEndSeconds <= 0f)
            {
                loopEndSeconds = mesinAudioSource.clip.length - 0.15f;
            }

            if (!mesinAudioSource.isPlaying)
            {
                mesinAudioSource.Play();
                // Set waktu awal langsung ke loopStartSeconds untuk melewati keheningan awal secara instan (sekali saja)
                if (mesinAudioSource.clip != null)
                {
                    mesinAudioSource.time = Mathf.Min(loopStartSeconds, mesinAudioSource.clip.length - 0.1f);
                }
            }
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            // Jika game sedang beku/pause, senyapkan/pause audio mobil
            if (mesinAudioSource != null && mesinAudioSource.isPlaying)
            {
                mesinAudioSource.Pause();
            }
            if (sfxDecitBan != null && sfxDecitBan.isPlaying)
            {
                sfxDecitBan.Pause();
            }
            return;
        }
        else
        {
            // Jika game berjalan kembali, mainkan lagi jika sebelumnya di-pause
            if (mesinAudioSource != null && !mesinAudioSource.isPlaying)
            {
                mesinAudioSource.UnPause();
            }
        }

        GetInput();
        HandleEngineSound(); // Jalankan kontrol suara mesin setiap frame
        HandleDriftSound();  // Jalankan kontrol cek decit ban setiap frame
        HandleEngineLoopRange(); // Kontrol area looping kustom suara mesin
    }

    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        AddDownforce(); // Berikan downforce agar mobil stabil menempel di jalan
    }

    private void GetInput()
    {
        verticalInput = Input.GetAxis("Vertical"); 
        horizontalInput = Input.GetAxis("Horizontal"); 
        isBraking = Input.GetKey(KeyCode.Space); 

        // Integrasi kontrol tombol Android (Keyboard PC tetap aktif)
        if (tombolGas != null && tombolGas.isPressed)
        {
            verticalInput = 1f;
        }
        else if (tombolMundur != null && tombolMundur.isPressed)
        {
            verticalInput = -1f;
        }

        if (tombolKiri != null && tombolKiri.isPressed)
        {
            horizontalInput = -1f;
        }
        else if (tombolKanan != null && tombolKanan.isPressed)
        {
            horizontalInput = 1f;
        }

        if (tombolRemTangan != null && tombolRemTangan.isPressed)
        {
            isBraking = true;
        }
    }

    // ==================================================
    // FUNGSI BARU: MENGATUR DINAMIKA SUARA MESIN
    // ==================================================
    private void HandleEngineSound()
    {
        if (mesinAudioSource == null || rb == null) return;

        // Hitung kecepatan mobil saat ini (diubah ke magnitudo positif)
        float kecepatanMobil = rb.linearVelocity.magnitude;

        // Iseng batas kecepatan maksimal untuk perkiraan pitch (misal top speed 30 m/s)
        float targetSpeedRatio = kecepatanMobil / 30f; 

        // Haluskan perubahan rasio kecepatan menggunakan Lerp agar suara deruman tidak bergetar/patah-patah
        smoothSpeedRatio = Mathf.Lerp(smoothSpeedRatio, targetSpeedRatio, Time.deltaTime * 6f);

        // Atur pitch suara mesin secara halus berdasarkan laju mobil saat ini
        mesinAudioSource.pitch = Mathf.Lerp(pitchMinimal, pitchMaksimal, smoothSpeedRatio);
    }

    // ==================================================
    // FUNGSI BARU: MEMICU SUARA DECIT BAN (SCREECH)
    // ==================================================
    private void HandleDriftSound()
    {
        if (sfxDecitBan == null || sfxDecitBan.clip == null) return; 

        float kecepatanMobil = rb.linearVelocity.magnitude;
        
        // Cek kecepatan pergeseran samping (lateral speed)
        float sidewaysSpeed = Vector3.Dot(rb.linearVelocity, transform.right);
        
        // Kondisi Ban Mendecit Realistis:
        // 1. Mobil benar-benar tergelincir ke samping (sideways speed > 3.5 m/s) DAN mobil sedang melaju
        bool isSlipping = Mathf.Abs(sidewaysSpeed) > 3.5f && kecepatanMobil > 2f;
        
        // 2. Rem tangan (Spasi) aktif DAN mobil masih bergerak maju/mundur dengan kecepatan minimal
        bool isHandbraking = isBraking && kecepatanMobil > 2f;

        bool isDrifting = isSlipping || isHandbraking;

        if (isDrifting)
        {
            if (!wasDrifting)
            {
                // Putar suara secara terkontrol (tidak me-loop otomatis)
                sfxDecitBan.loop = false;
                sfxDecitBan.Play();
                wasDrifting = true;
            }
        }
        else
        {
            if (wasDrifting)
            {
                // Matikan suara secara instan begitu tergelincir selesai / tombol dilepas
                sfxDecitBan.Stop();
                wasDrifting = false;
            }
        }
    }

    private void HandleMotor()
    {
        // Hitung kecepatan mobil saat ini
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float motorTorqueToApply = 0f;
        
        float frontBrake = 0f;
        float rearBrake = 0f;

        // 1. Rem Tangan (Spasi)
        if (isBraking)
        {
            // Rem roda belakang dikunci kuat agar bagian belakang slide (drift)
            // Namun roda depan tetap direm 80% agar mobil bisa berhenti mendadak dengan sangat cepat
            rearBrake = breakForce * 1.5f;
            frontBrake = breakForce * 0.8f;
        }
        else
        {
            // 2. Logika Gas & Rem Kaki Realistis (W/S)
            if (verticalInput > 0.05f) // Menekan tombol W (Maju)
            {
                if (forwardSpeed < -0.5f) // Mobil sedang mundur -> REM terlebih dahulu
                {
                    frontBrake = breakForce;
                    rearBrake = breakForce;
                }
                else // Mobil diam atau sedang maju -> GAS
                {
                    motorTorqueToApply = verticalInput * motorForce;
                }
            }
            else if (verticalInput < -0.05f) // Menekan tombol S (Mundur)
            {
                if (forwardSpeed > 0.5f) // Mobil sedang melaju maju -> REM terlebih dahulu
                {
                    frontBrake = breakForce;
                    rearBrake = breakForce;
                }
                else // Mobil diam atau sedang mundur -> GAS MUNDUR
                {
                    motorTorqueToApply = verticalInput * motorForce;
                }
            }
            else // Gas dilepas -> Engine braking (Deselerasi lambat)
            {
                frontBrake = decelerationForce;
                rearBrake = decelerationForce;
            }
        }

        // Terapkan torsi motor ke roda depan (FWD)
        frontLeftCollider.motorTorque = motorTorqueToApply;
        frontRightCollider.motorTorque = motorTorqueToApply;

        // Terapkan rem yang disesuaikan
        ApplyBraking(frontBrake, rearBrake);
    }

    private void ApplyBraking(float frontForce, float rearForce)
    {
        frontLeftCollider.brakeTorque = frontForce;
        frontRightCollider.brakeTorque = frontForce;
        rearLeftCollider.brakeTorque = rearForce;
        rearRightCollider.brakeTorque = rearForce;
    }

    private void HandleSteering()
    {
        float speed = rb.linearVelocity.magnitude;
        
        // SENSITIVE STEERING: Kurangi sudut belok maksimal di kecepatan tinggi agar tidak slip/terbalik
        // Asumsi kecepatan tinggi sekitar 25 m/s (~90 km/jam)
        float speedFactor = Mathf.Clamp01(speed / 25f);
        float dynamicMaxSteerAngle = Mathf.Lerp(maxSteerAngle, maxSteerAngle * 0.35f, speedFactor);

        float targetSteerAngle = dynamicMaxSteerAngle * horizontalInput;
        
        // SMOOTHING: Gerakkan setir perlahan menggunakan MoveTowards ke sudut target
        currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, targetSteerAngle, steeringSpeed * Time.fixedDeltaTime);

        frontLeftCollider.steerAngle = currentSteerAngle;
        frontRightCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        if (frontLeftMesh != null) UpdateSingleWheel(frontLeftCollider, frontLeftMesh);
        if (frontRightMesh != null) UpdateSingleWheel(frontRightCollider, frontRightMesh);
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

        combinedMesh.rotation = rotL;
        combinedMesh.position = (posL + posR) / 2f;
    }

    // ==================================================
    // FUNGSI BARU: MENAMBAHKAN GAYA DOWNFORCE
    // ==================================================
    private void AddDownforce()
    {
        if (rb != null)
        {
            // Tambahkan gaya dorong ke bawah tegak lurus mobil proporsional dengan kecepatannya
            rb.AddForce(-transform.up * downforceValue * rb.linearVelocity.magnitude);
        }
    }

    // ==================================================
    // FUNGSI BARU: MENGONTROL AREA LOOP KUSTOM MESIN
    // ==================================================
    private void HandleEngineLoopRange()
    {
        if (mesinAudioSource == null || mesinAudioSource.clip == null || !mesinAudioSource.isPlaying) return;
 
        float currentTime = mesinAudioSource.time;
        float fadeDuration = 0.06f; // Durasi fade-out & fade-in (60ms) untuk meredam bunyi pop
 
        // 1. Logika Lompat/Loop Kustom (Hanya reset jika menyentuh batas akhir)
        if (currentTime >= loopEndSeconds)
        {
            mesinAudioSource.time = loopStartSeconds;
            currentTime = loopStartSeconds; // Sinkronkan nilai waktu lokal
        }
 
        // 2. Logika Micro-Fade untuk Mencegah Bunyi Cetuk/Dentuman
        float targetVolume = engineMaxVolume;
 
        if (currentTime >= loopEndSeconds - fadeDuration)
        {
            // Fading out saat mendekati akhir loop
            float t = (loopEndSeconds - currentTime) / fadeDuration;
            targetVolume = Mathf.Lerp(0f, engineMaxVolume, t);
        }
        else if (currentTime <= loopStartSeconds + fadeDuration)
        {
            // Fading in saat baru melompat ke awal loop
            float t = (currentTime - loopStartSeconds) / fadeDuration;
            targetVolume = Mathf.Lerp(0f, engineMaxVolume, t);
        }
 
        // Terapkan volume hasil fade kustom
        mesinAudioSource.volume = targetVolume;
    }
}