# Simulacion de Fase 6

2000 runs deterministas: 1000 sin compras y 1000 con compras. Limite de observacion: 60 rondas; no es un limite del juego. Politica: pedir hasta 17, doblar en 11, sin seguro ni division. Compras heuristicas, no jugador optimo.

| Politica | Ronda media | Ronda maxima | Runs que llegan a ronda 10 |
|---|---:|---:|---:|
| Sin compras | 2,98 | 9 | 0 |
| Con compras | 3,17 | 12 | 3 |

Las compras eligen reliquias de la oferta, vales disponibles y consumibles; editan cartas al azar con la misma semilla. Comprueba ausencia de bloqueos y una aproximacion a progresion, no certifica balance de lanzamiento ni ausencia de builds dominantes. El balance final y las politicas avanzadas corresponden a Fase 9.
