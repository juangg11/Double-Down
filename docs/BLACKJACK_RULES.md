# Reglas de blackjack de la Fase 2

La mesa usa un mazo estándar de 52 cartas barajado con una semilla determinista. Cada ronda baraja otro mazo derivado de la semilla y el índice de ronda. Los efectos visuales usan un RNG distinto y no cambian el orden de las cartas. La configuración editable está en `Assets/_Game/Settings/BlackjackRules.asset`.

| Regla | Valor inicial |
| --- | --- |
| Fichas iniciales y apuesta mínima | 200 y 10 |
| Blackjack natural | Paga 3:2; el resultado devuelve apuesta + 1,5× apuesta. |
| Banca | Se planta en todos los 17, incluso 17 blando. |
| Seguro | Se ofrece con as visible; cuesta media apuesta y paga 2:1 si la banca tiene blackjack. |
| Doblar | Disponible con dos cartas y fichas suficientes, también tras dividir; roba una carta y termina esa mano. |
| Dividir | Una división máxima, con dos cartas de igual valor de blackjack. Diez, J, Q y K tienen valor equivalente. |
| Ases divididos | Cada mano recibe una sola carta adicional y termina. Un 21 dividido paga como victoria normal, no como blackjack natural. |
| Empate | Recupera la apuesta. |

El as vale 1 u 11 según el mayor total que no supere 21. Un total mayor de 21 pierde la mano y, si todas las manos se pasan, la banca no roba. La banca revisa blackjack antes de las acciones del jugador; cuando muestra un as, espera primero la decisión de seguro.

## Fases del turno

`Ready → Insurance (si procede) → PlayerTurn → DealerTurn → Settled`. Un blackjack natural puede pasar directamente de la apertura a `Settled`. Las acciones inválidas no alteran el estado. Los eventos tipados de `BlackjackSession` informan a la presentación sin mezclar reglas y animación.

La reserva de fichas de esta fase prueba apuestas y pagos de blackjack. La puntuación incremental de **fichas × multiplicador**, metas y manos limitadas se añadirá en Fase 3 sobre el resultado de cada mano. El guardado de runs se desarrollará en su fase correspondiente.
