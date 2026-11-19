# LA MYPIME GAME — guía del MVP v11

Simulador de una **MYPIME / minimercado** en Unity 3D para móvil.

> **Importante:** los 8 productos actuales son **provisionales** y se cambiarán por la lista definitiva al final del proyecto. La moneda (CUP), el índice de precios y el consumo eléctrico son **mecánicas ficticias de juego**, no datos económicos reales. La tasa USD/CUP toma **930 CUP** como referencia informativa publicada por **elTOQUE el 18/11/2026** y cambia de forma simulada cada día de partida; el juego **no consulta internet**.

---

## 1. Preparar Unity

El proyecto **ya está listo para abrir**: usa **Unity 6000.6.9f1** (instalado en `C:\Program Files\Unity\Hub\Editor\6000.6.9f1`).

1. **Forma rápida:** haz doble clic en `AbrirUnity.cmd` (está en la raíz del proyecto). Inicia Hub, activa la licencia y abre el Editor con el proyecto y la escena demo.
2. **Forma manual:** abre **Unity Hub** (necesario para la licencia Personal) → **Open > Add project from disk** → selecciona la carpeta `LA MYPIME GAME` (la que contiene `ProjectSettings/ProjectVersion.txt`).
3. Espera a que importe. **Active Input Handling = Both** se configura solo al cargar (`InputHandlingSetup.cs`), que también abre `DemoMypime.unity` si no hay ninguna escena activa.
4. Si quieres regenerar todo, usa el menú **LA MYPIME GAME > Generar escena demo**.

> Verificado en este equipo: compilación con **0 errores y 0 warnings**, escena `DemoMypime.unity` creada y añadida a Build Settings. Los paquetes que faltaban se resolvieron localmente (`Packages/com.unity.ugui` y stubs de analytics/purchasing), así que no depende de internet.

```
Assets/LA MYPIME GAME/
  Scripts/   (27 archivos .cs)
  Editor/    (2 archivos .cs: SceneBuilder + InputHandlingSetup)
  Models/    (25 FBX cartoon)
  Scenes/    (DemoMypime.unity, ya generada)
  Materials/ (ya generados)
  Data/      (ProductData + CatalogoPedidos, ya generados)
```

---

## 2. Generar la escena

Menú superior: **LA MYPIME GAME > Generar escena demo**

Crea el minimercado completo:

- **Local ampliado**: la zona de venta se extendió 4 m hacia el norte (pared norte en `z=12`, suelo de 20×20 m). El almacén sigue separado por la pared en `z=-3`.
- **Entrada principal**: hueco en la pared norte en `x=-8`, hoja de puerta entreabierta con marco y tirador, porche exterior y **cartel `LA MYPIME · MINIMERCADO`** a ambos lados, **fijo** (no gira ni sigue a la cámara). Los clientes entran y salen por aquí.
- **8 unidades de venta**: 3 estanterías en la pared norte ampliada, ahora en `z=10` (Arroz, Frijoles, Café), 1 nevera (Leche) y 4 estanterías en las paredes laterales (Azúcar, Aceite, Pan, Tomate), giradas hacia el interior.
- **8 cajas de pedido** repartidas en **4 estantes metálicos de almacén** (2 cajas por estante, sobre el nivel inferior a 0,5 m), una por producto. El almacén está **separado de la tienda por una pared con una puerta central**.
- **Carretera exterior y muelle de carga**: el triciclo aparca fuera y descarga por el hueco central de la pared sur hacia el almacén. Nunca repone estanterías.
- **Triciclo del proveedor** con repartidor, 3 ruedas, caja de carga y ruta de 3 puntos: carretera → centro de carretera → muelle. Descarga en el almacén sin entrar en la tienda.
- Caja registradora con cinta, lector y 4 huecos de escaneo, **junto al almacén** (`x=5, z=-0.5`), delante de la pared divisoria y sin tapar su puerta.
- **Modelos reales de `Cartoon_Supermarket_Collection.blend`** (25 FBX exportados a `Assets/LA MYPIME GAME/Models/CartoonSupermarket/`): góndolas de frescos y dulces, estanterías de bebidas y ofertas, nevera de lácteos, congelador, vitrina de frescos, mesa de productos, caja registradora cartoon, mostrador de devoluciones, carrito, cesta, expositor, señales, cajas de manzanas/naranjas/verduras y productos sueltos (snacks, cereales, brik, zumo, lata, pack de agua). Sustituyen a los primitivos genéricos: se eliminaron los estantes, la nevera y la caja viejos, y sus mallas fueron reemplazadas por los modelos cartoon **sobre los mismos objetos jugables** (se duplican los modelos de estantería para cubrir las 8 unidades de venta). Todos se instancian **sin colisionadores**, así que no alteran las rutas A*. Las cajas de cartón del almacén siguen siendo primitivas porque no estaban en el blend.
- **Computadora de pedidos** en el almacén (`x=7.5, z=-4.2`): al acercarte y pulsar **E** (o INTERACTUAR) abre la pantalla del proveedor.
- `NavigationGrid`, jugador, cámara, joystick, clientes, empleados e interfaz.

La escena se guarda en `Assets/LA MYPIME GAME/Scenes/DemoMypime.unity` y ya está en *Build Settings*.

> Tras actualizar el código, vuelve a generar la escena desde el menú para ver la entrada principal con su cartel `LA MYPIME`, los estantes laterales, la caja junto al almacén, la carretera, el muelle, la pared divisoria con su puerta, el almacén con sus estantes y palés, la computadora de pedidos y el triciclo aparcado fuera.

---

## 2b. Pedidos por computadora (estilo Supermarket Simulator)

La computadora del almacén abre una pantalla con el **catálogo del proveedor**:

- **Poner los productos que se pueden pedir**: edita `Assets/LA MYPIME GAME/Data/CatalogoPedidos.asset` (campo `activo`, tamaño de caja `lote` y `maximo`), o toca el botón **SÍ/NO** de cada fila dentro del juego. El cambio se guarda en la partida.
- **Elegir cantidades**: botones **− / +** suman o quitan una caja (8 unidades por defecto, hasta 32).
- **Hacer pedido**: se cobra al confirmar (`precio de coste × índice del día`) y queda esperando en `pedidosPendientes`.
- **Entrega**: al confirmar, el triciclo **sale de inmediato** por la carretera y deja los pedidos en las cajas del almacén. No hay horarios: si haces otro pedido mientras está en ruta, sale un viaje más al terminar. El triciclo sigue sin reponer estanterías.
- **Recoger**: al coger una caja de pedido PC no se vuelve a pagar; el dependiente o un empleado la llevan a la estantería.
- Los pedidos pendientes y los productos activados se guardan en `supermercado_guardado.json`.
- **Sin pedidos, no hay triciclo**: el proveedor ya no trae mercancía aleatoria ni hay escasez. El triciclo permanece aparcado fuera hasta que pagas algo en la computadora.

---

## 2c. Tienda de muebles y ampliaciones (v11)

La **tablet** junto a la caja (`x=-4,5, z=-0,6`) o el botón **TIENDA (B)** abren una pantalla con dos pestañas, igual que en *My Supermarket Simulator 3D*:

### Pestaña MUEBLES

- Elige primero el **producto** que venderá el mueble (los 8 del catálogo).
- Compra **estante básico** (8 huecos), **góndola doble** (12 huecos) o **nevera** (8 huecos, gasta luz). Los precios en CUP suben con el índice diario.
- El mueble aparece en el **primer hueco de obra libre** (o en el hueco desde el que abriste la pantalla).
- Quedan **2 huecos de obra** en el pasillo central desde el día 1, señalizados con un cartel **OBRA**. También puedes interactuar directamente con ellos.
- El mueble nuevo es jugable: se rellena desde las cajas del almacén y los clientes lo usan igual que los demás.

### Pestaña AMPLIACIONES

- **Ampliación Este**: 6 × 9 m, 2 huecos nuevos, +1 cliente simultáneo y +alquiler diario. Cuesta 55 USD (51.150 CUP el día 1).
- **Ampliación Oeste**: requiere la Este; añade otros 2 huecos, +1 cliente y más alquiler. Cuesta 85 USD.
- El ala está cerrada por una **persiana metálica** hasta que la compras. Al comprarla se retira la persiana, se abre el paso y se activan sus huecos.
- Cada ampliación sube el **alquiler diario** y el **límite de clientes**; la meta diaria no cambia.

Las compras, los muebles construidos y las ampliaciones abiertas se guardan en `supermercado_guardado.json`.

---

## 3. Controles

| Acción | PC | Móvil |
|---|---|---|
| Andar | WASD / flechas | Joystick abajo-izquierda |
| Mirar | Arrastrar ratón | Arrastrar mitad derecha |
| Interactuar | **E** / botón verde | Botón verde abajo-derecha |
| Encender/apagar nevera | Acércate con las manos vacías y **E** | Manos vacías + botón verde |
| Analítica de ventas | **Tab** | Botón `VENTAS` arriba-derecha |
| Contratar empleado | **H** | Botón `CONTRATAR` |
| Despedir empleado | **Mayús+H** | Botón `DESPEDIR` |
| Tienda de muebles | **B** / toca un cartel OBRA | Botón `TIENDA` o tablet |
| Guardar | **F5** | Automático al pausar |
| Reiniciar tras quiebra | **F6** o clic | Toca la pantalla |

---

## 4. Bucle de juego

1. Empiezas con **46.500 CUP** (50 USD de referencia). Date la vuelta: hay 8 pedidos.
2. Pulsa interactuar en un pedido → pagas el coste (ajustado por el índice) y coges 8 unidades.
3. Repón la estantería correspondiente (o deja que lo haga un empleado).
4. Los clientes entran, esquivan los muebles con A*, cogen 1-3 productos y van a la caja.
5. Escanea cada producto en la cinta (beep) y cobra el total en CUP.
6. Si el total con inflación supera el presupuesto del cliente, puede **rechazar el precio** e irse: pierdes reputación.
7. Al cerrar se paga **alquiler + luz + sueldos** y se evalúa la **meta diaria**: cúmplela para ganar un bonus y reputación; si no, baja la reputación.
8. Los clientes tienen **paciencia limitada**: si esperan demasiado en la cola o en la caja, se van y pierdes reputación. La barra sobre su cabeza pasa de verde a rojo.
9. La **Leche** va en nevera. Acércate con las manos vacías para apagarla y ahorrar **465 CUP** de luz, pero entonces la leche se estropea en 45 s y los clientes dejan de comprarla.
10. Pulsa **Tab** (o el botón `VENTAS`) para consultar la analítica: unidades e ingresos por producto hoy, acumulado y últimos días.
11. Contrata hasta 2 empleados con **H**: caminan solos hasta una caja de pedidos, pagan el lote y reponen la estantería más vacía.
12. El triciclo del proveedor **solo sale si hay pedidos pagados en la computadora**, sin horarios ni escasez: lo pagas y sale enseguida por la carretera hasta el muelle, deja **únicamente esos pedidos** en el **almacén** y vuelve a quedar aparcado. Si pides más cosas mientras está en ruta, hará otro viaje al terminar. Ya no hay viajes al amanecer ni antes de cerrar. Para reponer, entra al almacén por la **puerta de la pared divisoria**; eres tú o un empleado quien lleva la mercancía a las estanterías. El día 1 empieza con las cajas llenas y el triciclo oculto.
13. Si no puedes pagar los gastos, entra la **quiebra**: toca la pantalla para volver a empezar.
14. Abre la **TIENDA** (botón, tablet o cartel `OBRA`) para comprar estanterías y ampliar el local. El mueble se coloca al instante y luego se rellena desde el almacén.
15. Las **ampliaciones Este/Oeste** añaden metros, huecos de muebles, clientes y alquiler: conviene comprarlas cuando las ventas ya cubren el gasto extra.

### Productos provisionales (v10)

Los importes se introducen en **USD de referencia** y se convierten a CUP con la tasa del día. La tabla muestra el valor inicial a 930 CUP/USD.

| Producto | Precio USD | Precio CUP | Coste pedido USD | Coste pedido CUP | Demanda | Frío |
|---|---:|---:|---:|---:|---:|---|
| Arroz | 3 | 2.790 | 10 | 9.300 | 3.0 | No |
| Frijoles | 4 | 3.720 | 12 | 11.160 | 2.5 | No |
| Café | 6 | 5.580 | 18 | 16.740 | 1.5 | No |
| Leche | 5 | 4.650 | 15 | 13.950 | 2.0 | **Sí** |
| Azúcar | 2 | 1.860 | 8 | 7.440 | 2.0 | No |
| Aceite | 5 | 4.650 | 14 | 13.020 | 1.8 | No |
| Pan | 1 | 930 | 5 | 4.650 | 2.8 | No |
| Tomate | 3 | 2.790 | 9 | 8.370 | 1.6 | No |

Se editan en los assets `Data/*.asset` o en `SceneBuilder.cs` (función `Product`). Cada `ProductData` guarda `precioUSD` y `costePedidoUSD` como datos de partida, y `precio` / `costePedido` ya convertidos a CUP. El precio cobrado = `precio × índice del día`.

### Tasa USD y moneda (v10)

| Elemento | Detalle |
|---|---|
| Referencia | **1 USD = 930 CUP**, informe informal de elTOQUE del 18/11/2026 (la tasa oficial de la web, 500 CUP, no se usa) |
| Cambio diario | `Currency.NuevoDia()` aplica una variación aleatoria de **±0,8%** al cambiar de día |
| Límites | La tasa se mantiene entre **850 y 1050 CUP** para evitar escaladas absurdas |
| Efecto en precios | `indicePrecios` se multiplica por `tasaNueva / tasaAnterior` y después por la inflación ficticia (8%/día por defecto) |
| Visualización | El HUD muestra `USD: 930,00 CUP` y todos los importes van con separador de miles (`Currency.Format`) |
| Guardado | La tasa se guarda en JSON (`version = 2`). Las partidas antiguas se ignoran al cambiar la escala económica |
| Sin red | El juego no descarga la tasa: el valor de elTOQUE es solo la referencia inicial |

### Preferencias de clientes (v7)

Cada cliente elige un **producto favorito** al entrar, con un peso proporcional al campo `demanda` de cada producto. Sobre su cabeza aparece un cubo del color del producto que busca; desaparece cuando lo consigue.

- Va **primero** a por su favorito si hay stock disponible.
- Si lo encuentra → **cliente satisfecho**: +1 reputación extra y `Satisfechos` sube en el HUD.
- Si no lo encuentra (agotado, estropeado o sin pedido) → compra otra cosa pero queda **insatisfecho**: −2 reputación al cobrar.
- El presupuesto se calcula con los **precios del día** (`GameManager.PrecioDe`), no con los base, para que la inflación no provoque rechazos automáticos.

### Analítica de ventas (v8)

Pulsa **Tab** en PC o el botón **VENTAS** en móvil para abrir un panel con el informe de gestión. Vuelve a pulsar para cerrarlo.

| Bloque | Contenido |
|---|---|
| Hoy | Ingresos CUP y unidades vendidas desde que empezó el día |
| Por producto (hoy) | Unidades e ingresos estimados de cada producto ordenados por unidades |
| Acumulado | Unidades vendidas de cada producto desde el inicio de la partida |
| Últimos días | Los 5 días anteriores con ingresos, unidades y producto más vendido |

- La caja registra cada producto escaneado (`Checkout` → `SalesAnalytics.RegisterSale`).
- Al pasar de día se cierra el informe (`GameManager.NextDay` → `SalesAnalytics.NewDay`).
- El historial (10 días) y el acumulado se guardan en JSON dentro del campo `analyticsJson`. Tras una quiebra y reinicio se borra.

### Empleados (v9)

| Elemento | Detalle |
|---|---|
| Contratación | **H** o botón `CONTRATAR`. Pago único de **13.950 CUP**; máximo 2 empleados |
| Despido | **Mayús+H** o botón `DESPEDIR`. No devuelve la entrada |
| Salario | **11.160 CUP/día por empleado**, se suma a alquiler y luz al cerrar |
| Trabajo | Buscan la estantería más vacía, van a su caja de pedido, pagan el lote y lo reponen |
| Ahorro real | El lote cuesta lo mismo que si lo compraras tú: el empleado ahorra **tiempo**, no dinero |
| Navegación | Usan el mismo `NavigationGrid` con A* que los clientes |
| Guardado | El número de empleados se guarda en JSON; al recargar aparecen solo los contratados |

Si no hay dinero o no hay pedido disponible, el empleado espera y lo intenta de nuevo; no repone mercancía gratis.

### Neveras (v6)

El sistema es **genérico**: cualquier producto futuro puede marcarse con `necesitaFrio` en su `ProductData` y se comportará igual que la Leche.

| Elemento | Detalle |
|---|---|
| Unidad fría | `Shelf` con `esNevera = true`: cristal delantero, indicador verde/rojo y `costeLuz = 465` |
| Interruptor | Manos vacías + interactuar. Apagar ahorra luz, pero empieza el deterioro |
| Deterioro | 45 s sin frío (`tiempoDesperdicio`); los cubos se vuelven grises y los clientes no los cogen |
| Recuperación | Volver a encender la nevera o reponer mercancía fresca |
| Coste diario | Se suma al alquiler al cerrar; visible en el HUD como `Luz: N CUP` |

### Triciclo, carretera y almacén (solo con pedido + pared divisoria)

| Elemento | Detalle |
|---|---|
| Modelo | Modelo 3D detallado generado por código: caja de carga con 4 paredes y bordes, chasis tubular, horquilla, manubrio con agarres, faro, asiento, pedales, guardabarros, repartidor con brazos y 3 ruedas con neumático, rin, cubo y 6 radios que giran |
| Punto de partida | Aparcado en la **carretera exterior** (x=12, z=-11,2), con el morro hacia la izquierda. Nunca está dentro de la tienda |
| Ruta | `RutaTriciclo_Carretera` → `CarreteraCentro` → `Muelle`. El vehículo se queda fuera, en la carretera; no pasa por ninguna estantería |
| Descarga | Desde el muelle deja **solo las cajas de los pedidos pagados** (`Refill`); las demás quedan vacías. El jugador/empleado las recoge cruzando la puerta y las reparte por las estanterías |
| Almacén | Suelo de hormigón, cartel `ALMACÉN` estático, **4 estantes metálicos de 2 niveles con las 8 cajas de pedido** en el inferior y bultos en el superior, dos palés y una **pared divisoria con puerta central de 3 m** que lo separa de la tienda |
| Horario | **No tiene horario**: sale únicamente cuando se confirma un pedido en la computadora (`HacerPedido()` → `IntentarEnviarTriciclo()`). Puede salir a cualquier hora del día |
| Activación | Día 1 oculto con las cajas llenas; aparece en cuanto se pagó el primer pedido |
| Pedidos nuevos en ruta | Si se hacen más pedidos mientras el triciclo va en camino, quedan en `pedidosPendientes` y salen en el viaje siguiente al terminar la descarga |
| Escasez | **Eliminada**: ya no hay mercancía aleatoria ni porcentaje de llegada. Solo se transporta lo que se pagó |
| Sin triciclo | Si la referencia está vacía, los pedidos se reponen directamente en las cajas |

### Presión diaria (v10)

| Mecánica | Valor inicial (a 930 CUP/USD) | Dónde se ajusta |
|---|---|---|
| Dinero de partida | 46.500 CUP | `dinero` en GameManager / `Currency.CUP(50f)` |
| Meta de ventas del día | 27.900 CUP (crece con el índice) | `metaBase` y `metaVentas` en GameManager |
| Alquiler | 18.600 CUP/día (crece con el índice) | `alquilerBase` y `alquiler` en GameManager |
| Reputación | 70/100 | `reputacion` en GameManager |
| Rechazo por precio | −6 reputación | `RegisterRejection()` en GameManager |
| Venta completada | +1 reputación, CUP sumados a `ventasHoy` | `RegisterSale()` en GameManager |
| Cliente satisfecho | +1 reputación extra | `RegisterSale(amount, true)` |
| Cliente insatisfecho | −2 reputación | `RegisterSale(amount, false)` |
| Cumplir meta | +9.300 CUP y +5 reputación | cierre del día en GameManager |
| Fallar meta | −10 reputación | cierre del día en GameManager |
| Paciencia en cola/caja | 22 s por cliente | `paciencia` en Customer |
| Abandonar por espera | −4 reputación y cliente perdido | `RegisterAbandon()` en GameManager |
| Consumo de neveras | 465 CUP/día por nevera encendida | `costeLuz` en Shelf |
| Contratar | 13.950 CUP (pago único) | `costeContratacion` en GameManager |
| Sueldos | 11.160 CUP/día por empleado | `salarioPorEmpleado` en GameManager |
| Quiebra | si el dinero queda < 0 al pagar alquiler + luz + sueldos | cierre del día en GameManager |
| Reiniciar | toca la pantalla (o **F6**) | `Reiniciar()` en GameManager |

El presupuesto de cada cliente se calcula al llegar a caja: `suma de precios del día × 1,08–1,45`. Si el total con inflación lo supera, se marcha. La reputación, las estadísticas, la tasa y el estado de quiebra se guardan en JSON junto al resto de la partida.

---

## 5. Scripts

| Archivo | Función |
|---|---|
| `Currency.cs` | Tasa USD/CUP de referencia, conversión, formato con miles y variación diaria. |
| `ProductData.cs` | Producto: nombre, precios USD/CUP, coste, color, tamaño, demanda y flag `necesitaFrio`. |
| `IInteractable.cs` | Interfaz de objetos interactuables. |
| `MaterialFactory.cs` | Materiales de color reutilizables. |
| `GameManager.cs` | Día, dinero, ventas, tasa, meta, alquiler, luz, sueldos, empleados, reputación, quiebra, triciclo y singleton. |
| `NavigationGrid.cs` | Grid A* con suavizado, gizmos y paredes como obstáculo (excepto dinteles, para no tapar puertas). |
| `Shelf.cs` | Stock, reposición (manual y de empleado), nevera, interruptor, deterioro y visuales. |
| `Employee.cs` | NPC repositor con A*: elige estantería, paga el pedido y repone. |
| `DeliveryBox.cs` | Cajas de pedido del almacén: recogen la mercancía pagada sin cobrar dos veces. |
| `DeliveryTricycle.cs` | Ruta, ruedas y descarga del triciclo del proveedor. |
| `Checkout.cs` | Cola, escaneo en cinta, cobro y registro de ventas. |
| `Customer.cs` | IA con producto favorito, ruta A*, presupuesto, paciencia y satisfacción. |
| `CustomerSpawner.cs` | Crea clientes según la reputación (máx. 4). |
| `PlayerController.cs` | Movimiento, cámara, raycast y producto en mano. |
| `Joystick.cs` | Joystick táctil sin EventSystem. |
| `HUD.cs` | Día, CUP formateados, USD, meta, alquiler, luz, sueldos, empleados, reputación, avisos, quiebra y analítica. |
| `SoundFx.cs` | Beeps generados por código. |
| `SalesAnalytics.cs` | Informes de ventas por producto y por día (estático). |
| `SaveSystem.cs` | JSON en `persistentDataPath` con versión 2, tasa, analítica, pedidos PC y catálogo activo. |
| `OrderCatalog.cs` | Catálogo del proveedor: productos pedibles, lote, máximo y activo/desactivado. |
| `OrderComputer.cs` | Computadora interactuable del almacén que abre la pantalla de pedidos. |
| `OrderComputerUI.cs` | Pantalla táctil/PC de pedidos: cantidades, coste, confirmación y estado pendiente. |
| `StoreManager.cs` | Precios de muebles/ampliaciones, compra y colocación, alquiler extra, límite de clientes y fábrica de muebles procedurales. |
| `StoreUI.cs` | Pantalla de tienda con pestañas MUEBLES y AMPLIACIONES, selección de producto y avisos. |
| `StoreTablet.cs` | Tablet del local que abre la tienda de muebles. |
| `BuildSpot.cs` | Hueco de obra interactuable: cartel, compra y construcción del mueble. |
| `ExpansionZone.cs` | Ala nueva cerrada por persiana: compra, apertura y activación de sus huecos. |
| `Editor/SceneBuilder.cs` | Generador de escena (entrada con cartel, 8 muebles, 8 pedidos, computadora, carretera, muelle, pared divisoria, almacén, triciclo, huecos de obra, tablet y ampliaciones). |

### A* en breve

`NavigationGrid.Build()` marca las celdas que solapan estanterías, pedidos, caja, tablet y paredes (con margen de 0,35 m); los dinteles se ignoran para dejar libre la puerta del almacén y la persiana cerrada bloquea el ala sin comprar. `FindPath()` usa A* de 8 vecinos, evita cortar esquinas y suaviza la ruta con línea de visión. Si no hay grid, el cliente se mueve en línea recta. El grid cubre también las alas Este/Oeste (`x` de −16,5 a 16,5) y se **recalcula al comprar** un mueble o una ampliación.

Selecciona `NavigationGrid` y activa **Mostrar Grid** para ver las celdas bloqueadas. Si mueves muebles, usa **Recalcular grid** (clic derecho en el componente).

---

## 6. Exportar a Android

1. **File > Build Profiles > Android > Switch Platform** (instala *Android Build Support* si falta).
2. **Player Settings**: Product Name `LA MYPIME GAME`, Package Name `com.tunombre.mypimegame`, Landscape Left, API 24+.
3. **Build** → `.apk`. Instalar: `adb install -r tuapk.apk`.

---

## 7. Problemas comunes

| Síntoma | Solución |
|---|---|
| Clientes atraviesan muebles | Comprueba que `NavigationGrid` esté en la escena y pulsa **Recalcular grid**. |
| Clientes quietos | Revisa que los `PuntoCliente` no caigan en celdas bloqueadas; amplía `min/max` del grid. |
| No veo los estantes laterales, la carretera, la pared ni el triciclo | Vuelve a generar la escena con **LA MYPIME GAME > Generar escena demo**. |
| Precios no suben | Revisa `inflacionDiaria` y `Currency.NuevoDia()`, y borra la partida guardada. |
| La tasa no es la de hoy | El juego usa 930 CUP como referencia fija y la mueve de forma simulada; consulta elTOQUE para comparar, no para importar datos. |
| Empezar de cero | Borra `supermercado_guardado.json` de `persistentDataPath` o pulsa **F6** tras la quiebra. |
| No aparece el menú | Espera a que importe `Editor/SceneBuilder.cs`. |
| La leche se estropea mucho | Enciende la nevera (manos vacías + E). Ajusta `tiempoDesperdicio` en `Shelf`. |
| La luz sale cara | Apaga la nevera cuando no vendas leche, o baja `costeLuz` en `Shelf`. |
| El triciclo se queda quieto | Es normal si no hay pedidos: confirma uno en la computadora. Si hay pedidos y no sale, revisa los 3 `waypoints` (`Carretera`, `CarreteraCentro`, `Muelle`) y que `manager.triciclo` esté asignado. |
| El empleado no repone | Comprueba que haya pedido disponible y dinero; ajusta `intervalo` en `Employee`. |
| Los sueldos arruinan la partida | Despide con Mayús+H o baja `salarioPorEmpleado` en `GameManager`. |
| No aparecen los carteles OBRA ni la tablet | Vuelve a generar la escena con **LA MYPIME GAME > Generar escena demo**. |
| Compré una ampliación y no puedo entrar | Acércate a la persiana y pulsa E, o usa el botón TIENDA; comprueba que la persiana desapareció. |
| Los clientes no van a los muebles nuevos | Pulsa **Recalcular grid** en `NavigationGrid` o revisa que el hueco no esté bloqueado. |

---

## 8. Siguiente paso

Pendiente: **lista definitiva de productos** al final del proyecto. Ya están listas neveras, preferencias, analítica, empleados, estanterías laterales, carretera/muelle/almacén, triciclo y conversión USD→CUP: solo hay que crear cada `ProductData` con su `precioUSD`, `costePedidoUSD`, `demanda` y `necesitaFrio`.
