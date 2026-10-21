using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Interfaz de inicio simple para 'Delivery Arequipeño'.
/// Controla el panel de menú de inicio con el botón 'Jugar'.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    [Tooltip("Panel principal que contiene el fondo, título y botón")]
    [SerializeField] private GameObject menuPanel;

    [Tooltip("Botón de 'Jugar'")]
    [SerializeField] private Button playButton;

    [Header("Configuración de Juego")]
    [Tooltip("Pausar el tiempo del juego mientras el menú esté visible")]
    [SerializeField] private bool pauseOnStart = true;

    [Tooltip("Permite presionar Enter o Espacio para comenzar")]
    [SerializeField] private bool allowKeyboardStart = true;

    [Tooltip("Permite abrir/cerrar menú con la tecla Escape")]
    [SerializeField] private bool allowEscapeToggle = true;

    [Header("Referencias Opcionales")]
    [SerializeField] private PlayerController player;

    public bool IsMenuOpen => menuPanel != null && menuPanel.activeSelf;

    private void Awake()
    {
        if (player == null)
        {
            player = Object.FindFirstObjectByType<PlayerController>();
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(StartGame);
        }
    }

    private void Start()
    {
        ShowMenu();
    }

    private void Update()
    {
        // Iniciar con Enter o Espacio si el menú está activo
        if (IsMenuOpen && allowKeyboardStart)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                StartGame();
                return;
            }
        }

        // Alternar menú con Escape (pausa/reanudación)
        if (allowEscapeToggle && Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsMenuOpen)
            {
                StartGame();
            }
            else
            {
                ShowMenu();
            }
        }
    }

    /// <summary>
    /// Inicia la partida ocultando el menú y reanudando la simulación.
    /// </summary>
    public void StartGame()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        Time.timeScale = 1f;

        if (player != null)
        {
            player.enabled = true;
        }

        Debug.Log("[Delivery Arequipeño] ¡Partida iniciada! Que comiencen los repartos.");
    }

    /// <summary>
    /// Muestra el menú de inicio y congela la simulación si está configurado.
    /// </summary>
    public void ShowMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }

        if (pauseOnStart)
        {
            Time.timeScale = 0f;
        }

        if (player != null)
        {
            player.enabled = false;
        }
    }
}
