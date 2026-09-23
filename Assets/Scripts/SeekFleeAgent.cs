using UnityEngine;

/// HU-03: Obstáculo dinámico interactivo (ej. perro callejero, tráfico imprudente).
/// Estados: Wander (por defecto) -> Seek (persigue al jugador) -> Flee (huye si el
/// jugador usa la bocina o hace una acción evasiva rápida).
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(WanderAgent))]
public class SeekFleeAgent : MonoBehaviour
{
    private enum AgentState { Wander, Seek, Flee }

    [Header("Referencias")]
    [SerializeField] private Transform player;

    [Header("Seek")]
    [SerializeField] private float detectionRadius = 4f;
    [SerializeField] private float seekSpeed = 3f;

    [Header("Flee")]
    [SerializeField] private float fleeSpeed = 4f;
    [SerializeField] private float fleeDuration = 1.5f;

    private Rigidbody2D rb;
    private WanderAgent wanderAgent;
    private AgentState state = AgentState.Wander;
    private float fleeTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wanderAgent = GetComponent<WanderAgent>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    private void OnEnable() => PlayerController.OnPlayerHonk += HandlePlayerHonk;
    private void OnDisable() => PlayerController.OnPlayerHonk -= HandlePlayerHonk;

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case AgentState.Wander:
                if (distanceToPlayer <= detectionRadius) state = AgentState.Seek;
                break;

            case AgentState.Seek:
                if (distanceToPlayer > detectionRadius * 1.3f) state = AgentState.Wander; // histéresis simple
                break;

            case AgentState.Flee:
                fleeTimer -= Time.deltaTime;
                if (fleeTimer <= 0f) state = AgentState.Wander;
                break;
        }
    }

    private void FixedUpdate()
    {
        wanderAgent.enabled = (state == AgentState.Wander);

        switch (state)
        {
            case AgentState.Seek:
                Vector2 desiredSeek = ((Vector2)player.position - rb.position).normalized * seekSpeed;
                rb.MovePosition(rb.position + desiredSeek * Time.fixedDeltaTime);
                break;

            case AgentState.Flee:
                Vector2 desiredFlee = (rb.position - (Vector2)player.position).normalized * fleeSpeed;
                rb.MovePosition(rb.position + desiredFlee * Time.fixedDeltaTime);
                break;

            case AgentState.Wander:
                // WanderAgent maneja su propio FixedUpdate mientras está enabled.
                break;
        }
    }

    private void HandlePlayerHonk()
    {
        float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;
        if (distanceToPlayer <= detectionRadius * 1.5f)
        {
            state = AgentState.Flee;
            fleeTimer = fleeDuration;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
