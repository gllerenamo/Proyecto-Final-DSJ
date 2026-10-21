using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menú de inicio de 'Delivery Arequipeño' (escena MainMenu).
/// 'Jugar' carga el primer nivel; 'Salir' cierra el juego.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    [Header("Configuración")]
    [SerializeField] private string firstLevelScene = "Nivel1";

    [Tooltip("Permite presionar Enter o Espacio para comenzar")]
    [SerializeField] private bool allowKeyboardStart = true;

    private bool loading;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (playButton != null) playButton.onClick.AddListener(StartGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        if (!allowKeyboardStart) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            StartGame();
        else if (Input.GetKeyDown(KeyCode.Escape))
            QuitGame();
    }

    public void StartGame()
    {
        if (loading) return;
        loading = true;
        SceneManager.LoadScene(firstLevelScene);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
