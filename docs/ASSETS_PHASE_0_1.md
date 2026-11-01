# Assets: Fases 0 y 1

## Fase 0

**Ningún archivo adicional requerido.** Ya se han importado del paquete proporcionado 52 caras y un reverso, todos PNG RGBA de `64×64 px`, bajo `Assets/Art/Cards/Kenney/`. Se conserva `KENNEY_LICENSE.txt` (CC0). Las variantes de 16×16 y 32×32 del paquete no se usan en esta fase.

Nombres exactos importados:

```text
card_back.png
card_clubs_02.png       card_diamonds_02.png       card_hearts_02.png       card_spades_02.png
card_clubs_03.png       card_diamonds_03.png       card_hearts_03.png       card_spades_03.png
card_clubs_04.png       card_diamonds_04.png       card_hearts_04.png       card_spades_04.png
card_clubs_05.png       card_diamonds_05.png       card_hearts_05.png       card_spades_05.png
card_clubs_06.png       card_diamonds_06.png       card_hearts_06.png       card_spades_06.png
card_clubs_07.png       card_diamonds_07.png       card_hearts_07.png       card_spades_07.png
card_clubs_08.png       card_diamonds_08.png       card_hearts_08.png       card_spades_08.png
card_clubs_09.png       card_diamonds_09.png       card_hearts_09.png       card_spades_09.png
card_clubs_10.png       card_diamonds_10.png       card_hearts_10.png       card_spades_10.png
card_clubs_A.png        card_diamonds_A.png        card_hearts_A.png        card_spades_A.png
card_clubs_J.png        card_diamonds_J.png        card_hearts_J.png        card_spades_J.png
card_clubs_Q.png        card_diamonds_Q.png        card_hearts_Q.png        card_spades_Q.png
card_clubs_K.png        card_diamonds_K.png        card_hearts_K.png        card_spades_K.png
```

Cada `card_<palo>_<rango>.png` representa una carta. `card_back.png` es el reverso. Los sprites de Kenney son cuadrados; el sandbox de Fase 1 mostrará esta proporción sin deformarlos.

## Fase 1: arte y audio

| Archivo exacto | Tamaño / formato | Descripción |
| --- | --- | --- |
| `CardShadow.png` | 80×80 px, PNG RGBA | Sombra suave para una carta cuadrada de 64×64. |
| `CardSlot.png` | 72×72 px, PNG RGBA | Silueta discreta de la posición de carta en el abanico. |
| `HoverSpark.png` | 16×16 px, PNG RGBA | Destello breve al seleccionar una carta. |
| `SFX_CardHover.wav` | 48 kHz, 16 bit, mono, ≤0,20 s | Confirmación sutil de hover. |
| `SFX_CardPickup.wav` | 48 kHz, 16 bit, mono, ≤0,35 s | Inicio de arrastre. |
| `SFX_CardDrop.wav` | 48 kHz, 16 bit, mono, ≤0,45 s | Soltar y encajar en la mano. |
| `SFX_CardSwap.wav` | 48 kHz, 16 bit, mono, ≤0,35 s | Cruce entre dos posiciones. |
| `SFX_CardFlip.wav` | 48 kHz, 16 bit, mono, ≤0,50 s | Giro para revelar una carta. |
| `SFX_CardDeal.wav` | 48 kHz, 16 bit, mono, ≤0,45 s | Reparto con arco. |
| `SFX_CardSelect.wav` | 48 kHz, 16 bit, mono, ≤0,35 s | Salto/punch de selección. |

En Fase 1 se generaron `PH_CardShadow.png`, `PH_CardSlot.png`, `PH_HoverSpark.png` y `PH_TableFelt.png` (640×360) en `Assets/_Placeholders/`. El audio se cubrió con siete archivos `.ogg` del paquete Kenney UI Audio que proporcionaste, copiados a `Assets/Audio/SFX/Kenney/` con nombres `SFX_CardHover`, `SFX_CardPickup`, `SFX_CardDrop`, `SFX_CardSwap`, `SFX_CardFlip`, `SFX_CardDeal` y `SFX_CardSelect`. Se conserva la licencia CC0 junto a los sonidos. Los nombres WAV anteriores quedan como especificación opcional para sustituirlos por audio propio; los OGG funcionan en Unity sin conversión.

No se requieren imágenes de reliquias, consumibles o vales todavía: antes de definirlos se presentará una lista de propuestas y efectos para aprobación.
