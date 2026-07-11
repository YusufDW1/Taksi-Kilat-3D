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

    [Header("Jarak Lokasi")]
    [Tooltip("Jarak minimum antara lokasi baru dengan lokasi saat ini agar tidak berdekatan")]
    public float jarakMinimum = 80f;

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
            GameManager gm = FindAnyObjectByType<GameManager>();
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
            FindAnyObjectByType<GameManager>().TambahPenumpang(); 
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
        if (daftarTitik.Length == 0) return;

        Vector3 posisiSaatIni = transform.position;
        System.Collections.Generic.List<Transform> titikValid = new System.Collections.Generic.List<Transform>();

        // 1. Cari semua titik yang jaraknya lebih dari jarakMinimum
        foreach (Transform titik in daftarTitik)
        {
            if (titik != null && Vector3.Distance(posisiSaatIni, titik.position) >= jarakMinimum)
            {
                titikValid.Add(titik);
            }
        }

        // 2. Jika ada titik yang memenuhi syarat jarak minimum, pilih acak dari situ
        if (titikValid.Count > 0)
        {
            int randomIndex = Random.Range(0, titikValid.Count);
            transform.position = titikValid[randomIndex].position;
        }
        else
        {
            // 3. Jika tidak ada yang memenuhi (atau semua terlalu dekat), pilih titik yang PALING JAUH
            Transform titikTerjauh = null;
            float jarakTerjauh = -1f;

            foreach (Transform titik in daftarTitik)
            {
                if (titik != null)
                {
                    float jarak = Vector3.Distance(posisiSaatIni, titik.position);
                    if (jarak > jarakTerjauh)
                    {
                        jarakTerjauh = jarak;
                        titikTerjauh = titik;
                    }
                }
            }

            if (titikTerjauh != null)
            {
                transform.position = titikTerjauh.position;
            }
            else
            {
                // Fallback terakhir jika semua titik null/invalid
                int randomIndex = Random.Range(0, daftarTitik.Length);
                if (daftarTitik[randomIndex] != null)
                    transform.position = daftarTitik[randomIndex].position;
            }
        }
    }
}