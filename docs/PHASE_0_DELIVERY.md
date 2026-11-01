# Entrega de Fase 0

## Hecho

- Estudio del código y licencia de `mixandjam/Balatro-Feel`, con diferencias y adaptación detalladas en `FEEL_ANALYSIS.md`.
- Proyecto Unity 6.3.26f1 con URP 2D, Input System, uGUI/TextMeshPro y Pixel Perfect Camera a 640×360, 64 PPU.
- Estructura de carpetas, asmdefs, `.gitignore`, mapa de entrada y escena de arranque vacía.
- 52 caras y un reverso de Kenney importados como sprites Point, sin mipmaps ni compresión.
- Arquitectura y decisiones documentadas. Sin lógica de juego ni definiciones de ítems.

## Abrir y probar

1. Abre la carpeta raíz de `DoubleDown` en Unity Hub con Unity 6.3.26f1.
2. Abre `Assets/_Game/Scenes/Bootstrap.unity` y pulsa Play: debe verse solo el fondo oscuro de la cámara.
3. En Project Settings comprueba `Graphics > Scriptable Render Pipeline Settings = UniversalRP` e `Input Manager > Active Input Handling = Input System Package (New)`.
4. Selecciona la cámara y comprueba `Assets Pixels Per Unit = 64` y `Reference Resolution = 640×360`.
5. Selecciona una carta de `Assets/Art/Cards/Kenney` y comprueba `Sprite (2D and UI)`, `Filter Mode = Point`, `Compression = None`, `Generate Mip Maps = off`.
6. Comprueba que Console no contiene errores ni warnings del proyecto al terminar la importación.

## Checklist manual

- [ ] Proyecto abre sin paquetes faltantes.
- [ ] La escena Bootstrap entra en Play sin errores.
- [ ] La cámara tiene Pixel Perfect Camera y usa 640×360.
- [ ] URP 2D está asignado.
- [ ] Input System nuevo está activo.
- [ ] Las 53 imágenes se importan como sprites Point.
- [ ] Los 53 sprites conservan transparencia y no muestran filtrado borroso.
- [ ] El proyecto mantiene 16:9 sin deformación; otras relaciones muestran barras.

## Problemas conocidos y alcance

- La escena aún no contiene cartas interactivas, UI, SFX ni gameplay: son fases posteriores.
- Las cartas de Kenney miden 64×64 y no tienen proporción de naipe clásico. El sandbox de Fase 1 respetará su proporción.
- El objetivo de 60 FPS se medirá con la escena de Fase 1; una escena vacía no sirve para validarlo.

## Siguiente fase

La lista exacta de assets de Fase 1 está en `ASSETS_PHASE_0_1.md`. El arte y audio pueden llegar más tarde porque hay placeholders previstos. La Fase 1 requiere confirmación explícita antes de comenzar.
