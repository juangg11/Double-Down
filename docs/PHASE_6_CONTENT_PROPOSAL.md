# Propuesta de contenido pendiente de Fase 6

Estado: aprobado por el usuario el 3 de noviembre de 2026, incluidos los iconos provisionales. Conserva las 15 reliquias y los 3 jefes existentes. Este lote alcanza 50 reliquias, 12 modificaciones de carta, 20 consumibles, 12 vales y 10 jefes. Los valores son de balance inicial, ajustables en ScriptableObjects.

Los bonos de carta y reliquias solo se aplican en manos que puntúan (victoria, blackjack o empate). La banca usa el mismo mazo editado, pero nunca recibe sus bonos. Se edita entre manos; los consumibles no se gastan si la operación no es válida. Cada carta conserva una edición, una mejora y un sello independientes.

## 35 reliquias nuevas

| ID | Nombre | Efecto |
|---|---|---|
| R15 | Brújula roja | +0,3 MULT por corazón. Rareza: Común |
| R16 | Corte ámbar | +0,3 MULT por diamante. Rareza: Común |
| R17 | Raíz de hierro | +0,3 MULT por trébol. Rareza: Común |
| R18 | Aguja negra | +0,3 MULT por pica. Rareza: Común |
| R19 | As de cristal | +0,8 MULT por as. Rareza: Rara |
| R20 | Mesa real | +0,4 MULT por figura. Rareza: Común |
| R21 | Doble fondo | +24 base al ganar doblando. Rareza: Rara |
| R22 | Pacto gemelo | +1 MULT al ganar una mano dividida. Rareza: Rara |
| R23 | Saldo exacto | +18 base en empate. Rareza: Común |
| R24 | Firma inicial | +20 base al ganar con dos cartas sin doblar. Rareza: Común |
| R25 | Cuenta larga | +0,4 MULT por carta a partir de la tercera. Rareza: Rara |
| R26 | Cadena de cobre | +4 base por victoria consecutiva. Rareza: Común |
| R27 | Borde veintiuno | +30 base al llegar a 21 sin perder. Rareza: Rara |
| R28 | Pulso natural | ×1,5 MULT en blackjack natural. Rareza: Épica |
| R29 | Cinco latidos | ×1,8 MULT con 5 o más cartas sin perder. Rareza: Épica |
| R30 | Paso blando | +1 MULT en una mano blanda que puntúe. Rareza: Rara |
| R31 | Paso duro | +15 base en una mano dura que puntúe. Rareza: Común |
| R32 | Cuatro esquinas | ×1,6 MULT con los cuatro palos en la mano. Rareza: Épica |
| R33 | Tinta única | ×1,4 MULT si todas las cartas comparten palo. Rareza: Épica |
| R34 | Par de salida | +1 MULT si las dos primeras cartas tienen el mismo rango. Rareza: Rara |
| R35 | Número par | +4 base por carta de rango 2,4,6,8 o 10. Rareza: Común |
| R36 | Número impar | +4 base por carta de rango 3,5,7 o 9. Rareza: Común |
| R37 | Escalón bajo | +0,25 MULT por carta de rango 2 a 5. Rareza: Común |
| R38 | Escalón alto | +0,25 MULT por carta de rango 6 a 10. Rareza: Común |
| R39 | Diecinueve | ×1,5 MULT con total 19. Rareza: Rara |
| R40 | Veinte limpio | ×1,35 MULT con total 20. Rareza: Rara |
| R41 | Dieciocho | +2 MULT con total 18. Rareza: Rara |
| R42 | Hilo rojo | ×1,4 MULT si toda la mano es roja. Rareza: Épica |
| R43 | Hilo negro | ×1,4 MULT si toda la mano es negra. Rareza: Épica |
| R44 | Bisagra | ×1,2 MULT en empate. Rareza: Rara |
| R45 | Martillo doble | ×1,6 MULT al ganar doblando. Rareza: Épica |
| R46 | Espejo dividido | ×1,4 MULT al ganar dividida. Rareza: Épica |
| R47 | Resonador | ×1,4 MULT por cada carta a partir de la quinta (potencia). Rareza: Legendaria |
| R48 | Constancia | +2 MULT al ganar. Rareza: Épica |
| R49 | Motor perpetuo | ×2 MULT en toda mano que puntúe. Rareza: Legendaria |

## 12 ediciones, mejoras y sellos

| ID | Nombre | Efecto |
|---|---|---|
| E01 | Lámina | Edición: +12 base por carta |
| E02 | Prisma | Edición: +0,6 MULT por carta |
| E03 | Órbita | Edición: ×1,15 MULT por carta |
| M01 | Carga | Mejora: +20 base por carta |
| M02 | Impulso | Mejora: +1 MULT por carta |
| M03 | Salvaje | Mejora: cuenta como cualquier palo para condiciones; mantiene su valor |
| M04 | Reserva | Mejora: +2 oro al puntuar la carta |
| M05 | Vidrio | Mejora: ×1,5 MULT; se destruye al puntuar |
| M06 | Ceniza | Mejora: +45 base; se destruye al puntuar |
| S01 | Eco | Sello: repite una vez los bonos de edición/mejora de esa carta |
| S02 | Tesoro | Sello: +1 oro al puntuar la carta |
| S03 | Escalera | Sello: +0,2 MULT por la posición de la carta en la mano |

## 20 consumibles

| ID | Nombre | Efecto |
|---|---|---|
| C01 | Tijeras | Quita una carta seleccionada; mínimo 24 cartas |
| C02 | Calco | Duplica la carta seleccionada con sus modificaciones; máximo 104 cartas |
| C03 | Ascenso | Sube un rango la carta seleccionada; K pasa a A |
| C04 | Descenso | Baja un rango; A pasa a K |
| C05 | Tinta rubí | Cambia el palo a corazones |
| C06 | Tinta ámbar | Cambia el palo a diamantes |
| C07 | Tinta hierro | Cambia el palo a tréboles |
| C08 | Tinta obsidiana | Cambia el palo a picas |
| C09 | Lámina | Aplica E01 a la carta seleccionada; reemplaza solo su misma capa |
| C10 | Prisma | Aplica E02 a la carta seleccionada; reemplaza solo su misma capa |
| C11 | Órbita | Aplica E03 a la carta seleccionada; reemplaza solo su misma capa |
| C12 | Carga | Aplica M01 a la carta seleccionada; reemplaza solo su misma capa |
| C13 | Impulso | Aplica M02 a la carta seleccionada; reemplaza solo su misma capa |
| C14 | Salvaje | Aplica M03 a la carta seleccionada; reemplaza solo su misma capa |
| C15 | Reserva | Aplica M04 a la carta seleccionada; reemplaza solo su misma capa |
| C16 | Vidrio | Aplica M05 a la carta seleccionada; reemplaza solo su misma capa |
| C17 | Ceniza | Aplica M06 a la carta seleccionada; reemplaza solo su misma capa |
| C18 | Eco | Aplica S01 a la carta seleccionada; reemplaza solo su misma capa |
| C19 | Tesoro | Aplica S02 a la carta seleccionada; reemplaza solo su misma capa |
| C20 | Escalera | Aplica S03 a la carta seleccionada; reemplaza solo su misma capa |

## 12 vales

| ID | Nombre | Efecto |
|---|---|---|
| V01 | Turno extra | +1 mano en rondas normales; los límites de jefe siguen vigentes |
| V02 | Segundo turno | Otro +1 mano; requiere V01 |
| V03 | Estante | +1 hueco de reliquia (5 en total) |
| V04 | Galería | +1 hueco de reliquia (6); requiere V03 |
| V05 | Bolsa | +1 hueco de consumible (3) |
| V06 | Maletín | +1 hueco de consumible (4); requiere V05 |
| V07 | Socio | −1 oro al precio de reliquias; mínimo 1 |
| V08 | Mayorista | −1 oro al precio de consumibles; mínimo 1 |
| V09 | Ruleta ligera | −1 al coste inicial de renovar; mínimo 1 |
| V10 | Caja de ahorro | Interés de 1 oro por cada 5 ahorrados al superar ronda, máximo 5 |
| V11 | Propina | +2 oro al superar ronda |
| V12 | Seguro de caja | Recupera 20 fichas de blackjack al superar ronda |

## 7 jefes nuevos

| ID | Nombre | Efecto |
|---|---|---|
| B04 | Corazón sellado | Los corazones no aportan bonos de carta ni cuentan para palos; mesa 2 |
| B05 | Diamante sellado | Los diamantes no aportan bonos de carta ni cuentan para palos; mesa 2 |
| B06 | Trébol sellado | Los tréboles no aportan bonos de carta ni cuentan para palos; mesa 3 |
| B07 | Pica sellada | Las picas no aportan bonos de carta ni cuentan para palos; mesa 3 |
| B08 | Sin red | No se puede doblar; mesa 2 |
| B09 | Una sola voz | No se puede dividir; mesa 2 |
| B10 | Presión | Meta ×1,5 y banca en 19; mesa 4 |

## Arte y formato

Solicito autorización para usar iconos provisionales de los packs que ya has suministrado, identificados en Assets/_Placeholders, sin bloquear la implementación por nuevos PNG. Para sustituirlos por arte propio: PNG RGBA 32×32, contorno de 1 píxel, sin texto. Archivo exacto por cada fila: PH_R15.png hasta PH_R49.png; PH_E01.png hasta PH_E03.png; PH_M01.png hasta PH_M06.png; PH_S01.png hasta PH_S03.png; PH_C01.png hasta PH_C20.png; PH_V01.png hasta PH_V12.png; PH_B04.png hasta PH_B10.png. El nombre y el efecto de cada fila son la descripción visual del icono.

La interfaz paginará el catálogo y el mazo para conservar el espacio y la legibilidad a 640×360. La tienda separará reliquias, consumibles y vales en pestañas. No se introducirán todas las tarjetas en un único panel.
