using UnityEngine;

/// Zona de lanzamiento del pedido (física predictiva, Avance 2): este GameObject es el
/// punto de origen y `target` la zona de entrega a distancia. Solo dibuja en el editor
/// un arco de referencia para comprobar que no haya bloqueos en la trayectoria.
public class LaunchZoneGizmo : MonoBehaviour
{
    [Header("Lanzamiento")]
    [SerializeField] private Transform target;
    [SerializeField] private float zoneRadius = 1.5f;

    [Header("Gizmo")]
    [SerializeField] private float arcHeight = 3f;
    [SerializeField] private Color color = new Color(0.95f, 0.35f, 0.15f, 1f);

    public Transform Target => target;
    public float ZoneRadius => zoneRadius;

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, zoneRadius);
        if (target == null) return;

        Gizmos.DrawWireSphere(target.position, zoneRadius);

        const int segments = 24;
        Vector3 prev = transform.position;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 p = Vector3.Lerp(transform.position, target.position, t) + Vector3.up * (4f * arcHeight * t * (1f - t));
            if (i % 2 == 1) Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }
}
