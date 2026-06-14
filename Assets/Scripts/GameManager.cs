using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    [Header("Pengaturan Shift/Level")]
    public float waktuBermain = 180f; 
    public int targetPenumpang = 3;
    private int penumpangSekarang = 0;

    [Header("Referensi UI HUD")]
    public TextMeshProUGUI timerText; 
    public TextMeshProUGUI targetText; 

    [Header("Referensi UI Struk (Baru)")]
    public GameObject panelStruk; 
    public TextMeshProUGUI tarifDasarText;
    public TextMeshProUGUI bonusWaktuText;
    public TextMeshProUGUI totalPendapatanText;

    [Header("Referensi UI Pause (Sesuai Hirarki)")]
    [Tooltip("Tarik objek 'Panel_Pause' dari Hierarchy ke sini")]
    public GameObject panelPause;

    [Header("Pengaturan Uang (Baru)")]
    public int hargaPerPenumpang = 15000;
    public int bonusPerDetik = 500;

    // -------- BAGIAN BARU UNTUK AUDIO / SFX --------
    [Header("Pengaturan Audio / SFX")]
    public AudioMixer masterMixer;           // Untuk memuat volume saat pindah level
    public AudioSource sfxPlayer;            // Speaker khusus untuk memutar efek suara
    public AudioClip sfxPenumpangNaik;       // Suara saat penumpang masuk (jemput)
    public AudioClip sfxPenumpangTurun;      // Suara saat penumpang sampai / dapat koin (antar)
    public AudioClip sfxKertasStruk;         // Suara cash register/struk saat level selesai

    [Header("Sistem SFX Tombol Pause (Baru)")]
    [Tooltip("Masukkan komponen AudioSource yang ada di objek _GameManager")]
    public AudioSource sfxAudioSource; 
    [Tooltip("Masukkan AudioClip suara klik/click sound dari folder Assets")]
    public AudioClip clickSoundClip;
    // -----------------------------------------------

    private bool gameAktif = true;

    private void Start()
    {
        // Pastikan game tidak di-pause dan panel struk hilang saat mulai
        Time.timeScale = 1f; 
        if (panelStruk != null) panelStruk.SetActive(false); 
        
        UpdateUITarget(); 

        // Memuat setting volume yang disimpan dari Main Menu
        if (masterMixer != null)
        {
            float savedMusik = PlayerPrefs.GetFloat("MusikVolume", 1f);
            float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);
            
            float bgmDb = Mathf.Log10(Mathf.Clamp(savedMusik, 0.0001f, 1f)) * 20;
            float sfxDb = Mathf.Log10(Mathf.Clamp(savedSFX, 0.0001f, 1f)) * 20;
            
            masterMixer.SetFloat("BGMVol", bgmDb);
            masterMixer.SetFloat("SFXVol", sfxDb);
        }
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

    // Dipanggil dari TaxiMission saat penumpang masuk ke taksi (Jemput)
    public void PenumpangNaik()
    {
        if (sfxPlayer != null && sfxPenumpangNaik != null)
        {
            sfxPlayer.PlayOneShot(sfxPenumpangNaik);
        }
    }

    // Dipanggil dari TaxiMission saat penumpang diturunkan di tujuan (Antar)
    public void TambahPenumpang()
    {
        if (!gameAktif) return; // Cegah error kalau game sudah selesai

        penumpangSekarang++;
        UpdateUITarget();

        // Putar suara antar/koin. Jika kosong, gunakan suara naik sebagai fallback agar tetap bersuara.
        if (sfxPlayer != null)
        {
            if (sfxPenumpangTurun != null)
            {
                sfxPlayer.PlayOneShot(sfxPenumpangTurun);
            }
            else if (sfxPenumpangNaik != null)
            {
                sfxPlayer.PlayOneShot(sfxPenumpangNaik);
            }
        }

        if (penumpangSekarang >= targetPenumpang)
        {
            // Hentikan update timer segera agar adil
            gameAktif = false;
            // Jalankan jeda sebelum memunculkan struk kemenangan
            StartCoroutine(TungguSebelumLevelSelesai());
        }
    }

    private System.Collections.IEnumerator TungguSebelumLevelSelesai()
    {
        // Tunggu 1.2 detik agar SFX penumpang naik (koin) selesai diputar
        yield return new WaitForSeconds(1.2f);
        LevelComplete();
    }

    private void GameOver()
    {
        gameAktif = false;
        waktuBermain = 0; 
        UpdateUITimer();
        Debug.Log("WAKTU HABIS! Game Over.");
    }

    private void LevelComplete()
    {
        gameAktif = false;
        
        // 1. Hitung-hitungan Uang
        int sisaDetik = Mathf.FloorToInt(waktuBermain);
        int totalTarifDasar = targetPenumpang * hargaPerPenumpang;
        int totalBonus = sisaDetik * bonusPerDetik;
        int grandTotal = totalTarifDasar + totalBonus;

        // 2. Masukkan hasil ke UI 
        tarifDasarText.text = "Tarif (" + targetPenumpang + "x) : Rp " + totalTarifDasar.ToString("N0");
        bonusWaktuText.text = "Bonus Waktu (" + sisaDetik + "s) : Rp " + totalBonus.ToString("N0");
        totalPendapatanText.text = "TOTAL PENDAPATAN : Rp " + grandTotal.ToString("N0");

        // Putar efek suara struk/mesin kasir sebelum game di-pause
        if (sfxPlayer != null && sfxKertasStruk != null)
        {
            sfxPlayer.PlayOneShot(sfxKertasStruk);
        }

        // 3. Munculkan Kertas Struk & Hentikan Waktu (Pause)
        if (panelStruk != null) panelStruk.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void LanjutLevelBerikutnya()
    {
        Time.timeScale = 1f; 

        int levelBerikutnya = SceneManager.GetActiveScene().buildIndex + 1;

        if (levelBerikutnya < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("Memuat Level Berikutnya...");
            SceneManager.LoadScene(levelBerikutnya);
        }
        else
        {
            Debug.Log("Game Tamat! Kembali ke Menu Utama.");
            SceneManager.LoadScene(0);
        }
    }

    // Fungsi pembantu untuk memutar suara klik
    private void PutarSuaraKlik()
    {
        if (sfxAudioSource != null && clickSoundClip != null)
        {
            // Menggunakan PlayOneShot agar suara tidak terpotong jika tombol diklik cepat
            sfxAudioSource.PlayOneShot(clickSoundClip);
        }
    }

    // 4. Dipasang pada objek: Menu_btn
    public void KembaliKeMainMenu()
    {
        PutarSuaraKlik(); // <-- Pemicu suara klik tombol main menu
        Time.timeScale = 1f; // PENTING: Wajib dinormalkan agar scene MainMenu tidak ikut membeku/macet!
        SceneManager.LoadScene("MainMenu"); // Sesuaikan dengan nama Scene Menu Utamamu
    }

    // 1. Dipasang pada objek: Tombol_Pause_HUD
    public void PauseGame()
    {
        // Hanya bisa pause jika level masih berjalan (belum tamat / gagal)
        if (!gameAktif) return; 

        PutarSuaraKlik(); // <-- Pemicu suara klik saat tombol pause HUD ditekan

        if (panelPause != null)
        {
            panelPause.SetActive(true); // Membuka Panel_Pause (background, overlay, & tombol otomatis muncul)
            Time.timeScale = 0f;        // Menghentikan waktu game total
        }
    }

    // 2. Dipasang pada objek: Resume_btn
    public void ResumeGame()
    {
        // PENTING: Karena Time.timeScale akan diset ke 0 (game beku), 
        // AudioSource biasa tidak akan mau berbunyi KECUALI kita set ia agar mengabaikan waktu game.
        if (sfxAudioSource != null) sfxAudioSource.ignoreListenerPause = true;

        PutarSuaraKlik(); // <-- Pemicu suara klik tombol resume

        if (panelPause != null)
        {
            panelPause.SetActive(false); // Menyembunyikan kembali Panel_Pause
            Time.timeScale = 1f;         // Mengembalikan waktu game menjadi normal
        }
    }

    // 3. Dipasang pada objek: Setting_btn
    // Parameter ini digunakan agar kamu bisa menarik objek Panel_Setting level kamu langsung lewat Button Event
    public void BukaSettingInGame(GameObject panelSetting)
    {
        PutarSuaraKlik(); // <-- Pemicu suara klik tombol setting
        if (panelSetting != null) 
        {
            panelSetting.SetActive(true);
        }
    }

    // Fungsi untuk menutup panel Setting dari dalam Game (Dipasang di Tombol CLOSE/BACK di Panel Setting)
    public void TutupSettingInGame(GameObject panelSetting)
    {
        PutarSuaraKlik(); // <-- Pemicu suara klik tombol tutup setting
        if (panelSetting != null) 
        {
            panelSetting.SetActive(false);
        }
    }
}