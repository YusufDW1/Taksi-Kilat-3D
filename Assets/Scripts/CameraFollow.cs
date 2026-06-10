using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Referensi Objek")]
    [Tooltip("Masukkan objek Target_Kamera yang ada di dalam mobilmu")]
    public Transform target;

    [Header("Pengaturan Posisi")]
    [Tooltip("X: Kiri/Kanan, Y: Tinggi, Z: Jarak Depan/Belakang")]
    public Vector3 offset = new Vector3(0f, 4f, -7f); 
    
    [Header("Pengaturan Kelenturan")]
    public float smoothSpeed = 10f; // Semakin kecil angkanya, kamera semakin "karet" (lambat menyusul)

    private void LateUpdate()
    {
        // Cegah error jika target belum diisi
        if (target == null) return;

        // 1. Kalkulasi posisi ideal kamera (berada di belakang target sesuai offset)
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        
        // 2. Gerakkan posisi kamera secara mulus (Lerp) dari posisi saat ini ke posisi ideal
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 3. Paksa kamera untuk selalu menatap tepat ke arah target
        transform.LookAt(target);
    }
}