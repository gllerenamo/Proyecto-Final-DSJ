using UnityEngine;

/// Mantiene el marcador del destino en el minimapa. La cámara del minimapa es fija y
/// encuadra todo el nivel; los marcadores viven en la capa "Minimapa", que la cámara
/// principal no renderiza.
public class MinimapController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private Transform destinationBlip;

    [Header("Parpadeo")]
    [SerializeField] private float blinkSpeed = 5f;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.3f;

    private Vector3 baseScale;

    private void Awake()
    {
        if (orderManager == null) orderManager = FindFirstObjectByType<OrderManager>();
        baseScale = destinationBlip.localScale;
    }

    private void LateUpdate()
    {
        Transform dest = orderManager.CurrentDestination;
        bool visible = dest != null && orderManager.OrderActive;
        destinationBlip.gameObject.SetActive(visible);
        if (!visible) return;

        destinationBlip.position = new Vector3(dest.position.x, dest.position.y, destinationBlip.position.z);
        float t = (Mathf.Sin(Time.unscaledTime * blinkSpeed) + 1f) * 0.5f;
        destinationBlip.localScale = baseScale * Mathf.Lerp(minScale, maxScale, t);
    }
}
