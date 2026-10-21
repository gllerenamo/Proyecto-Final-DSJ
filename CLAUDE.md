# Delivery Arequipeño — CLAUDE.md

Guía de contexto para agentes IA que trabajen en este repositorio. Léela antes de modificar cualquier archivo.

## Descripción del proyecto

Videojuego 2D top-down de reparto ambientado en una ciudad inspirada en Arequipa. El jugador controla a un repartidor que recoge pedidos y los entrega antes del tiempo límite, esquivando obstáculos y administrando recursos.

Motor: **Unity** (URP, vista superior 2D).  
Lenguaje: **C# (.NET Standard 2.1)**.  
Control de versiones: Git — rama principal `main`.

## Integrantes y áreas de responsabilidad

| Integrante | Área | Artefactos principales |
|---|---|---|
| **Paolo** | Core + IA | `PlayerController`, `WanderAgent`, `SeekFleeAgent`, `OrderManager` |
| **Gabriel** | Diseño de niveles, arte y UI | Mapa, sprites, escenas, UI (HUD, menú) |
| **Alejandro** | Producción, gestión y docs | Tablero Trello/GitHub, documentación SUM, ISO/IEC/IEEE 29148 |

Respeta esta división al proponer cambios: no mezcles responsabilidades entre integrantes en un mismo commit.

## Estado actual del código (Avance 1)

Scripts implementados y funcionales:

- [`Assets/Scripts/PlayerController.cs`](Assets/Scripts/PlayerController.cs) — Movimiento WASD 8 direcciones + evento `OnPlayerHonk` (HU-01).
- [`Assets/Scripts/WanderAgent.cs`](Assets/Scripts/WanderAgent.cs) — Comportamiento Wander de Craig Reynolds (HU-02).
- [`Assets/Scripts/SeekFleeAgent.cs`](Assets/Scripts/SeekFleeAgent.cs) — Máquina de estados Wander→Seek→Flee, reacciona a la bocina (HU-03).
- [`Assets/Scripts/OrderManager.cs`](Assets/Scripts/OrderManager.cs) — Genera pedido con destino aleatorio y temporizador; expone eventos para la UI (HU-04).
- [`Assets/Scripts/DeliveryZoneMarker.cs`](Assets/Scripts/DeliveryZoneMarker.cs) — Marcador visual con efecto de pulso para zonas de entrega.
- [`Assets/Scripts/CameraFollow2D.cs`](Assets/Scripts/CameraFollow2D.cs) — Cámara que sigue al jugador.
- [`Assets/Scripts/CarVisuals.cs`](Assets/Scripts/CarVisuals.cs) — Ayudante visual del vehículo repartidor.
- [`Assets/Scripts/MainMenuUI.cs`](Assets/Scripts/MainMenuUI.cs) — Menú de inicio con pausa de tiempo y soporte teclado/Escape.

Escena activa: `Assets/Scenes/SampleScene.unity`.

## Convenciones de código

- **Idioma de código**: inglés para nombres de variables, métodos y clases. Comentarios en español cuando explican lógica de negocio del juego.
- **Estilo**: convenciones estándar de C#/Unity — `PascalCase` para clases y métodos públicos, `camelCase` para variables privadas, prefijo `_` opcional para campos serializados privados. No uses prefijo `m_`.
- **SerializeField**: usa `[SerializeField] private` en lugar de campos públicos para todo lo que deba aparecer en el Inspector.
- **Headers**: agrupa campos del Inspector con `[Header("Nombre")]`.
- **Eventos**: usa `System.Action` o `UnityEvent` para desacoplar sistemas. No llames directamente entre scripts de distintos sistemas cuando un evento sea suficiente.
- **RequireComponent**: declara dependencias de componentes con `[RequireComponent(typeof(X))]`.
- **Tags**: el jugador lleva el tag `"Player"`. Úsalo para búsquedas por tag, no hard-codes de nombre de objeto.
- **Capas (Layers)**: los colisionadores de límites de pistas/verjas van en la capa `"Obstaculo"` (definida en el proyecto de Gabriel).
- **Comentarios**: solo cuando el *por qué* no es obvio. No documentes el *qué* si el nombre del método ya lo dice.

## Arquitectura de sistemas

```
PlayerController  ──(OnPlayerHonk event)──►  SeekFleeAgent
                                                    │
                                            WanderAgent (component)

OrderManager  ──(events: OnTimeUpdated, OnOrderDelivered, OnOrderExpired)──►  UI scripts
              ──(CurrentDestination property)──►  DeliveryZoneMarker / minimap
```

- Los sistemas se comunican vía **eventos C#** (`event Action`) para mantener bajo acoplamiento.
- La UI no tiene lógica de juego; solo lee propiedades y se suscribe a eventos.
- `WanderAgent` y `SeekFleeAgent` coexisten en el mismo GameObject: `SeekFleeAgent` deshabilita `WanderAgent` cuando toma control.

## Comportamientos IA (Craig Reynolds)

| Comportamiento | Script | Descripción |
|---|---|---|
| Wander | `WanderAgent` | Deambulación autónoma con círculo de wander proyectado |
| Seek | `SeekFleeAgent` (estado `Seek`) | Persigue al jugador cuando entra en `detectionRadius` |
| Flee | `SeekFleeAgent` (estado `Flee`) | Huye del jugador al recibir bocina, durante `fleeDuration` segundos |

Histéresis en Seek→Wander: el agente vuelve a Wander cuando la distancia supera `detectionRadius × 1.3f`.

## Reglas para modificar el proyecto

1. **No romper los eventos públicos** de `PlayerController.OnPlayerHonk` ni de `OrderManager`. Otros sistemas dependen de ellos.
2. **No añadir física 3D** ni cambiar el Rigidbody2D a Rigidbody. Todo el juego es 2D.
3. **No cambiar la escena SampleScene.unity directamente** sin coordinar con Gabriel (Game Design). Los cambios de escena suelen generar conflictos de merge en YAML.
4. **No añadir lógica de UI dentro de scripts de gameplay** (`PlayerController`, `OrderManager`, etc.). La UI va en scripts separados que se suscriben a eventos.
5. **Gravity Scale del Rigidbody2D del jugador = 0**. No lo cambies.
6. **Prioriza alcance acotado**: este es un proyecto académico con entregas cortas. No añadas sistemas completos que no estén en el backlog.

## Backlog (Historias de Usuario activas)

| ID | Historia | Estado |
|---|---|---|
| HU-01 | Movimiento jugador WASD 8 direcciones | Implementado |
| HU-02 | Agentes Wander autónomos | Implementado |
| HU-03 | Obstáculo dinámico Seek/Flee | Implementado |
| HU-04 | Pedido básico con temporizador | Implementado |

Nuevas HU deben acordarse entre los tres integrantes antes de ser implementadas.

## Cómo probar cambios

1. Abrir `Assets/Scenes/SampleScene.unity` en Unity.
2. Asignar en el Inspector las referencias necesarias (ver `[SerializeField]` de cada script).
3. Ejecutar con Play. El tag `"Player"` debe estar en el GameObject del repartidor.
4. Bocina: `Space`. Movimiento: `WASD` o flechas.
5. Verificar que los agentes de Wander no se atasquen y que Seek/Flee responda a la bocina.

## Dependencias externas

- **Unity URP** (Universal Render Pipeline) — configurado en `Assets/Settings/UniversalRP.asset`.
- **TextMesh Pro** — disponible en `Assets/TextMesh Pro/`. Usa `TMP_Text` para cualquier texto de UI nueva.
- Sin paquetes de terceros adicionales hasta la fecha.
