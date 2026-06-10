using UnityEngine;
using UnityEngine.SceneManagement; // Wajib dipanggil untuk berpindah-pindah level/scene

public class MenuManager : MonoBehaviour
{
<<<<<<< HEAD
    [Header("UI Panels (Tarik Objek Panel dari Hierarchy ke Sini)")]
    public GameObject panelSetting;
    public GameObject panelCredit;

    // ==========================================
    // 1. TOMBOL START / MULAI SHIFT
    // ==========================================
    public void MulaiGame()
    {
        // Pastikan waktu berjalan normal (takutnya beku akibat fungsi pause di game over/complete)
        Time.timeScale = 1f; 
        
        Debug.Log("Memuat Level 1...");
        // Memuat Level 1 menggunakan Index 1 agar lebih aman dari salah ketik nama
        SceneManager.LoadScene(1); 
    }

    // ==========================================
    // 2. TOMBOL SETTING (Buka & Tutup)
    // ==========================================
    public void BukaSetting()
    {
        if (panelSetting != null)
        {
            panelSetting.SetActive(true); // Memunculkan panel Setting
            Debug.Log("Panel Setting Dibuka.");
        }
    }

    public void TutupSetting()
    {
        if (panelSetting != null)
        {
            panelSetting.SetActive(false); // Menyembunyikan panel Setting
            Debug.Log("Panel Setting Ditutup.");
        }
    }

    // ==========================================
    // 3. TOMBOL CREDIT (Buka & Tutup)
    // ==========================================
    public void BukaCredit()
    {
        if (panelCredit != null)
        {
            panelCredit.SetActive(true); // Memunculkan panel Credit
            Debug.Log("Panel Credit Dibuka.");
        }
    }

    public void TutupCredit()
    {
        if (panelCredit != null)
        {
            panelCredit.SetActive(false); // Menyembunyikan panel Credit
            Debug.Log("Panel Credit Ditutup.");
        }
    }

    // ==========================================
    // 4. TOMBOL EXIT / KELUAR
    // ==========================================
    public void KeluarGame()
    {
        Debug.Log("Keluar dari Game.");
        Application.Quit(); // Hanya berfungsi setelah game di-build/jadi file .exe atau .apk
=======
    // Fungsi ini akan dipanggil saat Tombol Play diklik
    public void MulaiGame()
    {
        Debug.Log("Memuat Level 1...");
        // Pastikan nama di dalam tanda kutip PERSIS dengan nama file scene Level 1 kamu
        SceneManager.LoadScene("Level_1"); 
    }

    // Fungsi ini akan dipanggil saat Tombol Quit diklik
    public void KeluarGame()
    {
        Debug.Log("Keluar dari Game.");
        Application.Quit(); // Catatan: Ini hanya akan berfungsi jika game sudah di-build (di-export)
>>>>>>> fd908f1a3e652c8187809e8ec853bbaac37d071f
    }
}