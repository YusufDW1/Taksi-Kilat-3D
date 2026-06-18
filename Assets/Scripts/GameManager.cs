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

    [Header("Referensi HUD")]
    public GameObject tombolPauseHUD; // Tarik objek Tombol_Pause_HUD ke sini via Inspector 

    [Header("Referensi UI Struk")]
    public GameObject panelStruk; 
    public TextMeshProUGUI tarifDasarText;
    public TextMeshProUGUI bonusWaktuText;
    public TextMeshProUGUI totalPendapatanText;
    public TextMeshProUGUI textHighScore;

    [Header("Referensi UI Pause")]
    [Tooltip("Tarik objek 'Panel_Pause' dari Hierarchy ke sini")]
    public GameObject panelPause;

    [Header("Referensi UI Game Over")]
    public GameObject panelGameOver; // Tarik objek Panel_GameOver ke sini via Inspector

    [Header("Referensi UI Tutorial ")]
    public GameObject panelTutorial; // Tarik objek Panel_Tutorial ke sini via Inspector

    [Header("Pengaturan Uang")]
    public int hargaPerPenumpang = 15000;
    public int bonusPerDetik = 500;

    // -------- BAGIAN BARU UNTUK AUDIO / SFX --------
    [Header("Pengaturan Audio / SFX")]
    public AudioMixer masterMixer;           // Untuk memuat volume saat pindah level
    public AudioSource sfxPlayer;            // Speaker khusus untuk memutar efek suara
    public AudioClip sfxPenumpangNaik;       // Suara saat penumpang masuk (jemput)
    public AudioClip sfxPenumpangTurun;      // Suara saat penumpang sampai / dapat koin (antar)
    public AudioClip sfxKertasStruk;         // Suara cash register/struk saat level selesai

    [Header("Sistem SFX Tombol Pause")]
    [Tooltip("Masukkan komponen AudioSource yang ada di objek _GameManager")]
    public AudioSource sfxAudioSource; 
    [Tooltip("Masukkan AudioClip suara klik/click sound dari folder Assets")]
    public AudioClip clickSoundClip;
    // -----------------------------------------------

    private bool gameAktif = true;

    private void Start()
    {
        // Pastikan panel struk/GameOver hilang saat mulai
        if (panelStruk != null) panelStruk.SetActive(false); 
        if (panelGameOver != null) panelGameOver.SetActive(false); 
        if (tombolPauseHUD != null) tombolPauseHUD.SetActive(true); // Pastikan tombol pause aktif saat mulai
        
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

        // Cek apakah ini Level 1 (asumsi nama scene kamu adalah "Level_1")
        // Ini biar tutorial gak muncul lagi di Level 2 atau Level 3
        Debug.Log("GameManager Start - Scene Aktif: '" + SceneManager.GetActiveScene().name + "'");
        if (panelTutorial == null)
        {
            Debug.LogWarning("Peringatan: panelTutorial bernilai NULL! Pastikan Anda sudah menarik objek Panel_Tutorial ke slot panelTutorial di Inspector GameManager pada Scene Level_1.");
        }

        if (SceneManager.GetActiveScene().name == "Level_1")
        {
            gameAktif = false;
            Time.timeScale = 0f; // Bekukan waktu game (taksi & timer tidak jalan)
            
            if (panelTutorial != null)
            {
                panelTutorial.SetActive(true); // Munculkan kertas tutorial
                Debug.Log("GameManager - Berhasil mengaktifkan panelTutorial!");
            }
        }
        else
        {
            // Jika bukan Level 1 (misal Level 2), langsung mulai game
            MulaiPermainanLangsung();
        }
    }

    private void Update()
    {
        if (!gameAktif) return; 

        waktuBermain -= Time.deltaTime;
        UpdateUITimer();

        if (waktuBermain <= 0)
        {
            PemicuGameOver();
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

            // Sembunyikan tombol pause HUD agar tidak bocor ke panel struk
            if (tombolPauseHUD != null) tombolPauseHUD.SetActive(false);

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

    // Fungsi yang dipanggil otomatis dari script Timer kamu saat waktu mencapai 0
    public void PemicuGameOver()
    {
        if (!gameAktif) return; // Jika sudah menang/pause, jangan pemicu game over lagi

        gameAktif = false;
        Time.timeScale = 0f; // Hentikan pergerakan taksi dan dunia game

        // Sembunyikan tombol pause HUD agar layar bersih
        if (tombolPauseHUD != null) tombolPauseHUD.SetActive(false);

        // Munculkan panel Game Over
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }
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

        // Hitung dan Simpan High Score (Baru)
        HitungDanSimpanHighScore(grandTotal);

        // Putar efek suara struk/mesin kasir sebelum game di-pause
        if (sfxPlayer != null && sfxKertasStruk != null)
        {
            sfxPlayer.PlayOneShot(sfxKertasStruk);
        }

        // Sembunyikan tombol pause agar tidak menumpuk dengan panel struk
        if (tombolPauseHUD != null) tombolPauseHUD.SetActive(false);

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

    // Fungsi untuk mengulang level yang sama (Dipasang di Tombol REPLAY)
    public void ReplayLevel()
    {
        Time.timeScale = 1f; // PENTING: Kembalikan waktu ke normal sebelum reload!
        
        // Mengambil nama scene yang sedang aktif saat ini lalu memuatnya ulang
        string sceneSekarang = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneSekarang);
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

    // Panggil fungsi ini di dalam fungsi tempat kamu mengkalkulasi total pendapatan (saat level selesai)
    public void HitungDanSimpanHighScore(float totalDuitShiftIni)
    {
        // 1. Ambil nama scene/level aktif saat ini secara dinamis (Lvl 1, 2, atau 3)
        // Ini biar key PlayerPrefs-nya unik tiap level (contoh: "HighScore_Level1")
        string keyLevel = "HighScore_" + SceneManager.GetActiveScene().name;

        // 2. Ambil rekor lama dari memori lokal (kalau belum ada, otomatis diset 0)
        float rekorLama = PlayerPrefs.GetFloat(keyLevel, 0f);

        // 3. Cek apakah pendapatan shift ini berhasil melewati rekor lama
        if (totalDuitShiftIni > rekorLama)
        {
            rekorLama = totalDuitShiftIni; // Update variabel rekorLama ke angka baru
            PlayerPrefs.SetFloat(keyLevel, totalDuitShiftIni); // Simpan permanen ke memori lokal
            PlayerPrefs.Save(); // Amankan data
        }

        // 4. Tampilkan angkanya ke TextMeshPro UI dengan format mata uang Rp
        if (textHighScore != null)
        {
            textHighScore.text = "REKOR TERBAIK : Rp " + rekorLama.ToString("N0");
            
            // Opsional: Kalau pecah rekor baru, teksnya bisa kamu beri warna hijau/emas biar dramatis!
            if (totalDuitShiftIni >= rekorLama && totalDuitShiftIni > 0)
            {
                textHighScore.text += " <color=yellow>(REKOR BARU!)</color>";
            }
        }
    }

    // Fungsi tambahan untuk memulai game tanpa tutorial (untuk Lvl 2 & 3)
    public void MulaiPermainanLangsung()
    {
        gameAktif = true;
        Time.timeScale = 1f; // Jalankan waktu normal

        // Matikan panel tutorial jika aktif (biar aman)
        if (panelTutorial != null) panelTutorial.SetActive(false);
    }

    // Fungsi yang dipasang di TOMBOL MENGERTI!
    public void TutupTutorialDanMulaiGame()
    {
        gameAktif = true;
        Time.timeScale = 1f; // Jalankan waktu normal (game dimulai!)

        // Sembunyikan panel tutorial
        if (panelTutorial != null)
        {
            panelTutorial.SetActive(false);
        }
    }
}