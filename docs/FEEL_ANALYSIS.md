# Análisis de `mixandjam/Balatro-Feel`

Referencia examinada: [repositorio](https://github.com/mixandjam/balatro-feel), commit `aa2c8acd0202126c788c0ff081692c9b69762961` (MIT, copyright 2024 Mix and Jam). Se estudiaron `Card.cs`, `CardVisual.cs`, `HorizontalCardHolder.cs`, `CurveParameters.cs`, `ShaderCode.cs` y la licencia. No se ha copiado código de ese proyecto a DOUBLE DOWN.

## Técnicas que sí implementa

| Técnica | Implementación de referencia | Adaptación prevista para DOUBLE DOWN |
| --- | --- | --- |
| Hover y selección | [`Card.cs`](https://github.com/mixandjam/balatro-feel/blob/aa2c8acd0202126c788c0ff081692c9b69762961/Assets/Scripts/Card.cs) recibe eventos de puntero; un clic corto alterna selección; [`CardVisual.cs`](https://github.com/mixandjam/balatro-feel/blob/aa2c8acd0202126c788c0ff081692c9b69762961/Assets/Scripts/CardVisual.cs) escala y aplica un golpe angular. | Separar estado de interacción y presentación; hover, selección y arrastre tendrán prioridades explícitas para evitar tweens que compitan. |
| Inclinación 3D e idle sway | `CardVisual.CardTilt` combina desplazamiento del cursor con seno/coseno dependientes del índice, y suaviza con `LerpAngle`. | Usar coordenadas locales de la carta, ángulos acotados y sway determinista por semilla; reducir movimiento será un factor global. |
| Seguimiento y arrastre | La carta lógica se mueve hacia el puntero con velocidad limitada; el visual la sigue con `Lerp`, generando retraso aparente. `FollowRotation` inclina según el desfase. | Implementar un muelle amortiguado real para la capa visual y rotación según velocidad; límite de sobrepaso y retorno suave al soltar. |
| Sombra | La sombra es hija del visual; al pulsar cambia de offset y sorting. | Offset ligado a elevación y tilt, opacidad/escala ligadas a altura; preservar filtro Point. |
| Abanico | [`CurveParameters.cs`](https://github.com/mixandjam/balatro-feel/blob/aa2c8acd0202126c788c0ff081692c9b69762961/Assets/Scripts/_SciptableObjects/CurveParameters.cs) guarda curvas de posición y rotación; `CardVisual` las evalúa según índice normalizado. | `HandLayoutProfile` ScriptableObject con curvas, spacing máximo y zonas seguras para 16:9/ultrawide. |
| Swap | [`HorizontalCardHolder.cs`](https://github.com/mixandjam/balatro-feel/blob/aa2c8acd0202126c788c0ff081692c9b69762961/Assets/Scripts/HorizontalCardHolder.cs) cambia padres de slots al cruzar otra carta y lanza un punch angular. | Reordenar una lista lógica, animar ambos destinos y mantener un orden de dibujo estable; evitar el ajuste artificial de `RectTransform.sizeDelta` usado allí. |
| Escala y curvas | DOTween anima hover, press, selección, swap y retorno (`OutBack`, punch). | Usar un tween ligero propio con curvas/easing configurables en Fase 1, equivalente a DOTween y sin dependencia externa. |
| Shader de carta | `ShaderCode.cs` transfiere la inclinación a `_Rotation` y activa palabras clave de edición. | Crear shaders URP propios en Fase 4; los efectos visuales no definen el estado de la carta. |

## Diferencias importantes frente al encargo

- **Muelle:** el repositorio no calcula una fuerza de muelle; usa `Lerp` para el visual y velocidad limitada para la carta lógica.
- **Reparto, volteo y squash & stretch:** no hay una implementación completa de estas acciones en los scripts examinados. Se diseñarán desde cero en Fase 1.
- **Input:** usa `UnityEngine.Input` y `Camera.main` en varios `Update`. DOUBLE DOWN usará Input System y una referencia cacheada a cámara/rectángulo de mesa.
- **Robustez:** el ejemplo usa `FindObjectOfType`, un `GraphicRaycaster` global que desactiva durante el arrastre y IDs numéricos de tween compartidos. Para muchas cartas o paneles, se sustituirán por referencias inyectadas, captura de puntero por carta y tweens propiedad de cada visual.
- **Resolución:** la referencia procede de Unity 2022.3 y URP 14; este proyecto usa Unity 6.3 y URP 17.

## Criterios de aceptación para Fase 1

1. Hover y selección responden al primer fotograma de entrada y no producen saltos al salir.
2. El visual sigue al arrastre con amortiguación ajustable y vuelve sin vibración persistente.
3. El abanico y el swap mantienen el orden lógico y el visual incluso al arrastrar deprisa.
4. Repartir y voltear conservan silueta pixel perfect al terminar; mientras se animan pueden usar subpíxeles visuales controlados.
5. El panel de depuración permite ajustar velocidades, amplitudes, curvas y reducción de movimiento en Play.
6. El Perfilador muestra 60 FPS en la escena de pruebas con al menos 20 cartas visibles.

## Licencias

La referencia es MIT. Se han reutilizado ideas y patrones, no archivos ni código. Los sprites de Kenney incluidos en el proyecto son CC0; se conserva su texto de licencia en `Assets/Art/Cards/Kenney/KENNEY_LICENSE.txt`.
