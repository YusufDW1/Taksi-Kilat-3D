using UnityEngine;

public class TaxiMission : MonoBehaviour
{
    [Header("Pengaturan Waktu")]
    public float waktuTunggu = 1.5f;
    private float timer = 0f;

    [Header("Status Penumpang")]
    public bool sedangBawaPenumpang = false; // False = Jemput, True = Antar
    
    [Header("Visual Zona")]
    public Material materialJemput; // Masukkan material warna Biru
    public Material materialAntar;  // Masukkan material warna Hijau
    private Renderer zonaRenderer;

    [Header("Daftar Lokasi")]
    [Tooltip("Titik-titik orang menunggu taksi")]
    public Transform[] titikJemput;
    [Tooltip("Titik-titik tujuan (gedung, rumah, mall, dll)")]
    public Transform[] titikAntar;

    private void Start()
    {
        // Mengambil komponen visual untuk ganti-ganti warna
        zonaRenderer = GetComponent<Renderer>();
        
        // Memulai game dengan mode mencari penumpang
        SetModeJemput(); 
    }

    private void OnTriggerStay(Collider other)
    {
        // 1. Cek apakah yang masuk zona adalah Taksi
        if (other.CompareTag("Player"))
        {
            Rigidbody rbMobil = other.attachedRigidbody;

            // 2. Cek apakah mobil sudah direm sampai berhenti
            if (rbMobil != null && rbMobil.linearVelocity.magnitude < 0.5f)
            {
                timer += Time.deltaTime; // Mulai hitung detik

                // 3. Jika sudah diam selama 1.5 detik
                if (timer >= waktuTunggu)
                {
                    timer = 0f; // Reset waktu
                    ProsesPenumpang(); // Jalankan transisi
                }
            }
            else
            {
                timer = 0f; // Reset jika mobil masih maju/mundur
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer = 0f; // Reset jika mobil keluar lingkaran sebelum waktunya
        }
    }

    private void ProsesPenumpang()
    {
        if (!sedangBawaPenumpang)
        {
            // TAHAP 1: Penumpang Naik
            Debug.Log("Penumpang NAIK! Segera antar ke tujuan.");
            
            // Panggil GameManager untuk memutar SFX penumpang naik
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                gm.PenumpangNaik();
            }

            SetModeAntar();
        }
        else
        {
            // TAHAP 2: Penumpang Turun
            Debug.Log("Penumpang TURUN! Misi Selesai.");
            
            // --- BARIS BARU UNTUK UI ---
            // Lapor ke GameManager untuk nambah poin penumpang
            FindObjectOfType<GameManager>().TambahPenumpang(); 
            // ---------------------------

            SetModeJemput();
        }
    }

    private void SetModeJemput()
    {
        sedangBawaPenumpang = false;
        
        // Ubah warna zona jadi Biru
        if(zonaRenderer != null && materialJemput != null) 
            zonaRenderer.material = materialJemput;
        
        // Pindah zona ke salah satu titik jemput
        PindahLokasiAcak(titikJemput);
    }

    private void SetModeAntar()
    {
        sedangBawaPenumpang = true;
        
        // Ubah warna zona jadi Hijau
        if(zonaRenderer != null && materialAntar != null) 
            zonaRenderer.material = materialAntar;
        
        // Pindah zona ke salah satu titik tujuan/drop-off
        PindahLokasiAcak(titikAntar);
    }

    private void PindahLokasiAcak(Transform[] daftarTitik)
    {
        if (daftarTitik.Length > 0)
        {
            int randomIndex = Random.Range(0, daftarTitik.Length);
            transform.position = daftarTitik[randomIndex].position;
        }
    }
}