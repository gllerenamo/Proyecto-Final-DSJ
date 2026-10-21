using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Reglas mínimas de un nivel: entregar N pedidos antes de que venza el temporizador.
/// No dibuja UI; el HUD se suscribe a sus eventos. La carga del siguiente nivel (HU-14)
/// queda expuesta en LoadNextLevel() para la integración.
public class LevelManager : MonoBehaviour
{
    public enum LevelState { Playing, Completed, Failed }

    [Header("Nivel")]
    [SerializeField] private string levelDisplayName = "Nivel 1 · Centro Histórico";
    [SerializeField] private int deliveriesToComplete = 3;
    [SerializeField] private float nextOrderDelay = 1.5f;

    [Header("Referencias")]
    [SerializeField] private OrderManager orderManager;

    [Header("Escenas")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [Tooltip("Vacío si aún no existe el siguiente nivel")]
    [SerializeField] private string nextLevelScene = "";

    public string LevelDisplayName => levelDisplayName;
    public int DeliveriesDone { get; private set; }
    public int DeliveriesToComplete => deliveriesToComplete;
    public LevelState State { get; private set; } = LevelState.Playing;
    public bool HasNextLevel => !string.IsNullOrEmpty(nextLevelScene);

    public event Action<int, int> OnDeliveriesChanged;
    public event Action OnLevelCompleted;
    public event Action OnLevelFailed;

    private void Awake()
    {
        if (orderManager == null) orderManager = FindFirstObjectByType<OrderManager>();
        // El menú o un nivel anterior pudo dejar el tiempo congelado.
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        if (orderManager == null) return;
        orderManager.OnOrderDelivered += HandleOrderDelivered;
        orderManager.OnOrderExpired += HandleOrderExpired;
    }

    private void OnDisable()
    {
        if (orderManager == null) return;
        orderManager.OnOrderDelivered -= HandleOrderDelivered;
        orderManager.OnOrderExpired -= HandleOrderExpired;
    }

    private void Start()
    {
        OnDeliveriesChanged?.Invoke(DeliveriesDone, deliveriesToComplete);
    }

    private void HandleOrderDelivered()
    {
        if (State != LevelState.Playing) return;

        DeliveriesDone++;
        OnDeliveriesChanged?.Invoke(DeliveriesDone, deliveriesToComplete);

        if (DeliveriesDone >= deliveriesToComplete)
        {
            EndLevel(LevelState.Completed);
            return;
        }

        StartCoroutine(NextOrderAfterDelay());
    }

    private void HandleOrderExpired()
    {
        if (State != LevelState.Playing) return;
        EndLevel(LevelState.Failed);
    }

    private IEnumerator NextOrderAfterDelay()
    {
        yield return new WaitForSeconds(nextOrderDelay);
        if (State == LevelState.Playing) orderManager.GenerateNewOrder();
    }

    private void EndLevel(LevelState result)
    {
        State = result;
        Time.timeScale = 0f;
        if (result == LevelState.Completed) OnLevelCompleted?.Invoke();
        else OnLevelFailed?.Invoke();
    }

    public void RestartLevel() => LoadScene(SceneManager.GetActiveScene().name);

    public void GoToMainMenu() => LoadScene(mainMenuScene);

    public void LoadNextLevel()
    {
        if (HasNextLevel) LoadScene(nextLevelScene);
    }

    private static void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
