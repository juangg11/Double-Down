# Sandbox de game feel

Abre `Assets/_Game/Scenes/FeelSandbox.unity` y pulsa Play. No contiene reglas de blackjack; sirve para aprobar la respuesta táctil antes de integrar el juego.

## Controles

| Acción | Control |
| --- | --- |
| Hover y giro suave | Mover el cursor sobre una carta. |
| Seleccionar/deseleccionar | Clic izquierdo breve. |
| Arrastrar y reordenar | Mantener clic izquierdo y cruzar el centro de otra posición. |
| Voltear carta enfocada o seleccionada | `F` o `Enter`; también botón del panel. |
| Repartir de nuevo | `D`; también botón del panel. |
| Mostrar 7 cartas | `R`; también botón del panel. |
| Mostrar 20 cartas | `T`; también botón del panel. |
| Limpiar selección | `Esc` o clic derecho. |
| Mostrar/ocultar ajustes | `F1`. |

## Parámetros

El panel de Play se abre con `F1` y tiene desplazamiento para caber a 640×360. Ajusta muelle, amortiguación, separación, abanico, lift, escala, giro 2D, sway y duración de reparto/volteo. `Reducir movimiento` disminuye giro, arco y lift. `Silenciar SFX` apaga los siete sonidos. Los cambios del panel afectan solo a la copia del perfil creada en Play; el asset original no se altera.

El panel incluye presets que cambian `fanCurve` y `dealCurve` en Play. El asset `Assets/_Game/Settings/FeelProfile.asset` expone ambas curvas en el Inspector para guardar un perfil definitivo entre sesiones. El perfil inicial prioriza respuesta rápida y amplitud moderada; los valores finales requieren evaluación manual.

## Implementación

- `FeelSandboxController` usa el Input System, gestiona la mano lógica y crea una sola vez pools de 20 cartas, 56 destellos y 8 fuentes de audio.
- `FeelCardView` mantiene por carta destino lógico, posición con muelle, velocidad, estado de hover/selección/arrastre, reparto y flip. El visual sigue al estado sin modificar la lista lógica.
- El reparto usa una curva configurable, arco y squash de aterrizaje. El volteo reduce el ancho a casi cero y cambia el sprite al cruzar la mitad.
- El abanico usa una curva ScriptableObject: centro elevado, extremos más bajos y rotación de ±8,4° para siete cartas. Reordenar mueve la carta en la lista y las demás vuelven a su posición por muelle.
- Las sprites se importan con filtro Point y la cámara URP usa 640×360, `UpscaleRenderTexture`, `Windowbox` y filtro Point.
- Las cartas se muestran a escala entera 2× y permanecen paralelas a la cámara. Un shader de tinta cambia solo los píxeles rojos de los diamantes a ámbar; los corazones siguen rojos. Los PNG originales de Kenney se conservan.
- El fondo es el shader original `CasinoVortex`: cintas cromáticas animadas con deformación de dominio, paleta limitada y muestreo a 320×180 sobre la escena lógica de 640×360. La escena incluye una luz global 2D para que las cartas se vean completas.
- La selección calcula la carta más cercana al centro bajo el puntero, así puede recorrer la mano en ambos sentidos aunque los sprites se solapen. El contorno seleccionado usa una línea fina de un píxel de la textura.
- Los siete SFX son de Kenney UI Audio, CC0; cada reproducción tiene pequeña variación de pitch.

## Límites de Fase 1

El panel es IMGUI de depuración y no representa la UI final. La escena muestra naipes cuadrados porque así es el paquete suministrado. La calidad subjetiva del tacto y los 60 FPS con 20 cartas requieren prueba manual en el equipo objetivo; la prueba automatizada comprueba la inicialización, la geometría de la mano y los materiales, sin sustituir esa evaluación.
