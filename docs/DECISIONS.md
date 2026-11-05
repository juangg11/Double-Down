# Decisiones

| Fecha | Decisión | Motivo |
| --- | --- | --- |
| 2026-11-01 | Unity 6.3.26f1, Windows x64, 640×360 y 64 PPU | Preferencias confirmadas; versión instalada localmente. |
| 2026-11-01 | Blackjack natural 3:2; banca se planta en 17 blando | Preferencia confirmada; la implementación de reglas corresponde a Fase 2. |
| 2026-11-01 | URP 2D y barras para relaciones distintas de 16:9 | Evita deformar cartas y mantiene escalado entero; se revisará con la UI de Fase 4. |
| 2026-11-01 | Animación equivalente propia en vez de DOTween | Evita copiar el proyecto de referencia o incorporar un paquete externo; Fase 1 debe validar la calidad del resultado. |
| 2026-11-01 | Sprites Kenney de 64×64 importados como sprites Point | El paquete suministrado tiene cartas cuadradas de 64×64, no cartas rectangulares. El marco de mesa será independiente. |
| 2026-11-01 | No definir reliquias ni otros ítems todavía | El encargo exige presentar efectos e iconos al usuario antes de definirlos; corresponde a Fase 5. |
| 2026-11-01 | Sustituir Pixel Perfect Camera clásico por el componente de URP | Unity mostraba incompatibilidad en Inspector. El componente URP con `UpscaleRenderTexture`, `Windowbox` y filtro Point conserva 640×360 sin el aviso. Es una corrección necesaria de Fase 0 realizada durante Fase 1. |
| 2026-11-01 | Una división máxima; diez, J, Q y K forman pareja por valor | Mantiene completa la acción de dividir con un tablero legible a 640×360; la configuración está en ScriptableObject. |
| 2026-11-01 | Ases divididos reciben una carta y el 21 dividido no es blackjack natural | Regla explícita para evitar pagos 3:2 inesperados tras dividir. |
| 2026-11-01 | Canvas de la mesa en espacio mundo a 640×360 | Alinea TextMeshPro y botones con la cámara Pixel Perfect y las barras laterales. |
| 2026-11-01 | TextMeshPro Essentials oficial incluido en el repositorio | La mesa abre con fuente y materiales resueltos sin importar assets manualmente. |
| 2026-11-02 | Reorganizar la mesa en columna de run, fila de reliquias, dos bandas de cartas y acciones inferiores | Corrección solicitada por el usuario para eliminar solapamientos; las manos divididas conservan grupos separados. |
| 2026-11-02 | Tienda sustituye la zona central entre rondas | Evita apilar otro panel sobre las cartas; mantiene visibles oro y build. |
| 2026-11-02 | Oro separado de las fichas de apuesta; cuatro reliquias iniciales | Conserva las reglas de blackjack y la build inicial mientras se introduce adquisición real; balance provisional configurable. |
| 2026-11-02 | Renovación 2, 3, 4... por tienda; premio por manos restantes | Implementa coste creciente y recompensa de eficiencia; valores en EconomyBalance.asset. |
| 2026-11-02 | Superficies mates con sprites del pack Crusenho y botones sin aclarado ni escala al hover | Revisión solicitada por el usuario; se conserva respuesta de un píxel y audio, con texto a escala fija. |
| 2026-11-02 | Primeros jefes: ases bajos, banca en 18 y dos manos; aviso antes de repartir | Reglas previstas en el encargo original, con el icono provisional ya disponible. Las restricciones expiran al terminar la ronda y no reinician el saldo. |


## Fase 6 y edición de portfolio (noviembre 2026)

Se conserva el zapato compartido de blackjack: los cambios del mazo afectan también a las cartas que recibe la banca, pero sus efectos de puntuación son exclusivos del jugador. Las cartas duplicadas reciben identidades nuevas. El mínimo de 24 evita mazos inutilizables; al llegar a él, una mejora destructiva se consume en lugar de eliminar la carta. La edición de portfolio incluye menú y derrota antes de la Fase 8 para poder distribuir un recorrido completo. No incluye guardado de runs ni metaprogresión de Fase 7.

En la versión 0.1.1 se eliminan las cuatro reliquias iniciales fijas. Cada run ofrece tres opciones gratuitas y equipa la elegida; las compras posteriores se equipan en el primer hueco libre. Las ofertas quedan desactivadas si faltan oro o huecos. El botón de idioma del menú guarda la preferencia y cambia los textos de interfaz y las explicaciones de efectos; los nombres propios de objetos siguen en español.
