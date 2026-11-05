# Fase 6 — run completa

La Fase 6 incorpora tienda, renovaciones, consumibles, vales, edición de mazo, efectos de cartas y jefes. El catálogo aprobado alcanza **50 reliquias, 12 modificaciones (3 ediciones, 6 mejoras, 3 sellos), 20 consumibles, 12 vales y 10 jefes**. La propuesta y aprobación están en PHASE_6_CONTENT_PROPOSAL.md.

## Funcionamiento

- La tienda se abre al superar una meta. Tres pestañas separan reliquias, consumibles y vales. El efecto se lee en cada oferta. Renovar cambia las tres pestañas y aumenta su coste; no repite reliquias o vales adquiridos. Los consumibles pueden volver a aparecer. Las reliquias compradas ocupan automáticamente el primer hueco libre; con todos los huecos ocupados, la compra queda desactivada.
- El oro es independiente de las fichas de blackjack. Los vales modifican precios, interés, recompensa de ronda y recuperación de fichas. Los requisitos de vales se comprueban antes de cobrar. La recompensa solo se concede una vez por ronda.
- **Mazo** muestra 18 cartas por página, permite seleccionar una carta y confirmar el uso de un consumible entre manos. Una operación inválida no gasta el objeto. Se puede tirar un consumible entre manos. Cada copia tiene identidad independiente y conserva sus capas al duplicarse.
- Edición, mejora y sello son independientes; cambiar una capa mantiene las otras. Sus bonos se aplican antes de las reliquias y solo en manos que puntúan. Eco repite los bonos de edición y mejora una vez. La banca usa el mismo mazo, sin recibir los bonos de sus cartas.
- La edición manual mantiene entre 24 y 104 cartas. Al llegar a 24, Vidrio/Ceniza consumen su mejora en lugar de borrar otra carta: el bono no queda permanente y el mazo permanece jugable. Si el zapato de una mano se agota, se puede plantar y la banca se planta con las cartas disponibles.
- La colección pagina 15 reliquias por pantalla. Se pueden ordenar con los botones o arrastrando los iconos de sus huecos entre manos. Los vales amplían de 4 a 6 huecos. La bolsa de consumibles aumenta de 2 a 4.
- Cada tercera ronda presenta un jefe. El catálogo se amplía por mesa. Al cambiar de ronda se restauran reglas normales; límites de manos de jefe prevalecen sobre los vales. La meta se multiplica cuando el jefe lo especifica.
- La run no tiene ronda final. Termina por no alcanzar una meta o quedarse sin apuesta válida. Reiniciar limpia el mazo, consumibles, vales, oro, reliquias y reglas temporales. Se conserva la mejor mano de la run para su resumen.

## Archivos principales

`RunDeck`, `RunInventory`, `CardModificationDefinition`, `ConsumableDefinition`, `VoucherDefinition`, `CardModificationEngine` y `RelicEngine` implementan contenido y efectos. `BlackjackSession` admite un mazo de instancias y restricciones temporales. `RunShop`, `DeckWorkshop`, `RelicRack` y `BossEncounterView` los presentan. Las definiciones están en `Assets/Resources/{Relics,CardModifications,Consumables,Vouchers,Bosses}`; los iconos provisionales en `Assets/_Placeholders/Phase6`.

Se amplió el núcleo anterior para distinguir copias de cartas; la igualdad incluye InstanceId. Las pruebas de reglas que construyen cartas sin identidad conservan su comportamiento previo. El guion de generación aprobado está en `tools/generate_phase6_content.py`; conserva GUIDs, pero sobrescribe los valores del lote generado, por lo que no debe ejecutarse después de ajustar manualmente esos assets sin actualizar el guion.

## Cómo probar

1. Abrir MainMenu, elegir una de tres reliquias iniciales y empezar una run. Pedir, plantarse, doblar y dividir cuando sean válidos; seguro al mostrar as la banca.
2. Superar una meta, comprar en cada pestaña y comprobar oro, capacidad y oferta agotada. Verificar equipamiento automático y compra desactivada cuando no quedan huecos.
3. Abrir Mazo, elegir carta, elegir consumible y Aplicar. Confirmar que solo desaparece el objeto usado y que la carta conserva las capas anteriores. Pasar página y volver a la mesa.
4. Arrastrar dos reliquias para intercambiarlas. Durante una mano, comprobar que no permite modificar la build.
5. Llegar a ronda 3, leer el jefe y comprobar su regla; continuar hasta ronda 4 para comprobar su retirada.
6. Perder, revisar el resumen y reintentar. Verificar oro, mazo de 52 cartas y una nueva selección entre tres reliquias, con los huecos inicialmente vacíos.

## Calidad y límites de esta entrega

Las pruebas de dominio cubren catálogo, duplicación, shuffle determinista, capas, retrigger, debuffs, límites de mazo, transacciones, requisitos, capacidad, reinicio, jefes y fin de mano sin cartas. Las pruebas PlayMode recorren compra, aplicación, paginación, siguiente ronda, menús y derrota. El informe final está en PORTFOLIO_RELEASE.md; la simulación de 2000 runs en PHASE_6_SIMULATION.md.

Los iconos son provisionales aprobados. El balance es inicial: la simulación usa una política sencilla, no optimiza builds. Guardado de runs, metaprogresión y logros siguen en Fase 7; no se afirma que estén implementados. No se necesita arte nuevo para ejecutar esta fase. Los PNG de reemplazo están especificados en la propuesta; música y arte definitivo se pueden integrar durante el pulido posterior.
