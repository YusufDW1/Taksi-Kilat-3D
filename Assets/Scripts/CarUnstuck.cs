using UnityEngine;

public class CarUnstuck : MonoBehaviour
{
    private Rigidbody rbMobil;

    private void Start()
    {
        // Mengambil komponen Rigidbody secara otomatis saat game mulai
        rbMobil = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Mengecek apakah pemain menekan tombol 'R' di keyboard
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPosisiMobil();
        }
    }

    private void ResetPosisiMobil()
    {
        if (rbMobil == null) return;

        // 1. Hapus semua momentum gaya (Nol-kan kecepatan)
        // Ini SANGAT PENTING agar mobil tidak kembali melayang saat di-reset
        rbMobil.linearVelocity = Vector3.zero;
        rbMobil.angularVelocity = Vector3.zero;

        // 2. Berdirikan mobil (Tegakkan rotasinya)
        // Kita biarkan mobil menghadap ke arah aslinya (sumbu Y), tapi kita nol-kan kemiringannya (X dan Z)
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

        // 3. Angkat mobil sedikit ke udara
        // Agar roda tidak nyangkut di dalam aspal saat di-reset
        transform.position = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
        
        Debug.Log("Mobil berhasil di-Reset!");
    }
}