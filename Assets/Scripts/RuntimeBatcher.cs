using UnityEngine;

public class RuntimeBatcher : MonoBehaviour
{
    void Awake()
    {
        // Batch all static and non-static meshes under this GameObject at runtime.
        // This gives the massive performance boost of Static Batching
        // WITHOUT bloating the APK build size by hundreds of megabytes!
        StaticBatchingUtility.Combine(gameObject);
    }
}
