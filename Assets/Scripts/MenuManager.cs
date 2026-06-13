using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.Audio; 
using UnityEngine.UI;    

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject panelSetting;
    public GameObject panelCredit;

    [Header("Audio Control")]
    public AudioMixer masterMixer; 
    public Slider sliderMusik; 
    public Slider sliderSFX;   

    [Header("UI Audio FX")]
    public AudioSource uiAudioSource; 
    public AudioClip clickSound;      

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
        SceneManager.LoadScene(1); 
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
        Application.Quit(); 
    }
}