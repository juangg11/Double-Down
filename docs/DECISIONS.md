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
