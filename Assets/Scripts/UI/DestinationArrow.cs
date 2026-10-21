using TMPro;
using UnityEngine;

/// Flecha que guía al destino del pedido: en el borde de la pantalla cuando está fuera
/// de cámara y flotando sobre el punto cuando ya es visible.
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DestinationArrow : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private Transform player;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private TMP_Text distanceText;
    [Tooltip("Contenedor (fondo + texto) de la distancia; si está vacío se usa el texto")]
    [SerializeField] private GameObject distanceLabel;

    [Header("Ajustes")]
    [SerializeField] private float edgePadding = 90f;
    [Tooltip("Margen superior mayor para no tapar los paneles del HUD")]
    [SerializeField] private float topPadding = 220f;
    [SerializeField] private float hoverOffset = 90f;
    [Tooltip("Escala narrativa: cuántos metros representa una celda del mapa")]
    [SerializeField] private float metersPerUnit = 5f;

    private RectTransform rect;
    private RectTransform canvasRect;
    private CanvasGroup group;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        group = GetComponent<CanvasGroup>();
        canvasRect = (RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform;

        if (orderManager == null) orderManager = FindFirstObjectByType<OrderManager>();
        if (worldCamera == null) worldCamera = Camera.main;
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    private void LateUpdate()
    {
        Transform dest = orderManager.CurrentDestination;
        if (dest == null || !orderManager.OrderActive || player == null)
        {
            group.alpha = 0f;
            return;
        }

        group.alpha = 1f;
        Vector3 vp = worldCamera.WorldToViewportPoint(dest.position);
        Vector2 size = canvasRect.rect.size;
        Vector2 dir = new Vector2((vp.x - 0.5f) * size.x, (vp.y - 0.5f) * size.y);
        bool onScreen = vp.x > 0.04f && vp.x < 0.96f && vp.y > 0.06f && vp.y < 0.94f;

        if (onScreen)
        {
            // Con el destino en pantalla, la flecha flota sobre él para distinguirlo de los demás puntos.
            float bob = Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4f)) * 18f;
            rect.anchoredPosition = dir + new Vector2(0f, hoverOffset + bob);
            arrow.localRotation = Quaternion.Euler(0f, 0f, -90f);
        }
        else
        {
            float limitX = size.x * 0.5f - edgePadding;
            float limitY = size.y * 0.5f - (dir.y > 0f ? topPadding : edgePadding);
            float scale = Mathf.Min(limitX / Mathf.Max(Mathf.Abs(dir.x), 0.001f), limitY / Mathf.Max(Mathf.Abs(dir.y), 0.001f));
            rect.anchoredPosition = dir * scale;
            arrow.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }

        (distanceLabel != null ? distanceLabel : distanceText.gameObject).SetActive(!onScreen);
        float meters = Vector2.Distance(player.position, dest.position) * metersPerUnit;
        distanceText.text = $"{Mathf.RoundToInt(meters)} m";
    }
}
