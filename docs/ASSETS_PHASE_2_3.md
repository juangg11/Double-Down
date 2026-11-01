# Assets de Fase 2 y propuesta para Fase 3

Fase 2 y 3 reutilizan los 52 PNG de cartas Kenney, reverso, `CardInk.mat`, `CasinoVortex.mat` y tres OGG de Kenney (`SFX_CardDeal`, `SFX_CardFlip`, `SFX_CardSelect`). La tipografía activa es la fuente bitmap [Thaleah de Tiny Worlds](https://assetstore.unity.com/packages/2d/fonts/free-pixel-font-thaleah-140059), disponible gratis bajo la Standard Unity Asset Store EULA. Se importó desde la caché local de Asset Store. No se necesita arte nuevo para ejecutar la mesa.

Para la presentación de la puntuación de Fase 3, estos archivos serían reemplazos opcionales de placeholders generados en el proyecto:

| Archivo exacto | Formato y tamaño | Uso |
| --- | --- | --- |
| `UI_ChipCounter.png` | PNG RGBA, 24×24 px | Icono junto al total de fichas de la mano. |
| `UI_Multiplier.png` | PNG RGBA, 24×24 px | Icono del multiplicador. |
| `UI_RoundGoalPanel.png` | PNG RGBA, 192×48 px, bordes de 8 px para 9-slice | Marco de meta y progreso de ronda. |
| `UI_ScoreBurst.png` | PNG RGBA, 32×32 px | Destello al cerrar la puntuación. |
| `SFX_ScoreTick.ogg` | OGG mono, 48 kHz, ≤0,25 s | Pulso por carta o modificador contado. |
| `SFX_ScoreTotal.ogg` | OGG mono, 48 kHz, ≤0,8 s | Confirmación de puntuación final. |

No hacen falta iconos de reliquias ni consumibles todavía. Antes de definir esos objetos se presentarán sus efectos y la lista de arte para aprobación, como pide el encargo.
