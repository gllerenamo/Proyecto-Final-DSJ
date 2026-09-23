using System;
using UnityEngine;

/// HU-01: Movimiento del jugador con WASD en 8 direcciones.
/// Requiere: Rigidbody2D (Gravity Scale = 0) + Collider2D en el jugador.
/// Los límites de pistas/verjas se respetan automáticamente si esos objetos
/// tienen Collider2D (no-trigger) en la capa "Obstaculo" definida por Gabriel.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Bocina / Acción evasiva (usado por SeekFleeAgent)")]
    [SerializeField] private KeyCode hornKey = KeyCode.Space;

    // Otros scripts (ej. SeekFleeAgent) escuchan este evento para reaccionar con Flee.
    public static event Action OnPlayerHonk;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        moveInput = new Vector2(h, v).normalized; // 8 direcciones, velocidad constante en diagonal

        if (Input.GetKeyDown(hornKey))
        {
            OnPlayerHonk?.Invoke();
        }
    }

    private void FixedUpdate()
    {
        // Rigidbody2D + colliders del escenario detienen al jugador en pistas/verjas.
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    public Vector2 CurrentVelocity => moveInput * moveSpeed;
}
