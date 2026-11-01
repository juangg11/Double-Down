# Simulación de balance — Fase 3

1.000 runs deterministas, apuesta mínima, política simple: doblar en 11, pedir hasta 17, plantarse desde 17, sin seguro ni división.
Configuración: meta inicial 120, crecimiento 1,18, 4 manos por ronda.

| Ronda | Runs que llegan | Runs que superan meta | Éxito condicional | Puntuación media al salir |
| --- | ---: | ---: | ---: | ---: |
| 1 | 1000 | 581 | 58,1% | 117,2 |
| 2 | 581 | 302 | 52,0% | 125,2 |
| 3 | 302 | 106 | 35,1% | 134,7 |
| 4 | 106 | 16 | 15,1% | 124,3 |
| 5 | 16 | 2 | 12,5% | 134,9 |

La simulación sirve para calibrar metas; no representa una estrategia óptima ni reliquias futuras.
