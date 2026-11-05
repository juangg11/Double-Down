# Añadir contenido

Usar **Create → Double Down** en Unity y guardar los ScriptableObjects en la carpeta Resources de su tipo. Elegir ID único y estable, nombre breve, descripción clara e icono. El runtime carga el catálogo por tipo; no necesita listas manuales de IDs.

- Reliquia: RelicDefinition, trigger, operación, cantidad y palo cuando corresponda. AddChips suma base; AddMultiplier suma MULT; MultiplyMultiplier eleva el factor a la cantidad de activaciones. Orden visual de izquierda a derecha. Rareza define su clasificación. Los triggers existentes permiten nuevos contenidos sin C#; un tipo de regla totalmente nuevo requiere extender el evaluador.
- Modificación: CardModificationDefinition, capa, base/MULT/factor/oro, flags especiales y edición visual. Mantener multiplierFactor en 1 si no modifica MULT. Configurar visualEdition 0–3. No aplicar valores negativos.
- Consumible: ConsumableDefinition; elegir operación y, en Modify, referenciar la modificación. El inventario cobra al comprar, gasta al aplicar correctamente y conserva el objeto si no puede usarse.
- Vale: VoucherDefinition; efecto y cantidad. Prerequisite referencia un ID existente. No crear ciclos de requisitos. Se compra una sola vez por run. Máximos de interfaz: seis reliquias y cuatro consumibles.
- Jefe: BossDefinition; primera mesa, ases bajos, banca 17–20, límite de manos, bloqueo de doblar/dividir, palo sin bonos y factor de meta. Componer esos campos permite añadir reglas sin modificar el código.

Precios y premios: `Assets/Resources/EconomyBalance.asset`. Reglas blackjack: `_Game/Settings/BlackjackRules.asset`. Metas: `_Game/Settings/ScoringBalance.asset`. Menú **Double Down → Phase 6 → Simulate 2000 Runs** genera un informe reproducible. Simula mismas semillas con y sin compras; el resultado no sustituye pruebas humanas.

Para cambiar iconos provisionales, conservar el archivo .meta al reemplazar su PNG. No ejecutar el generador sobre definiciones retocadas sin actualizar sus datos. Después de añadir contenido, ejecutar EditMode, PlayMode y una build mediante **Double Down → Portfolio → Build Windows x64**.
