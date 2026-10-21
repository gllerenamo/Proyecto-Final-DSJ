using System;
using UnityEngine;

/// HU-04: Genera un pedido con destino aleatorio y un temporizador visible.
/// No dibuja UI directamente (eso lo arma Gabriel); expone eventos y propiedades
/// para que un script de UI (Text / TMP_Text) se suscriba fácilmente.
public class OrderManager : MonoBehaviour
{
    [Header("Destinos posibles")]
    [SerializeField] private Transform[] possibleDestinations;

    [Header("Configuración del pedido")]
    [SerializeField] private float orderTimeLimit = 60f; // segundos

    [Header("Detección de entrega")]
    [SerializeField] private float deliveryRadius = 1f;
    [SerializeField] private Transform player;

    public Transform CurrentDestination { get; private set; }
    public float TimeRemaining { get; private set; }
    public bool OrderActive { get; private set; }

    // Para que la UI de Gabriel actualice el texto del temporizador.
    public event Action<float> OnTimeUpdated;
    public event Action OnOrderDelivered;
    public event Action OnOrderExpired;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        GenerateNewOrder();
    }

    private void Update()
    {
        if (!OrderActive) return;

        TimeRemaining -= Time.deltaTime;
        OnTimeUpdated?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            OrderActive = false;
            OnOrderExpired?.Invoke();
            return;
        }

        if (player != null && CurrentDestination != null)
        {
            float dist = Vector2.Distance(player.position, CurrentDestination.position);
            if (dist <= deliveryRadius)
            {
                OrderActive = false;
                OnOrderDelivered?.Invoke();
            }
        }
    }

    public void GenerateNewOrder()
    {
        if (possibleDestinations == null || possibleDestinations.Length == 0)
        {
            Debug.LogWarning("OrderManager: no hay destinos asignados. Gabriel debe llenar 'possibleDestinations' con los puntos de entrega del mapa.");
            return;
        }

        // Evita repetir el destino recién entregado: el jugador sigue dentro del radio
        // y el nuevo pedido se completaría al instante.
        int index = UnityEngine.Random.Range(0, possibleDestinations.Length);
        if (possibleDestinations.Length > 1 && possibleDestinations[index] == CurrentDestination)
            index = (index + UnityEngine.Random.Range(1, possibleDestinations.Length)) % possibleDestinations.Length;
        CurrentDestination = possibleDestinations[index];
        TimeRemaining = orderTimeLimit;
        OrderActive = true;
    }
}
