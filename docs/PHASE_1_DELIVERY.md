# Entrega de Fase 1

## Hecho

- Escena `FeelSandbox.unity` con 52 sprites de cartas disponibles y siete cartas repartidas al iniciar.
- Hover, tilt 3D, sombra dinámica, sway, selección con punch/destellos, arrastre amortiguado, swap, reparto con arco/rebote y flip.
- Panel de depuración que ajusta parámetros en Play, modo de 20 cartas, reducción de movimiento y mute.
- Siete SFX Kenney CC0 con variación de pitch y pools para cartas, slots, destellos y fuentes de audio.
- Corrección justificada de Fase 0: ambas escenas usan Pixel Perfect Camera de URP, no el componente clásico incompatible.
- Prueba de Play Mode: siete cartas activas y boca arriba tras el reparto.

## Abrir y probar

1. Si Unity ya tenía `Bootstrap.unity` abierto antes de esta entrega, vuelve a abrir una escena desde Project para cargar su versión nueva guardada en disco.
2. Abre `Assets/_Game/Scenes/FeelSandbox.unity`, pulsa Play y espera aproximadamente un segundo al reparto.
3. Prueba los controles de `FEEL_SANDBOX.md` y ajusta sliders mientras observas las cartas.
4. En Test Runner, ejecuta `DoubleDown.Tests.PlayMode > SandboxDealsSevenFaceUpCards`.

## Checklist manual

- [ ] Al iniciar se reparten siete cartas con arco, rebote, flip y sonidos escalonados.
- [ ] Hover eleva, escala, inclina y desplaza la sombra sin saltos visibles.
- [ ] Clic corto selecciona/deselecciona con punch y destellos.
- [ ] Arrastrar sigue al cursor con retraso amortiguado; al soltar vuelve al abanico.
- [ ] Al cruzar otras cartas, el orden cambia una sola vez por cruce y las cartas vecinas se recolocan.
- [ ] `F`/`Enter` voltea la carta enfocada o seleccionada.
- [ ] `R`, `T` y `D` recrean la mano sin errores de consola.
- [ ] Los sliders actúan en vivo; reducción de movimiento y mute funcionan.
- [ ] En Game View 16:9 y ultrawide no hay deformación; aparecen barras cuando corresponde.
- [ ] El Profiler alcanza 60 FPS estables con 20 cartas en el equipo objetivo.

## Verificación automatizada

Compilación en Unity 6.3.26f1 sin warnings de C#. Play Mode Test Runner: 1 prueba ejecutada, 1 superada. El test valida el reparto inicial, la revelación y que no quedan cartas en tránsito.

## Problemas conocidos y alcance

- El resultado táctil, audio percibido y 60 FPS todavía necesitan tu evaluación manual. No se pueden certificar mediante el test de reparto.
- La UI de depuración es IMGUI temporal. Los sprites de Kenney son cuadrados; los marcos de naipe final no existen todavía.
- `Bootstrap.unity` puede quedar con su versión antigua en memoria si Unity estaba abierto al actualizar archivos externos; abrir la escena de nuevo carga la cámara URP corregida.
- Las reglas de blackjack, puntuación, shaders definitivos y contenido de tienda pertenecen a fases posteriores.

## Assets para Fase 2

**Ningún archivo nuevo obligatorio.** El núcleo de blackjack puede usar los naipes y SFX ya incluidos, con UI de prueba generada con componentes de Unity. Si quieres sustituir arte/audio temporal, la convención será `UI_ActionHit.png`, `UI_ActionStand.png`, `UI_ActionDouble.png`, `UI_ActionSplit.png`, `UI_ActionInsurance.png` (cada uno 96×32 px, PNG RGBA) y `SFX_ActionHit.ogg`, `SFX_ActionStand.ogg`, `SFX_ActionDouble.ogg`, `SFX_ActionSplit.ogg`, `SFX_ActionInsurance.ogg` (OGG mono, 48 kHz, ≤0,5 s). No se definirá ningún ítem ni reliquia sin presentarte antes su efecto y pedirte el icono.
