using UnityEngine;

/// <summary>
/// Rota suavemente el sprite del auto hacia la dirección en la que se desplaza.
/// No afecta la posición física ni las colisiones de Rigidbody2D.
/// </summary>
public class CarVisuals : MonoBehaviour
{
    [Header("Configuración de rotación")]
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private Transform visualTransform;

    private PlayerController playerController;
    private float targetAngle = 0f;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        if (visualTransform == null) visualTransform = transform;
    }

    private void Update()
    {
        Vector2 velocity = playerController != null ? playerController.CurrentVelocity : Vector2.zero;

        if (velocity.sqrMagnitude > 0.05f)
        {
            // Calcula ángulo hacia donde se mueve el auto (0 = derecha, 90 = arriba, 180 = izquierda, -90 = abajo)
            targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        }

        // Suaviza la rotación visual
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        visualTransform.rotation = Quaternion.Lerp(visualTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
