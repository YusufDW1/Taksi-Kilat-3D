using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Pengaturan Shift/Level")]
    public float waktuBermain = 180f; 
    public int targetPenumpang = 3;
    private int penumpangSekarang = 0;

    [Header("Referensi UI HUD")]
    public TextMeshProUGUI timerText; 
    public TextMeshProUGUI targetText; 

    // -------- BAGIAN BARU UNTUK STRUK --------
    [Header("Referensi UI Struk (Baru)")]
    public GameObject panelStruk; // Objek panel induk
    public TextMeshProUGUI tarifDasarText;
    public TextMeshProUGUI bonusWaktuText;
    public TextMeshProUGUI totalPendapatanText;

    [Header("Pengaturan Uang (Baru)")]
    public int hargaPerPenumpang = 15000;
    public int bonusPerDetik = 500;
    // -----------------------------------------

    private bool gameAktif = true;

    private void Start()
    {
        // Pastikan game tidak di-pause dan panel struk hilang saat mulai
        Time.timeScale = 1f; 
        if (panelStruk != null) panelStruk.SetActive(false); 
        
        UpdateUITarget(); 
    }

    private void Update()
    {
        if (!gameAktif) return; 

        waktuBermain -= Time.deltaTime;
        UpdateUITimer();

        if (waktuBermain <= 0)
        {
            GameOver();
        }
    }

    private void UpdateUITimer()
    {
        int menit = Mathf.FloorToInt(waktuBermain / 60);
        int detik = Mathf.FloorToInt(waktuBermain % 60);
        timerText.text = string.Format("{0:00}:{1:00}", menit, detik);
    }

    private void UpdateUITarget()
    {
        targetText.text = "Penumpang: " + penumpangSekarang + " / " + targetPenumpang;
    }

    public void TambahPenumpang()
    {
        if (!gameAktif) return; // Cegah error kalau game sudah selesai

        penumpangSekarang++;
        UpdateUITarget();

        if (penumpangSekarang >= targetPenumpang)
        {
            LevelComplete();
        }
    }

    private void GameOver()
    {
        gameAktif = false;
        waktuBermain = 0; 
        UpdateUITimer();
        Debug.Log("WAKTU HABIS! Game Over.");
    }

    // FUNGSI INI DI-UPGRADE DENGAN KALKULASI MATEMATIKA
    private void LevelComplete()
    {
        gameAktif = false;
        
        // 1. Hitung-hitungan Uang
        int sisaDetik = Mathf.FloorToInt(waktuBermain);
        int totalTarifDasar = targetPenumpang * hargaPerPenumpang;
        int totalBonus = sisaDetik * bonusPerDetik;
        int grandTotal = totalTarifDasar + totalBonus;

        // 2. Masukkan hasil ke UI ("N0" digunakan untuk memberi format ribuan, misal 45000 jadi 45.000)
        tarifDasarText.text = "Tarif (" + targetPenumpang + "x) : Rp " + totalTarifDasar.ToString("N0");
        bonusWaktuText.text = "Bonus Waktu (" + sisaDetik + "s) : Rp " + totalBonus.ToString("N0");
        totalPendapatanText.text = "TOTAL PENDAPATAN : Rp " + grandTotal.ToString("N0");

        // 3. Munculkan Kertas Struk & Hentikan Waktu (Pause)
        if (panelStruk != null) panelStruk.SetActive(true);
        Time.timeScale = 0f; // Pause game agar mobil berhenti saat struk muncul
    }
    // Fungsi ini dipanggil saat pemain menekan tombol LANJUT di layar Struk
    public void LanjutLevelBerikutnya()
    {
        // Wajib! Kembalikan waktu berjalan normal (1) karena sebelumnya kita pause (0) di layar struk
        Time.timeScale = 1f; 

        // Membaca urutan (index) level yang sedang dimainkan, lalu ditambah 1
        int levelBerikutnya = SceneManager.GetActiveScene().buildIndex + 1;

        // Cek apakah level berikutnya ada di daftar Build Profiles?
        if (levelBerikutnya < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("Memuat Level Berikutnya...");
            SceneManager.LoadScene(levelBerikutnya);
        }
        else
        {
            // Jika sudah tamat (tidak ada level lagi), kembalikan ke Main Menu (Index 0)
            Debug.Log("Game Tamat! Kembali ke Menu Utama.");
            SceneManager.LoadScene(0);
        }
    }
    // Fungsi untuk kembali ke Main Menu (Bisa dipasang di tombol mana saja)
    public void KembaliKeMainMenu()
    {
        // Pastikan waktu berjalan normal
        Time.timeScale = 1f; 
        
        // Pindah ke MainMenu (Index 0 di Build Profiles)
        Debug.Log("Kembali ke Menu Utama...");
        SceneManager.LoadScene(0); 
    }
}