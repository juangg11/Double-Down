# Sandbox de game feel

Abre `Assets/_Game/Scenes/FeelSandbox.unity` y pulsa Play. No contiene reglas de blackjack; sirve para aprobar la respuesta táctil antes de integrar el juego.

## Controles

| Acción | Control |
| --- | --- |
| Hover e inclinación | Mover el cursor sobre una carta. |
| Seleccionar/deseleccionar | Clic izquierdo breve. |
| Arrastrar y reordenar | Mantener clic izquierdo y cruzar el centro de otra posición. |
| Voltear carta enfocada o seleccionada | `F` o `Enter`; también botón del panel. |
| Repartir de nuevo | `D`; también botón del panel. |
| Mostrar 7 cartas | `R`; también botón del panel. |
| Mostrar 20 cartas | `T`; también botón del panel. |
| Limpiar selección | `Esc` o clic derecho. |

## Parámetros

El panel de Play ajusta muelle, amortiguación, separación, abanico, lift, escala, tilt, sway y duración de reparto/volteo. `Reducir movimiento` disminuye tilt, arco y lift. `Silenciar SFX` apaga los siete sonidos. Los cambios del panel afectan solo a la copia del perfil creada en Play; el asset original no se altera.

El panel incluye presets que cambian `fanCurve` y `dealCurve` en Play. El asset `Assets/_Game/Settings/FeelProfile.asset` expone ambas curvas en el Inspector para guardar un perfil definitivo entre sesiones. El perfil inicial prioriza respuesta rápida y amplitud moderada; los valores finales requieren evaluación manual.

## Implementación

- `FeelSandboxController` usa el Input System, gestiona la mano lógica y crea una sola vez pools de 20 cartas, 20 slots, 56 destellos y 8 fuentes de audio.
- `FeelCardView` mantiene por carta destino lógico, posición con muelle, velocidad, estado de hover/selección/arrastre, reparto y flip. El visual sigue al estado sin modificar la lista lógica.
- El reparto usa una curva configurable, arco y squash de aterrizaje. El volteo reduce el ancho a casi cero y cambia el sprite al cruzar la mitad.
- El abanico usa curva ScriptableObject; reordenar mueve la carta en la lista y las demás vuelven a su slot por muelle.
- Las sprites se importan con filtro Point y la cámara URP usa 640×360, `UpscaleRenderTexture`, `Windowbox` y filtro Point.
- Los siete SFX son de Kenney UI Audio, CC0; cada reproducción tiene pequeña variación de pitch.

## Límites de Fase 1

El panel es IMGUI de depuración y no representa la UI final. La escena muestra naipes cuadrados porque así es el paquete suministrado. La calidad subjetiva del tacto y los 60 FPS con 20 cartas requieren prueba manual en el equipo objetivo; la prueba automatizada comprueba la inicialización y el reparto, no sustituye esa evaluación.
