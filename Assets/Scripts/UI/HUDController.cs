using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// HUD del nivel: temporizador, tarjeta del pedido, progreso de entregas, mensajes
/// y panel de fin de nivel. Solo lee estado y escucha eventos de OrderManager/LevelManager.
public class HUDController : MonoBehaviour
{
    [Header("Sistemas")]
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private LevelManager levelManager;

    [Header("Tarjeta de pedido")]
    [SerializeField] private TMP_Text destinationText;
    [SerializeField] private Image destinationIcon;

    [Header("Temporizador")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private RectTransform timerPanel;
    [SerializeField] private float warningTime = 20f;
    [SerializeField] private float dangerTime = 10f;
    [SerializeField] private Color normalColor = new Color(0.16f, 0.10f, 0.05f);
    [SerializeField] private Color warningColor = new Color(0.80f, 0.45f, 0.05f);
    [SerializeField] private Color dangerColor = new Color(0.80f, 0.12f, 0.12f);

    [Header("Progreso")]
    [SerializeField] private TMP_Text deliveriesText;

    [Header("Mensajes")]
    [SerializeField] private CanvasGroup levelBanner;
    [SerializeField] private TMP_Text levelBannerText;
    [SerializeField] private float bannerDuration = 2.5f;
    [SerializeField] private CanvasGroup controlsHint;
    [SerializeField] private float hintDuration = 7f;
    [SerializeField] private CanvasGroup toast;
    [SerializeField] private TMP_Text toastText;
    [SerializeField] private float toastDuration = 1.8f;

    [Header("Fin de nivel")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text endTitleText;
    [SerializeField] private TMP_Text endBodyText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button nextButton;

    private Transform shownDestination;
    private float elapsed;
    private float toastTimer;

    private void Awake()
    {
        if (orderManager == null) orderManager = FindFirstObjectByType<OrderManager>();
        if (levelManager == null) levelManager = FindFirstObjectByType<LevelManager>();

        if (retryButton != null) retryButton.onClick.AddListener(levelManager.RestartLevel);
        if (menuButton != null) menuButton.onClick.AddListener(levelManager.GoToMainMenu);
        if (nextButton != null) nextButton.onClick.AddListener(levelManager.LoadNextLevel);
    }

    private void OnEnable()
    {
        orderManager.OnTimeUpdated += UpdateTimer;
        orderManager.OnOrderDelivered += HandleDelivered;
        levelManager.OnDeliveriesChanged += UpdateDeliveries;
        levelManager.OnLevelCompleted += HandleLevelCompleted;
        levelManager.OnLevelFailed += HandleLevelFailed;
    }

    private void OnDisable()
    {
        orderManager.OnTimeUpdated -= UpdateTimer;
        orderManager.OnOrderDelivered -= HandleDelivered;
        levelManager.OnDeliveriesChanged -= UpdateDeliveries;
        levelManager.OnLevelCompleted -= HandleLevelCompleted;
        levelManager.OnLevelFailed -= HandleLevelFailed;
    }

    private void Start()
    {
        if (endPanel != null) endPanel.SetActive(false);
        if (toast != null) toast.alpha = 0f;
        if (levelBannerText != null) levelBannerText.text = levelManager.LevelDisplayName;
        UpdateDeliveries(levelManager.DeliveriesDone, levelManager.DeliveriesToComplete);
        UpdateTimer(orderManager.TimeRemaining);
    }

    private void Update()
    {
        // Animaciones con tiempo sin escalar: siguen corriendo con el juego congelado al final.
        // Se acota porque el primer frame tras cargar la escena trae un delta de varios segundos.
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
        elapsed += dt;

        if (levelBanner != null) levelBanner.alpha = FadeOut(elapsed, bannerDuration);
        if (controlsHint != null) controlsHint.alpha = FadeOut(elapsed, hintDuration);

        if (toast != null && toastTimer > 0f)
        {
            toastTimer -= dt;
            toast.alpha = Mathf.Clamp01(toastTimer / 0.4f);
        }

        if (orderManager.CurrentDestination != shownDestination || !orderManager.OrderActive)
            RefreshOrderCard();

        if (timerPanel != null)
        {
            bool danger = orderManager.OrderActive && orderManager.TimeRemaining <= dangerTime;
            float pulse = danger ? 1f + 0.08f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 6f)) : 1f;
            timerPanel.localScale = Vector3.one * pulse;
        }
    }

    private static float FadeOut(float t, float duration) => Mathf.Clamp01((duration - t) / 0.6f);

    private void RefreshOrderCard()
    {
        if (!orderManager.OrderActive)
        {
            if (levelManager.State == LevelManager.LevelState.Playing)
                destinationText.text = "<size=70%>PEDIDO</size>\nPreparando el siguiente…";
            shownDestination = null;
            return;
        }

        shownDestination = orderManager.CurrentDestination;
        var marker = shownDestination.GetComponent<DeliveryZoneMarker>();
        string zone = marker != null ? marker.zoneName : shownDestination.name;
        destinationText.text = $"<size=70%>ENTREGAR EN</size>\n{zone}";

        if (destinationIcon != null)
        {
            destinationIcon.sprite = marker != null ? marker.Icon : null;
            destinationIcon.enabled = destinationIcon.sprite != null;
        }

        if (levelManager.DeliveriesDone > 0) ShowToast("¡Nuevo pedido!");
    }

    private void UpdateTimer(float timeRemaining)
    {
        float t = Mathf.Max(0f, timeRemaining);
        int seconds = Mathf.CeilToInt(t);
        timerText.text = $"{seconds / 60}:{seconds % 60:00}";
        timerText.color = t <= dangerTime ? dangerColor : t <= warningTime ? warningColor : normalColor;
    }

    private void UpdateDeliveries(int done, int total)
    {
        if (deliveriesText != null) deliveriesText.text = $"Entregas  {done}/{total}";
    }

    private void HandleDelivered() => ShowToast("¡Pedido entregado!");

    private void ShowToast(string message)
    {
        if (toast == null) return;
        toastText.text = message;
        toastTimer = toastDuration;
        toast.alpha = 1f;
    }

    private void HandleLevelCompleted()
    {
        ShowEndPanel("¡Nivel completado!",
            $"Entregaste {levelManager.DeliveriesDone} pedidos por el Centro Histórico.\n¡Arequipa te lo agradece, causa!",
            levelManager.HasNextLevel);
    }

    private void HandleLevelFailed()
    {
        ShowEndPanel("¡Se acabó el tiempo!",
            $"El pedido llegó frío. Entregas: {levelManager.DeliveriesDone}/{levelManager.DeliveriesToComplete}.\nUsa la flecha y el minimapa para encontrar la ruta.",
            false);
    }

    private void ShowEndPanel(string title, string body, bool showNext)
    {
        if (toast != null) { toast.alpha = 0f; toastTimer = 0f; }
        endTitleText.text = title;
        endBodyText.text = body;
        if (nextButton != null) nextButton.gameObject.SetActive(showNext);
        endPanel.SetActive(true);
    }
}
