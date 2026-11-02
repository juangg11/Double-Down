# Arquitectura prevista

Esta fase establece carpetas, dependencias y límites. Los sistemas de juego se implementarán en sus fases correspondientes.

## Capas

- **Core:** máquina de estados, bus de eventos tipado y reloj/semilla de run. No depende de UI ni de Unity API salvo adaptadores.
- **Blackjack:** mazo, mano, acciones, crupier y reglas configuradas por ScriptableObject. Sus pruebas serán puras de presentación.
- **Scoring:** `BigNumber`, modificadores en orden, trazas de activación y acumulación de meta.
- **Content:** definiciones ScriptableObject de cartas, reliquias, consumibles, jefes, vales, curvas de meta y balance. Las definiciones no guardan estado mutable de run.
- **Infrastructure:** guardado JSON con `schemaVersion`, migraciones, RNG determinista y pools.
- **Presentation:** control de cartas, UI, audio, VFX y animación; consume eventos y presenta instantáneas de estado.

## Flujo previsto

`RunStateMachine → BlackjackRound → ScoringPipeline → RoundResult → Shop/NextRound`, con eventos de dominio inmutables publicados tras cada transición. El pipeline de puntuación aplicará primero fichas base, luego bonos de carta, luego reliquias de izquierda a derecha, luego multiplicadores globales, y finalmente redondeo/visualización. El orden exacto se fijará y probará en Fase 3 antes de añadir contenido.

## Datos y determinismo

Cada run conservará semilla, estado del RNG, configuración de reglas, mazo y orden de reliquias. Los efectos visuales usarán otro RNG, para no alterar el resultado de una run. Las referencias a contenido se guardarán por ID estable, no por nombre de asset ni posición en una lista.

## Carpetas

```text
Assets/
  _Game/Runtime/{Core,Blackjack,Scoring,Content,Presentation,Infrastructure}/
  _Game/Editor/
  _Game/Tests/EditMode/
  _Game/Scenes/
  _Game/Settings/
  _Placeholders/          # PH_*; solo arte y audio temporales
  Art/{Cards/Kenney,UI}/
  Audio/{SFX,Music}/
  Settings/               # URP 2D
  Shaders/
docs/
```

`DoubleDown.Runtime`, `DoubleDown.Editor`, `DoubleDown.Tests.EditMode` y `DoubleDown.Tests.PlayMode` separan código de juego, construcción de escenas y pruebas. Los datos de contenido y guías para añadir reliquias, ediciones y jefes se documentarán cuando existan esos sistemas.

## Render e input

- Unity `6000.3.26f1`, Windows x64, URP 2D.
- Resolución lógica `640×360`, `64 PPU`, escalado entero mediante Pixel Perfect Camera. En proporciones distintas de 16:9 se mantienen barras; una expansión controlada del fondo se evaluará en Fase 4.
- El renderer 2D está basado en el template oficial de Unity 6 instalado localmente. Se usa Pixel Perfect Camera de URP, con `UpscaleRenderTexture`, `Windowbox` y filtro Point. La escena de arranque tiene solo cámara.
- `DoubleDownInput.inputactions`: mapa `Table` con `Point`, `Select`, `Cancel`, `Confirm`; Input System como único sistema activo.
- TextMeshPro llega mediante `com.unity.ugui` en Unity 6.
- La animación de Fase 1 reside en `FeelCardView`: muelle amortiguado, curvas ScriptableObject y temporizadores por carta. Cada carta controla sus propias transiciones, sin dependencia de DOTween ni código de la referencia.

## Núcleo de blackjack (Fase 2)

`BlackjackSession` es la máquina de estados y única autoridad sobre mazo, acciones, apuestas y pagos. `BlackjackRulesConfig` es el ScriptableObject de parámetros; `BlackjackRules` copia sus valores al crear la sesión. `BlackjackDeck` usa un generador con semilla propia; la UI y los sonidos no consumen ese RNG. `BlackjackEvent` anuncia reparto, cambio de mano, revelación y cierre.

`BlackjackTableController` consume el estado y los eventos, construye el HUD con uGUI Text y la fuente bitmap pixelart, activa botones según acciones legales y reutiliza `BlackjackCardVisual` para animar cartas. Las reglas no conocen sprites, Canvas ni Input System. La puntuación incremental de Fase 3 consumirá los resultados de mano sin alterar el cálculo de blackjack.

## Mesa y economía actuales (inicio de Fase 6)

`TableLayout` reserva rectángulos lógicos a 640×360 para la información de la run, las reliquias, las cartas y las acciones. `BlackjackTableController` construye el HUD y utiliza esas posiciones para las cartas. `RelicRack` mantiene cuatro huecos en orden de izquierda a derecha y consulta la propiedad de cada reliquia antes de equiparla.

La transición implementada es `meta superada → tienda → siguiente ronda`. `RunShop` adapta UI a `RunEconomy`, que es C# puro: inventario por ID, oro, ofertas y transacciones. `EconomyBalanceConfig` proporciona parámetros como ScriptableObject; el dominio recibe una copia inmutable de reglas. La tienda usa un RNG propio con la semilla de run, sin consumir azar del mazo ni de efectos visuales. Reabrir una ronda no duplica premios ni reinicia precios.

El guardado del inventario y del estado RNG, los jefes y la edición del mazo todavía no están implementados. La arquitectura prevista arriba es una guía, no una afirmación de que esos sistemas ya existan.
