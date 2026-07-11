using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.Audio; 
using UnityEngine.UI;    

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels (Tarik Objek Panel dari Hierarchy ke Sini)")]
    public GameObject panelSetting;
    public GameObject panelCredit;

    [Header("Audio Control")]
    public AudioMixer masterMixer; 
    public Slider sliderMusik; 
    public Slider sliderSFX;   

    [Header("UI Audio FX")]
    public AudioSource uiAudioSource; 
    public AudioClip clickSound;      

    [Header("UI Pilih Shift (Tarik Objek Panel_PilihShift ke Sini)")]
    public GameObject panelPilihShift;
    private UnityEngine.UI.Button[] btnShift = new UnityEngine.UI.Button[3];
    private GameObject[] lockOv = new GameObject[2];
    private bool isShiftPanelInitialized = false;

    private void Start()
    {
        // SAAT LEVEL DIMULAI: Load nilai volume yang tersimpan, jika belum ada set ke default (1f)
        float savedMusik = PlayerPrefs.GetFloat("MusikVolume", 1f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Terapkan ke Mixer
        SetVolumeMixer("BGMVol", savedMusik);
        SetVolumeMixer("SFXVol", savedSFX);

        // Sinkronkan posisi Slider di UI (jika objek slidernya ada di Scene ini)
        if (sliderMusik != null)
        {
            sliderMusik.value = savedMusik;
            sliderMusik.onValueChanged.AddListener(SetVolumeMusik);
        }

        if (sliderSFX != null)
        {
            sliderSFX.value = savedSFX;
            sliderSFX.onValueChanged.AddListener(SetVolumeSFX);
        }
    }

    // Fungsi khusus mengontrol & MENYIMPAN volume Musik
    public void SetVolumeMusik(float sliderValue)
    {
        SetVolumeMixer("BGMVol", sliderValue);
        PlayerPrefs.SetFloat("MusikVolume", sliderValue); // Simpan ke memori
        PlayerPrefs.Save();
    }

    // Fungsi khusus mengontrol & MENYIMPAN volume SFX
    public void SetVolumeSFX(float sliderValue)
    {
        SetVolumeMixer("SFXVol", sliderValue);
        PlayerPrefs.SetFloat("SFXVolume", sliderValue); // Simpan ke memori
        PlayerPrefs.Save();
    }

    // Fungsi pembantu untuk konversi nilai slider ke Desibel Mixer
    private void SetVolumeMixer(string parameterName, float sliderValue)
    {
        if (masterMixer != null)
        {
            float dbValue = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;
            masterMixer.SetFloat(parameterName, dbValue);
        }
    }

    public void PlayClickSound()
    {
        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }
    }

    // ==========================================
    // FUNGSI NAVIGASI TOMBOL
    // ==========================================
    public void MulaiGame()
    {
        PlayClickSound(); // Bunyikan suara klik sebelum pindah scene
        Time.timeScale = 1f;
        
        // Buka panel Pilih Shift (bukan langsung load Level_1)
        if (panelPilihShift != null) 
        {
            if (!isShiftPanelInitialized) InitPanelPilihShift();
            panelPilihShift.SetActive(true);
            RefreshLocksShift();
        }
        else SceneManager.LoadScene(1); // fallback kalau panel belum ditarik
    }

    private void InitPanelPilihShift()
    {
        if (panelPilihShift == null) return;
        
        Transform papan = panelPilihShift.transform.Find("3_Papan");
        if (papan != null)
        {
            UnityEngine.UI.Button b1 = papan.Find("Btn_Shift1")?.GetComponent<UnityEngine.UI.Button>();
            if (b1 != null) { btnShift[0] = b1; b1.onClick.RemoveAllListeners(); b1.onClick.AddListener(PilihShift1); }
            
            UnityEngine.UI.Button b2 = papan.Find("Btn_Shift2")?.GetComponent<UnityEngine.UI.Button>();
            if (b2 != null) { btnShift[1] = b2; b2.onClick.RemoveAllListeners(); b2.onClick.AddListener(PilihShift2); }
            
            UnityEngine.UI.Button b3 = papan.Find("Btn_Shift3")?.GetComponent<UnityEngine.UI.Button>();
            if (b3 != null) { btnShift[2] = b3; b3.onClick.RemoveAllListeners(); b3.onClick.AddListener(PilihShift3); }
            
            UnityEngine.UI.Button bK = papan.Find("Btn_Kembali")?.GetComponent<UnityEngine.UI.Button>();
            if (bK != null) { bK.onClick.RemoveAllListeners(); bK.onClick.AddListener(KembaliPilihShift); }

            Transform l2 = papan.Find("Lock2");
            if (l2 != null) lockOv[0] = l2.gameObject;

            Transform l3 = papan.Find("Lock3");
            if (l3 != null) lockOv[1] = l3.gameObject;
        }
        isShiftPanelInitialized = true;
    }

    public void RefreshLocksShift()
    {
        bool s2 = IsUnlockedShift(2), s3 = IsUnlockedShift(3);
        if (lockOv[0] != null) lockOv[0].SetActive(!s2);
        if (lockOv[1] != null) lockOv[1].SetActive(!s3);
        if (btnShift[1] != null) btnShift[1].interactable = s2;
        if (btnShift[2] != null) btnShift[2].interactable = s3;
    }

    public void PilihShift(int nomor)
    {
        if (nomor < 1 || nomor > 3) return;
        if (!IsUnlockedShift(nomor)) return;
        PlayClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(nomor);
    }
    public void PilihShift1() { PilihShift(1); }
    public void PilihShift2() { PilihShift(2); }
    public void PilihShift3() { PilihShift(3); }
    public void KembaliPilihShift() { PlayClickSound(); if (panelPilihShift != null) panelPilihShift.SetActive(false); }

    public static bool IsUnlockedShift(int nomor)
    {
        if (nomor <= 1) return true;
        return PlayerPrefs.GetInt("ShiftUnlocked_" + nomor, 0) == 1;
    }

    public static void UnlockShiftBerikutnya(int nomorSelesai)
    {
        int next = nomorSelesai + 1;
        if (next >= 2 && next <= 3)
        {
            PlayerPrefs.SetInt("ShiftUnlocked_" + next, 1);
            PlayerPrefs.Save();
        }
    }

    public void BukaSetting()
    {
        PlayClickSound();
        if (panelSetting != null) panelSetting.SetActive(true);
    }

    public void TutupSetting()
    {
        PlayClickSound();
        if (panelSetting != null) panelSetting.SetActive(false);
    }

    public void BukaCredit()
    {
        PlayClickSound();
        if (panelCredit != null) panelCredit.SetActive(true);
    }

    public void TutupCredit()
    {
        PlayClickSound();
        if (panelCredit != null) panelCredit.SetActive(false);
    }

    public void KeluarGame()
    {
        PlayClickSound();
        Debug.Log("Keluar dari Game.");
        Application.Quit(); // Hanya berfungsi setelah game di-build/jadi file .exe atau .apk
    }
}