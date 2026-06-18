using UnityEngine;
using UnityEngine.EventSystems; // Wajib untuk deteksi sentuhan layar

// Script ini menggunakan antarmuka pointer bawaan Unity
public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector]
    public bool isPressed = false; // Status apakah tombol sedang ditahan

    // Saat jempol menyentuh layar
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    // Saat jempol dilepas dari layar
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }
}
