using UnityEngine;

public class NavigationArrow : MonoBehaviour
{
    [Header("Target Tujuan")]
    public Transform targetZona;

    void Update()
    {
        if (targetZona != null)
        {
            // 1. Suruh panah menatap arah zona tujuan
            transform.LookAt(targetZona);

            // 2. Kunci rotasi X dan Z agar panah tidak mendongak ke langit atau menunduk ke tanah
            // Panah hanya boleh berputar di sumbu Y (kiri/kanan)
            Vector3 rotasiSaatIni = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, rotasiSaatIni.y, 0f);
        }
    }
}