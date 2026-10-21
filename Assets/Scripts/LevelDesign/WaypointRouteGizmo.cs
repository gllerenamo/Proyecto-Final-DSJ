using UnityEngine;

/// HU-11: Ruta de waypoints colocada por diseño. Los waypoints son los hijos de este
/// GameObject, en orden de jerarquía. Solo dibuja la ruta en el editor; el seguidor
/// (WaypointFollower) lee los hijos y el modo.
public class WaypointRouteGizmo : MonoBehaviour
{
    public enum RouteMode { Lineal, Ciclica, IdaVuelta }

    [Header("Ruta")]
    [SerializeField] private RouteMode mode = RouteMode.Lineal;

    [Header("Gizmo")]
    [SerializeField] private Color color = new Color(1f, 0.75f, 0.1f, 1f);
    [SerializeField] private float pointRadius = 0.35f;

    public RouteMode Mode => mode;

    private void OnDrawGizmos()
    {
        int count = transform.childCount;
        if (count == 0) return;

        Gizmos.color = color;
        for (int i = 0; i < count; i++)
        {
            Vector3 p = transform.GetChild(i).position;
            Gizmos.DrawWireSphere(p, pointRadius);
            if (i + 1 < count) Gizmos.DrawLine(p, transform.GetChild(i + 1).position);
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.Label(p + Vector3.up * (pointRadius + 0.2f), (i + 1).ToString());
#endif
        }

        if (mode == RouteMode.Ciclica && count > 2)
            Gizmos.DrawLine(transform.GetChild(count - 1).position, transform.GetChild(0).position);
    }
}
