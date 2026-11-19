# LA MYPIME GAME

Simulador de un **minimercado (MYPIME)** en 3D para móvil, hecho con **Unity**.

> Los 8 productos actuales (**Arroz, Frijoles, Café, Leche, Azúcar, Aceite, Pan y Tomate**) son **provisionales**: se sustituirán por la lista definitiva al final del proyecto. La economía (CUP, tasa USD, índice de precios, consumo eléctrico) es ficticia y está pensada solo como mecánica de juego. La tasa inicial, **1 USD = 930 CUP**, es una referencia informativa de elTOQUE del 18/11/2026.

## Inicio rápido

1. Crea un proyecto Unity 3D (Unity 6 LTS recomendado).
2. Pon **Active Input Handling = Both** en *Project Settings > Player*.
3. Copia `LA MYPIME GAME/Assets/LA MYPIME GAME` en el `Assets` de tu proyecto.
4. Menú **LA MYPIME GAME > Generar escena demo**.
5. Pulsa **Play**.

👉 Guía completa: [GUIA.md](GUIA.md)

## Bucle de juego

```
Pedir en la computadora → El triciclo sale solo con ese pedido → Cruzar la puerta → Reponer estanterías/nevera
→ Cliente coge 1-3 productos (esquivando estanterías con A*)
→ Escanear en la cinta antes de que se acabe su paciencia → Cobrar en CUP
→ Cierre del día: tasa USD, alquiler + luz + sueldos, meta y reputación
→ Analítica (Tab) → Siguiente día (el triciclo sigue aparcado hasta otro pedido)
```

## Características

- 24 scripts C# comentados en español + generador de escena.
- **8 unidades de venta**: 3 estanterías norte, nevera y 4 estanterías laterales; **8 cajas de pedido** acomodadas en 4 estantes de almacén (2 por estante).
- **Local ampliado**: zona de venta extendida 4 m hacia el norte (suelo 20×20 m, pared norte en `z=12`).
- **Entrada principal**: puerta en la pared norte con porche y cartel **`LA MYPIME · MINIMERCADO`**; los clientes entran y salen por ahí.
- **Caja junto al almacén**: mostrador en `x=5, z=-0.5`, delante de la pared divisoria y sin bloquear la puerta, para cobrar cerca de la zona de descarga.
- **Colección cartoon real**: 25 modelos FBX exportados desde `Cartoon_Supermarket_Collection.blend` (góndolas, estanterías, nevera, congelador, vitrina, caja, carrito, cesta, señales, frescos y productos). Se colocan por código, sin colisionadores. Se eliminaron los estantes/nevera/caja primitivos: sus modelos cartoon se montan directamente sobre los objetos jugables, duplicando las estanterías para las 8 unidades de venta. La vista previa usa el GLB `preview/cartoon_supermarket.glb`.
- **Computadora de pedidos**: pantalla del proveedor en el almacén (estilo Supermarket Simulator). Permite poner qué productos se pueden pedir (`Data/CatalogoPedidos.asset` o botones SÍ/NO), elegir cajas con −/+ y pagar al confirmar. Al pagar, el triciclo sale de inmediato; los pedidos se recogen sin pagar dos veces.
- **Carretera, muelle y almacén**: el triciclo aparca fuera de la tienda y deja la mercancía en 8 cajas distribuidas en 4 estantes metálicos de almacén, con palés y cartel. El almacén está **separado de la tienda por una pared con puerta central**; **el triciclo nunca repone estanterías** (lo hacen el jugador o los empleados cruzando la puerta).
- **Triciclo del proveedor**: modelo 3D detallado (caja de carga, chasis, manubrio, faro, pedales, guardabarros, repartidor y ruedas con radios). **No tiene horario ni escasez**: permanece aparcado y solo viaja cuando confirmas un pedido; recorre carretera → muelle, se queda fuera y descarga en el almacén. Si pides más cosas mientras está en ruta, hace otro viaje al terminar.
- **Economía en CUP con USD**: referencia 1 USD = 930 CUP (elTOQUE), variación diaria simulada ±0,8%, precios y gastos convertidos, separador de miles en el HUD.
- Pathfinding **A\*** propio: los clientes no atraviesan los muebles.
- Cinta de escaneo, cola de clientes y cesta visual.
- Economía ficticia: pedidos con coste, inflación diaria, alquiler, luz, sueldos y meta diaria.
- Clientes con producto favorito, presupuesto calculado con los precios del día y paciencia: rechazan precios altos y se van si esperan demasiado.
- Reputación, meta diaria, alquiler y pantalla de quiebra con reinicio.
- **Neveras**: productos fríos que se deterioran sin luz; interruptor y coste eléctrico diario.
- **Analítica de ventas**: panel con unidades e ingresos por producto, acumulado e historial (Tab / botón VENTAS).
- **Empleados**: hasta 2 repositores automáticos con A*; pago único, salario diario y botones contratar/despedir.
- Controles táctiles (joystick + arrastrar + botón) y PC (WASD + ratón + E).
- Guardado JSON automático con **versión 2** (día, dinero, tasa, ventas, índice, stock, pedidos, neveras, empleados y analítica). Las partidas antiguas se ignoran.
- Sonidos generados por código, sin assets externos.

## Estructura

```
LA MYPIME GAME/
├── Assets/LA MYPIME GAME/
│   ├── Scripts/       (19 .cs, incluye Currency.cs y DeliveryTricycle.cs)
│   ├── Editor/        (SceneBuilder.cs)
│   ├── Scenes/        (se genera: DemoMypime.unity)
│   ├── Materials/     (se genera)
│   └── Data/          (se genera: 8 productos provisionales)
├── preview/storyboard.html   (preview animado ilustrativo)
├── GUIA.md
└── README.md
```

## Importes iniciales (a 930 CUP/USD)

| Concepto | CUP |
|---|---:|
| Dinero de partida | 46.500 |
| Meta diaria | 27.900 |
| Alquiler diario | 18.600 |
| Luz (nevera encendida) | 465 |
| Contratar empleado | 13.950 |
| Sueldo diario por empleado | 11.160 |
| Bonus por cumplir la meta | 9.300 |

La tasa y el índice de precios cambian cada día; consulta `GUIA.md` para los detalles y la tabla de productos.
