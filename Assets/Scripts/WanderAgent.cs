using UnityEngine;

/// HU-02: Comportamiento "Wander" (Craig Reynolds) para peatones o tráfico menor.
/// El agente deambula de forma autónoma y fluida sin salir de las aceras/pistas
/// (los límites se respetan vía Collider2D no-trigger en el escenario de Gabriel).
[RequireComponent(typeof(Rigidbody2D))]
public class WanderAgent : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float maxSteerForce = 3f;

    [Header("Parámetros de Wander (Reynolds)")]
    [SerializeField] private float wanderRadius = 1.2f;
    [SerializeField] private float wanderDistance = 2f;
    [SerializeField] private float wanderJitter = 40f; // grados por segundo

    private Rigidbody2D rb;
    private Vector2 velocity;
    private float wanderAngle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wanderAngle = Random.Range(0f, 360f);
        velocity = Random.insideUnitCircle.normalized * moveSpeed;
    }

    private void FixedUpdate()
    {
        Vector2 steer = ComputeWanderSteering();
        velocity = Vector2.ClampMagnitude(velocity + steer * Time.fixedDeltaTime, moveSpeed);

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

        if (velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }
    }

    private Vector2 ComputeWanderSteering()
    {
        // Centro del círculo de wander, proyectado delante del agente.
        Vector2 forward = velocity.sqrMagnitude > 0.01f ? velocity.normalized : Vector2.right;
        Vector2 circleCenter = forward * wanderDistance;

        // Jitter aleatorio del ángulo objetivo dentro del círculo.
        wanderAngle += Random.Range(-1f, 1f) * wanderJitter * Time.fixedDeltaTime;
        Vector2 displacement = new Vector2(Mathf.Cos(wanderAngle * Mathf.Deg2Rad), Mathf.Sin(wanderAngle * Mathf.Deg2Rad)) * wanderRadius;

        Vector2 wanderTarget = circleCenter + displacement;
        Vector2 desired = wanderTarget.normalized * moveSpeed;

        return Vector2.ClampMagnitude(desired - velocity, maxSteerForce);
    }

    // Permite que SeekFleeAgent u otro script retome el control temporalmente.
    public void SetVelocity(Vector2 newVelocity) => velocity = newVelocity;
    public Vector2 GetVelocity() => velocity;
}
