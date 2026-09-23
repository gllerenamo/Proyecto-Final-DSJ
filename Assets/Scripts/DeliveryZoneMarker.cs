using UnityEngine;

/// <summary>
/// Marcador visual para zonas de recojo y entrega (restaurantes, hospital, residencias).
/// Produce un efecto de pulsación para destacar el destino en el mapa.
/// </summary>
public class DeliveryZoneMarker : MonoBehaviour
{
    [Header("Identificación")]
    public string zoneName = "Punto de Entrega";

    [Header("Efecto de pulso")]
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float minScale = 0.85f;
    [SerializeField] private float maxScale = 1.15f;

    [Header("Detección")]
    [SerializeField] private float triggerRadius = 1.5f;

    private Vector3 initialScale;

    private void Start()
    {
        initialScale = transform.localScale;
    }

    private void Update()
    {
        float scaleMultiplier = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
        transform.localScale = initialScale * scaleMultiplier;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.35f);
        Gizmos.DrawSphere(transform.position, triggerRadius);
        Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
