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

`DoubleDown.Runtime` y `DoubleDown.Editor` son los asmdefs iniciales. El asmdef de pruebas se añadirá junto con las pruebas de reglas en Fase 2. Los datos de contenido y guías para añadir reliquias, ediciones y jefes se documentarán cuando existan esos sistemas.

## Render e input

- Unity `6000.3.26f1`, Windows x64, URP 2D.
- Resolución lógica `640×360`, `64 PPU`, escalado entero mediante Pixel Perfect Camera. En proporciones distintas de 16:9 se mantienen barras; una expansión controlada del fondo se evaluará en Fase 4.
- El renderer 2D está basado en el template oficial de Unity 6 instalado localmente. Se usa Pixel Perfect Camera de URP, con `UpscaleRenderTexture`, `Windowbox` y filtro Point. La escena de arranque tiene solo cámara.
- `DoubleDownInput.inputactions`: mapa `Table` con `Point`, `Select`, `Cancel`, `Confirm`; Input System como único sistema activo.
- TextMeshPro llega mediante `com.unity.ugui` en Unity 6.
- La animación de Fase 1 reside en `FeelCardView`: muelle amortiguado, curvas ScriptableObject y temporizadores por carta. Cada carta controla sus propias transiciones, sin dependencia de DOTween ni código de la referencia.
