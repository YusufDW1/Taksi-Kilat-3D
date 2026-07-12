using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Referensi Objek")]
    [Tooltip("Masukkan objek Target_Kamera yang ada di dalam mobilmu")]
    public Transform target;

    [Header("Pengaturan Posisi")]
    [Tooltip("X: Kiri/Kanan, Y: Tinggi, Z: Jarak Depan/Belakang")]
    public Vector3 offset = new Vector3(0f, 6.5f, -14f); 
    
    [Header("Pengaturan Kelenturan")]
    public float smoothSpeed = 10f; // Semakin kecil angkanya, kamera semakin "karet" (lambat menyusul)

    private void Start()
    {
        // --- PAKSA UPDATE OFFSET ---
        // (Nilai bawaan script sering diabaikan Unity jika Inspector sudah terlanjur menyimpan angka lama.
        // Kita paksa ubah nilainya di sini saat game dimulai agar langsung efek).
        offset = new Vector3(0f, 2.0f, -6.8f);

        // --- OPTIMASI RENDER (Rahasia Performa 60 FPS untuk Kota Besar) ---
        // Membatasi jarak pandang kamera (Far Clip Plane) agar tidak menggambar objek di ujung kota
        // Ini memangkas ribuan objek dari pemrosesan GPU/CPU secara instan
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.farClipPlane = 120f; 
        }

        // Aktifkan kabut (Fog) untuk menyamarkan batas pemotongan jarak pandang
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 80f;
        RenderSettings.fogEndDistance = 120f;
        // Warna kabut disesuaikan dengan warna langit/atmosfer (biru keputihan)
        RenderSettings.fogColor = new Color(0.6f, 0.7f, 0.8f);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // --- ALGORITMA ANTI-JITTER KELAS ATAS ---
        // Alih-alih mengejar target posisi 3D (yang menyebabkan kedutan akibat selisih frame rate vs physics),
        // kita mengunci jarak matematis dari sudut rotasi yang dihaluskan.

        float wantedRotationAngle = target.eulerAngles.y;
        float wantedHeight = target.position.y + offset.y;

        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        // 1. Haluskan rotasi dan ketinggian
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, smoothSpeed * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, smoothSpeed * Time.deltaTime);

        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // 2. Set posisi di belakang mobil sesuai rotasi yang sudah mulus
        Vector3 pos = target.position;
        pos -= currentRotation * Vector3.forward * Mathf.Abs(offset.z);
        
        // 3. Terapkan ketinggian
        pos.y = currentHeight;
        transform.position = pos;

        // 4. Kamera fokus menatap mobil (titik fokus sedikit diturunkan agar pantat mobil aman dari UI)
        transform.LookAt(target.position + Vector3.up * 0.4f);
    }
}