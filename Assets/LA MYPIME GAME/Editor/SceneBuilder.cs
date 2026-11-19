#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Genera una escena de demostración completa desde el menú:
/// LA MYPIME GAME > Generar escena demo
/// Todo está hecho con cubos y esferas para no necesitar modelos 3D externos.
/// </summary>
public static class SceneBuilder
{
    private const string Root = "Assets/LA MYPIME GAME";
    private const string Scenes = Root + "/Scenes";
    private const string Materials = Root + "/Materials";
    private const string Data = Root + "/Data";
    private const string CartoonModels = Root + "/Models/CartoonSupermarket";

    [MenuItem("LA MYPIME GAME/Generar escena demo")]
    public static void GenerarEscena()
    {
        EnsureFolder("Assets", "LA MYPIME GAME");
        EnsureFolder(Root, "Scenes");
        EnsureFolder(Root, "Materials");
        EnsureFolder(Root, "Data");
        EnsureFolder(Root, "Models");
        EnsureFolder(Root + "/Models", "CartoonSupermarket");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // --- Materiales ---
        Material floorMat = Mat("Suelo", new Color(0.72f, 0.68f, 0.60f));
        Material wallMat = Mat("Pared", new Color(0.90f, 0.90f, 0.94f));
        Material shelfMat = Mat("Estanteria", new Color(0.35f, 0.28f, 0.22f));
        Material woodMat = Mat("Mostrador", new Color(0.55f, 0.40f, 0.25f));
        Material boxMat = Mat("CajaPedido", new Color(0.62f, 0.45f, 0.28f));
        Material regMat = Mat("Registradora", new Color(0.20f, 0.35f, 0.60f));
        Material cintaMat = Mat("Cinta", new Color(0.13f, 0.14f, 0.17f));
        Material lectorMat = Mat("Lector", new Color(0.20f, 0.75f, 0.35f));
        Material neveraMat = Mat("Nevera", new Color(0.74f, 0.87f, 0.93f));
        Material cristalMat = Mat("CristalNevera", new Color(0.52f, 0.76f, 0.90f));
        Material indicadorMat = Mat("IndicadorNevera", new Color(0.20f, 0.90f, 0.35f));
        Material empleadoMat = Mat("Empleado", new Color(0.28f, 0.55f, 0.72f));
        Material delantalMat = Mat("Delantal", new Color(0.95f, 0.72f, 0.20f));

        // --- Luces ---
        GameObject luz = new GameObject("Luz principal");
        Light light = luz.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.3f;
        luz.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.60f);

        // --- Suelo y paredes ---
        // Suelo ampliado: el local de venta llega ahora hasta z = 12 (4 m más).
        Cube("Suelo", new Vector3(0f, -0.15f, 2f), new Vector3(20f, 0.3f, 20f), floorMat);

        // Pared norte partida: hueco de entrada en x = -8 (2,4 m) con dintel.
        Cube("Pared norte izq", new Vector3(-9f, 1.75f, 12f), new Vector3(2f, 3.5f, 0.3f), wallMat);
        Cube("Pared norte der", new Vector3(1f, 1.75f, 12f), new Vector3(18f, 3.5f, 0.3f), wallMat);
        Cube("Pared norte dintel", new Vector3(-8f, 2.9f, 12f), new Vector3(2.4f, 1.2f, 0.3f), wallMat);

        // Materiales compartidos por entrada, almacén y carretera (se declaran antes de usarse).
        Material marcoMat = Mat("MarcoAlmacen", new Color(0.35f, 0.28f, 0.22f));
        Material concreteMat = Mat("SueloAlmacen", new Color(0.55f, 0.56f, 0.54f));

        // Marco, hoja entreabierta, tirador y porche de la entrada principal.
        Material puertaMat = Mat("PuertaEntrada", new Color(0.16f, 0.42f, 0.55f));
        Material tiradorMat = Mat("TiradorEntrada", new Color(0.90f, 0.78f, 0.35f));
        Cube("MarcoEntrada_Izq", new Vector3(-9.2f, 1.2f, 12f), new Vector3(0.18f, 2.4f, 0.4f), marcoMat);
        Cube("MarcoEntrada_Der", new Vector3(-6.8f, 1.2f, 12f), new Vector3(0.18f, 2.4f, 0.4f), marcoMat);
        Cube("MarcoEntrada_Superior", new Vector3(-8f, 2.35f, 12f), new Vector3(2.6f, 0.18f, 0.4f), marcoMat);
        GameObject hoja = Cube("PuertaEntrada_Hoja", new Vector3(-8.85f, 1.15f, 12.05f), new Vector3(2.1f, 2.3f, 0.12f), puertaMat);
        hoja.transform.rotation = Quaternion.Euler(0f, -28f, 0f);
        GameObject tirador = Cube("PuertaEntrada_Tirador", new Vector3(-7.95f, 1.15f, 12.35f), new Vector3(0.12f, 0.4f, 0.12f), tiradorMat);
        tirador.transform.rotation = Quaternion.Euler(0f, -28f, 0f);
        Cube("PorcheEntrada", new Vector3(-8f, -0.02f, 12.9f), new Vector3(4.2f, 0.1f, 2f), concreteMat);

        // Cartel del minimercado sobre la entrada (tamaño original).
        Material cartelNombreMat = Mat("CartelNombre", new Color(0.05f, 0.22f, 0.32f));
        Cube("CartelNombre_Fondo", new Vector3(-8f, 3.05f, 11.74f), new Vector3(4.6f, 0.8f, 0.08f), cartelNombreMat);
        // Carteles fijos: la cara interior mira a la tienda (-Z) y la exterior a la calle (+Z).
        TextMesh nombreTienda = Texto("CartelNombre_Texto", "LA MYPIME", new Vector3(-8f, 3.16f, 11.68f), 0.115f, new Color(1f, 0.93f, 0.62f));
        nombreTienda.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        nombreTienda.fontStyle = FontStyle.Bold;
        TextMesh subtituloTienda = Texto("CartelNombre_Subtitulo", "MINIMERCADO", new Vector3(-8f, 2.84f, 11.68f), 0.055f, new Color(0.80f, 0.92f, 1f));
        subtituloTienda.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        subtituloTienda.fontStyle = FontStyle.Bold;
        Cube("CartelNombre_Fondo_Exterior", new Vector3(-8f, 3.05f, 12.26f), new Vector3(4.6f, 0.8f, 0.08f), cartelNombreMat);
        TextMesh nombreExt = Texto("CartelNombre_Texto_Exterior", "LA MYPIME", new Vector3(-8f, 3.16f, 12.32f), 0.115f, new Color(1f, 0.93f, 0.62f));
        nombreExt.fontStyle = FontStyle.Bold;
        TextMesh subExt = Texto("CartelNombre_Subtitulo_Exterior", "MINIMERCADO", new Vector3(-8f, 2.84f, 12.32f), 0.055f, new Color(0.80f, 0.92f, 1f));
        subExt.fontStyle = FontStyle.Bold;

        // Pared sur con hueco central: es el muelle de carga del almacén.
        Cube("Pared sur izq", new Vector3(-6f, 1.75f, -8f), new Vector3(8f, 3.5f, 0.3f), wallMat);
        Cube("Pared sur der", new Vector3(6f, 1.75f, -8f), new Vector3(8f, 3.5f, 0.3f), wallMat);
        Cube("Pared sur dintel", new Vector3(0f, 2.9f, -8f), new Vector3(4f, 1.2f, 0.3f), wallMat);
        // Paredes este/oeste partidas: dejan un hueco central (z 5,5 a 9) para
        // las ampliaciones laterales. La persiana metálica lo cierra hasta comprarlas.
        Cube("Pared este sur", new Vector3(10f, 1.75f, -1.25f), new Vector3(0.3f, 3.5f, 13.5f), wallMat);
        Cube("Pared este norte", new Vector3(10f, 1.75f, 10.5f), new Vector3(0.3f, 3.5f, 3f), wallMat);
        Cube("Pared este dintel", new Vector3(10f, 2.9f, 7.25f), new Vector3(0.3f, 1.2f, 3.5f), wallMat);
        Cube("Pared oeste sur", new Vector3(-10f, 1.75f, -1.25f), new Vector3(0.3f, 3.5f, 13.5f), wallMat);
        Cube("Pared oeste norte", new Vector3(-10f, 1.75f, 10.5f), new Vector3(0.3f, 3.5f, 3f), wallMat);
        Cube("Pared oeste dintel", new Vector3(-10f, 2.9f, 7.25f), new Vector3(0.3f, 1.2f, 3.5f), wallMat);

        // Pared interior: separa la tienda del almacén con una puerta central de 3 m.
        Cube("Pared almacen izq", new Vector3(-5.75f, 1.75f, -3f), new Vector3(8.5f, 3.5f, 0.3f), wallMat);
        Cube("Pared almacen der", new Vector3(5.75f, 1.75f, -3f), new Vector3(8.5f, 3.5f, 0.3f), wallMat);
        Cube("Pared almacen dintel", new Vector3(0f, 2.9f, -3f), new Vector3(3f, 1.2f, 0.3f), wallMat);
        Cube("MarcoAlmacen_Izq", new Vector3(-1.6f, 1.15f, -3f), new Vector3(0.2f, 2.3f, 0.35f), marcoMat);
        Cube("MarcoAlmacen_Der", new Vector3(1.6f, 1.15f, -3f), new Vector3(0.2f, 2.3f, 0.35f), marcoMat);
        Cube("MarcoAlmacen_Superior", new Vector3(0f, 2.35f, -3f), new Vector3(3.4f, 0.2f, 0.35f), marcoMat);

        // Carretera exterior, muelle y almacén: el triciclo para aquí y NUNCA entra en las estanterías.
        Material roadMat = Mat("Carretera", new Color(0.18f, 0.19f, 0.21f));
        Material lineMat = Mat("LineaCarretera", new Color(0.92f, 0.86f, 0.35f));
        Material palletMat = Mat("PaletAlmacen", new Color(0.45f, 0.32f, 0.20f));
        Material bultoMat = Mat("BultoAlmacen", new Color(0.78f, 0.66f, 0.42f));
        Material cartelMat = Mat("CartelAlmacen", new Color(0.12f, 0.14f, 0.18f));
        Cube("Carretera", new Vector3(0f, -0.06f, -11.5f), new Vector3(30f, 0.12f, 5f), roadMat);
        for (int i = -12; i <= 12; i += 4)
            Cube("LineaCarretera", new Vector3(i, 0.01f, -11.5f), new Vector3(2f, 0.02f, 0.10f), lineMat);
        Cube("MuelleExterior", new Vector3(0f, -0.05f, -9f), new Vector3(4.2f, 0.10f, 2f), concreteMat);
        Cube("SueloAlmacen", new Vector3(0f, 0.005f, -5.5f), new Vector3(16f, 0.01f, 5f), concreteMat);
        CreatePalletStack("PaletAlmacenIzq", new Vector3(-6.5f, 0f, -6.6f), palletMat, bultoMat);
        CreatePalletStack("PaletAlmacenDer", new Vector3(6.5f, 0f, -6.6f), palletMat, bultoMat);

        // Cartel visible desde el interior para identificar la zona de descarga.
        Cube("CartelAlmacen_Fondo", new Vector3(0f, 2.9f, -7.82f), new Vector3(3.4f, 0.65f, 0.06f), cartelMat);
        Texto("CartelAlmacen_Texto", "ALMACÉN", new Vector3(0f, 2.9f, -7.76f), 0.09f, new Color(1f, 0.92f, 0.62f));

        // Cartel sobre la puerta, visto desde la tienda.
        Cube("CartelPuertaAlmacen_Fondo", new Vector3(0f, 2.75f, -2.78f), new Vector3(2.6f, 0.5f, 0.06f), cartelMat);
        Texto("CartelPuertaAlmacen_Texto", "ALMACÉN", new Vector3(0f, 2.75f, -2.72f), 0.065f, new Color(1f, 0.92f, 0.62f));

        // --- GameManager ---
        GameObject gmGo = new GameObject("GameManager");
        GameManager manager = gmGo.AddComponent<GameManager>();
        manager.duracionDia = 180f;
        manager.dinero = Currency.CUP(50f);

        // --- Productos PROVISIONALES (se sustituyen por la lista definitiva) ---
        // Los importes se dan en USD de referencia y se convierten a CUP con la tasa del día.
        ProductData arroz = Product("Arroz", 3f, 10f, new Color(0.94f, 0.93f, 0.86f),
            new Vector3(0.24f, 0.38f, 0.24f), Mat("Prod_Arroz", new Color(0.94f, 0.93f, 0.86f)), false, 3f);
        ProductData frijoles = Product("Frijoles", 4f, 12f, new Color(0.45f, 0.16f, 0.12f),
            new Vector3(0.26f, 0.28f, 0.26f), Mat("Prod_Frijoles", new Color(0.45f, 0.16f, 0.12f)), false, 2.5f);
        ProductData cafe = Product("Café", 6f, 18f, new Color(0.28f, 0.16f, 0.10f),
            new Vector3(0.20f, 0.34f, 0.20f), Mat("Prod_Cafe", new Color(0.28f, 0.16f, 0.10f)), false, 1.5f);
        ProductData leche = Product("Leche", 5f, 15f, new Color(0.93f, 0.95f, 0.98f),
            new Vector3(0.20f, 0.38f, 0.20f), Mat("Prod_Leche", new Color(0.93f, 0.95f, 0.98f)), true, 2f);
        ProductData azucar = Product("Azúcar", 2f, 8f, new Color(0.98f, 0.98f, 0.96f),
            new Vector3(0.22f, 0.32f, 0.22f), Mat("Prod_Azucar", new Color(0.98f, 0.98f, 0.96f)), false, 2f);
        ProductData aceite = Product("Aceite", 5f, 14f, new Color(0.92f, 0.72f, 0.18f),
            new Vector3(0.20f, 0.40f, 0.20f), Mat("Prod_Aceite", new Color(0.92f, 0.72f, 0.18f)), false, 1.8f);
        ProductData pan = Product("Pan", 1f, 5f, new Color(0.85f, 0.62f, 0.32f),
            new Vector3(0.28f, 0.22f, 0.22f), Mat("Prod_Pan", new Color(0.85f, 0.62f, 0.32f)), false, 2.8f);
        ProductData tomate = Product("Tomate", 3f, 9f, new Color(0.86f, 0.22f, 0.18f),
            new Vector3(0.24f, 0.24f, 0.24f), Mat("Prod_Tomate", new Color(0.86f, 0.22f, 0.18f)), false, 1.6f);

        // --- Catálogo de pedidos: aquí se pone QUÉ productos se pueden pedir en la computadora. ---
        OrderCatalog catalogo = CreateCatalogo(new[] { arroz, frijoles, cafe, leche, azucar, aceite, pan, tomate });
        manager.catalogoPedidos = catalogo;

        // --- 8 unidades de venta: la lógica es la misma, la malla la dan los modelos cartoon.
        // Se duplican los modelos de góndola/estantería para cubrir las 8 posiciones. ---
        Shelf s1 = CreateShelf("Estanteria Arroz", new Vector3(-5f, 0f, 10f), arroz, 4, shelfMat);
        AdjuntarModelo(s1.gameObject, "Gondola_Dulces");
        Shelf s2 = CreateShelf("Estanteria Frijoles", new Vector3(0f, 0f, 10f), frijoles, 5, shelfMat);
        AdjuntarModelo(s2.gameObject, "Gondola_Frescos");
        Shelf s3 = CreateShelf("Estanteria Café", new Vector3(5f, 0f, 10f), cafe, 3, shelfMat);
        AdjuntarModelo(s3.gameObject, "Estanteria_Bebidas");
        manager.shelves.AddRange(new[] { s1, s2, s3 });

        // --- Nevera: conserva la Leche, consume luz si está encendida ---
        Shelf nevera = CreateShelf("Nevera Leche", new Vector3(8f, 0f, 10f), leche, 4,
            neveraMat, true, cristalMat, indicadorMat);
        AdjuntarModelo(nevera.gameObject, "Nevera_Lacteos");
        manager.shelves.Add(nevera);

        // --- 4 unidades en las paredes laterales (miran al interior) ---
        Shelf s5 = CreateShelf("Estanteria Azúcar", new Vector3(-8.2f, 0f, 2f), azucar, 4, shelfMat);
        s5.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        AdjuntarModelo(s5.gameObject, "Estanteria_Ofertas");
        Shelf s6 = CreateShelf("Estanteria Aceite", new Vector3(-8.2f, 0f, -1f), aceite, 3, shelfMat);
        s6.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        AdjuntarModelo(s6.gameObject, "Gondola_Dulces");
        Shelf s7 = CreateShelf("Estanteria Pan", new Vector3(8.2f, 0f, 2f), pan, 5, shelfMat);
        s7.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        AdjuntarModelo(s7.gameObject, "Gondola_Frescos");
        Shelf s8 = CreateShelf("Estanteria Tomate", new Vector3(8.2f, 0f, -1f), tomate, 4, shelfMat);
        s8.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        AdjuntarModelo(s8.gameObject, "Estanteria_Bebidas");
        manager.shelves.AddRange(new[] { s5, s6, s7, s8 });

        // --- Elementos reales de Cartoon_Supermarket_Collection.blend (sin colisionadores) ---
        CreateCartoonSupermarket();

        // --- Tienda, huecos de obra y ampliaciones laterales ---
        // Dos huecos libres en el pasillo central desde el día 1. Las ampliaciones
        // Este y Oeste añaden alas nuevas cerradas por una persiana.
        List<BuildSpot> buildSpots = new List<BuildSpot>();
        List<ExpansionZone> expansiones = new List<ExpansionZone>();

        buildSpots.Add(CreateBuildSpot(0, "Pasillo central · derecha",
            new Vector3(0f, 0f, 6.4f), 0f, null));
        buildSpots.Add(CreateBuildSpot(1, "Pasillo central · izquierda",
            new Vector3(-4.2f, 0f, 6.4f), 0f, null));

        expansiones.Add(CreateExpansion(0, true, buildSpots));
        expansiones.Add(CreateExpansion(1, false, buildSpots));

        GameObject storeGo = new GameObject("TiendaMuebles");
        StoreManager tienda = storeGo.AddComponent<StoreManager>();
        if (tienda.spots == null) tienda.spots = new List<BuildSpot>();
        if (tienda.expansiones == null) tienda.expansiones = new List<ExpansionZone>();
        if (tienda.productos == null) tienda.productos = new List<ProductData>();
        tienda.spots.AddRange(buildSpots);
        tienda.expansiones.AddRange(expansiones);
        tienda.productos.AddRange(new[]
        {
            arroz, frijoles, cafe, leche, azucar, aceite, pan, tomate
        });
        tienda.clientesBase = 4;
        manager.tienda = tienda;

        // Tablet de gestión junto a la entrada de la tienda (abre la misma pantalla).
        CreateStoreTablet(woodMat);


        // --- Zona de almacén: 4 estantes metálicos con 2 cajas cada uno.
        // El triciclo deja la mercancía en las cajas; el jugador o un empleado la
        // recoge y la lleva a las estanterías de venta. El triciclo no repone. ---
        Material rackMetal = Mat("EstructuraEstanteAlmacen", new Color(0.42f, 0.47f, 0.52f));
        Material rackTablero = Mat("TableroEstanteAlmacen", new Color(0.55f, 0.42f, 0.28f));
        Material rackBulto = Mat("BultoEstanteAlmacen", new Color(0.70f, 0.54f, 0.34f));
        float[] rackXs = { -6.9f, -2.9f, 1.1f, 5.1f };
        ProductData[][] rackProductos =
        {
            new[] { arroz, frijoles },
            new[] { cafe, leche },
            new[] { azucar, aceite },
            new[] { pan, tomate }
        };
        string[] rackNombres = { "Arroz", "Frijoles", "Café", "Leche", "Azúcar", "Aceite", "Pan", "Tomate" };

        for (int r = 0; r < rackXs.Length; r++)
        {
            CreateStorageRack("Estante almacen " + (r + 1), new Vector3(rackXs[r], 0f, -5f),
                rackMetal, rackTablero, rackBulto);

            // Tablero inferior a 0,5 m: la caja jugable se apoya encima.
            CreateDelivery("Pedido " + rackNombres[r * 2],
                new Vector3(rackXs[r] - 0.65f, 0.5f, -5f), rackProductos[r][0], boxMat);
            CreateDelivery("Pedido " + rackNombres[r * 2 + 1],
                new Vector3(rackXs[r] + 0.65f, 0.5f, -5f), rackProductos[r][1], boxMat);
        }

        // --- Computadora de pedidos del almacén (estilo Supermarket Simulator). ---
        CreateOrderComputer(catalogo, woodMat);

        // --- Caja registradora ---
        Checkout checkout = CreateCheckout(new Vector3(5f, 0f, -0.5f), woodMat, cintaMat, lectorMat); // junto al almacén, sin tapar su puerta
        manager.checkout = checkout;

        // --- Triciclo del proveedor: modelo 3D detallado hecho con primitivas ---
        Material triMat = Mat("Triciclo", new Color(0.14f, 0.40f, 0.70f));
        Material triClaroMat = Mat("TricicloClaro", new Color(0.28f, 0.62f, 0.88f));
        Material ruedaMat = Mat("RuedaTriciclo", new Color(0.08f, 0.09f, 0.11f));
        Material metalMat = Mat("MetalTriciclo", new Color(0.70f, 0.73f, 0.77f));
        Material cargaMat = Mat("CargaTriciclo", new Color(0.94f, 0.72f, 0.18f));
        Material asientoMat = Mat("AsientoTriciclo", new Color(0.12f, 0.13f, 0.16f));
        Material faroMat = Mat("FaroTriciclo", new Color(1f, 0.92f, 0.45f));
        Material pielMat = Mat("PielRepartidor", new Color(0.85f, 0.65f, 0.48f));

        GameObject tri = new GameObject("TricicloReparto");
        // El triciclo pertenece a la carretera exterior: solo trae productos al almacén.
        tri.transform.position = new Vector3(12f, 0f, -11.2f);
        tri.transform.rotation = Quaternion.LookRotation(Vector3.right); // morro hacia la izquierda

        // Caja de carga trasera (suelo, 4 paredes y bordes metálicos)
        CubeChild(tri, "CajaCarga_Suelo", new Vector3(0f, 0.70f, 0.35f), new Vector3(1.5f, 0.08f, 1.15f), cargaMat);
        CubeChild(tri, "CajaCarga_Izq", new Vector3(-0.75f, 0.98f, 0.35f), new Vector3(0.08f, 0.55f, 1.15f), triMat);
        CubeChild(tri, "CajaCarga_Der", new Vector3(0.75f, 0.98f, 0.35f), new Vector3(0.08f, 0.55f, 1.15f), triMat);
        CubeChild(tri, "CajaCarga_Frente", new Vector3(0f, 0.98f, -0.22f), new Vector3(1.5f, 0.55f, 0.08f), triClaroMat);
        CubeChild(tri, "CajaCarga_Atras", new Vector3(0f, 0.98f, 0.92f), new Vector3(1.5f, 0.55f, 0.08f), triMat);
        CubeChild(tri, "CajaCarga_BordeIzq", new Vector3(-0.75f, 1.27f, 0.35f), new Vector3(0.10f, 0.06f, 1.15f), metalMat);
        CubeChild(tri, "CajaCarga_BordeDer", new Vector3(0.75f, 1.27f, 0.35f), new Vector3(0.10f, 0.06f, 1.15f), metalMat);

        // Chasis tubular y travesaños
        CylinderChild(tri, "Chasis_Izq", new Vector3(-0.45f, 0.55f, -0.15f), new Vector3(90f, 0f, 0f), new Vector3(0.09f, 0.85f, 0.09f), metalMat);
        CylinderChild(tri, "Chasis_Der", new Vector3(0.45f, 0.55f, -0.15f), new Vector3(90f, 0f, 0f), new Vector3(0.09f, 0.85f, 0.09f), metalMat);
        CylinderChild(tri, "Travesano", new Vector3(0f, 0.55f, 0.35f), new Vector3(0f, 0f, 90f), new Vector3(0.07f, 0.50f, 0.07f), metalMat);
        CylinderChild(tri, "TravesanoDelantero", new Vector3(0f, 0.62f, -0.75f), new Vector3(0f, 0f, 90f), new Vector3(0.06f, 0.42f, 0.06f), metalMat);

        // Dirección, horquilla, manubrio, agarres y faro
        CylinderChild(tri, "ColumnaDireccion", new Vector3(0f, 0.85f, -0.95f), new Vector3(12f, 0f, 0f), new Vector3(0.09f, 0.32f, 0.09f), triMat);
        CylinderChild(tri, "Horquilla", new Vector3(0f, 0.58f, -1.05f), Vector3.zero, new Vector3(0.07f, 0.32f, 0.07f), metalMat);
        CylinderChild(tri, "Manubrio", new Vector3(0f, 1.18f, -1.02f), new Vector3(0f, 0f, 90f), new Vector3(0.05f, 0.55f, 0.05f), metalMat);
        CylinderChild(tri, "AgarreIzq", new Vector3(-0.52f, 1.18f, -1.02f), new Vector3(0f, 0f, 90f), new Vector3(0.07f, 0.12f, 0.07f), asientoMat);
        CylinderChild(tri, "AgarreDer", new Vector3(0.52f, 1.18f, -1.02f), new Vector3(0f, 0f, 90f), new Vector3(0.07f, 0.12f, 0.07f), asientoMat);
        SphereChild(tri, "Faro", new Vector3(0f, 1.02f, -1.28f), new Vector3(0.26f, 0.26f, 0.18f), faroMat);

        // Asiento, pedales y guardabarros
        CylinderChild(tri, "PosteAsiento", new Vector3(0f, 0.90f, -0.55f), Vector3.zero, new Vector3(0.06f, 0.25f, 0.06f), metalMat);
        CubeChild(tri, "Asiento", new Vector3(0f, 1.18f, -0.55f), new Vector3(0.52f, 0.12f, 0.38f), asientoMat);
        CylinderChild(tri, "EjePedales", new Vector3(0f, 0.38f, -0.32f), new Vector3(0f, 0f, 90f), new Vector3(0.06f, 0.28f, 0.06f), metalMat);
        CubeChild(tri, "PedalIzq", new Vector3(-0.30f, 0.28f, -0.32f), new Vector3(0.18f, 0.05f, 0.24f), asientoMat);
        CubeChild(tri, "PedalDer", new Vector3(0.30f, 0.48f, -0.32f), new Vector3(0.18f, 0.05f, 0.24f), asientoMat);
        CubeChild(tri, "GuardabarroDelantero", new Vector3(0f, 0.76f, -1.05f), new Vector3(0.50f, 0.05f, 0.85f), triMat);
        CubeChild(tri, "GuardabarroTraseroIzq", new Vector3(-0.62f, 0.76f, 0.55f), new Vector3(0.30f, 0.05f, 0.85f), triMat);
        CubeChild(tri, "GuardabarroTraseroDer", new Vector3(0.62f, 0.76f, 0.55f), new Vector3(0.30f, 0.05f, 0.85f), triMat);

        // Repartidor con brazos hacia el manubrio
        GameObject repartidor = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        repartidor.name = "Repartidor";
        repartidor.transform.SetParent(tri.transform, false);
        repartidor.transform.localPosition = new Vector3(0f, 1.42f, -0.58f);
        repartidor.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        Object.DestroyImmediate(repartidor.GetComponent<Collider>());
        repartidor.GetComponent<Renderer>().sharedMaterial = cargaMat;

        GameObject cabeza = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cabeza.name = "CabezaRepartidor";
        cabeza.transform.SetParent(tri.transform, false);
        cabeza.transform.localPosition = new Vector3(0f, 1.82f, -0.58f);
        cabeza.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
        Object.DestroyImmediate(cabeza.GetComponent<Collider>());
        cabeza.GetComponent<Renderer>().sharedMaterial = pielMat;

        CylinderChild(tri, "BrazoIzq", new Vector3(-0.30f, 1.42f, -0.82f), new Vector3(35f, 0f, -20f), new Vector3(0.05f, 0.30f, 0.05f), pielMat);
        CylinderChild(tri, "BrazoDer", new Vector3(0.30f, 1.42f, -0.82f), new Vector3(35f, 0f, 20f), new Vector3(0.05f, 0.30f, 0.05f), pielMat);

        // Ruedas: neumático, rin, cubo y 6 radios cada una
        Transform ruedaDelantera = CreateWheel(tri, "RuedaDelantera", new Vector3(0f, 0.36f, -1.05f), ruedaMat, metalMat, triMat, metalMat);
        Transform ruedaTraseraIzq = CreateWheel(tri, "RuedaTraseraIzq", new Vector3(-0.62f, 0.36f, 0.55f), ruedaMat, metalMat, triMat, metalMat);
        Transform ruedaTraseraDer = CreateWheel(tri, "RuedaTraseraDer", new Vector3(0.62f, 0.36f, 0.55f), ruedaMat, metalMat, triMat, metalMat);
        Transform[] ruedas = { ruedaDelantera, ruedaTraseraIzq, ruedaTraseraDer };

        GameObject w1 = new GameObject("RutaTriciclo_Carretera");
        w1.transform.position = new Vector3(12f, 0f, -11.2f);
        GameObject w2 = new GameObject("RutaTriciclo_CarreteraCentro");
        w2.transform.position = new Vector3(4f, 0f, -11.2f);
        GameObject w3 = new GameObject("RutaTriciclo_Muelle");
        w3.transform.position = new Vector3(0f, 0f, -10.2f);

        // El triciclo se queda en el muelle exterior; la carga entra al almacén, no el vehículo.
        DeliveryTricycle triciclo = tri.AddComponent<DeliveryTricycle>();
        triciclo.waypoints = new[] { w1.transform, w2.transform, w3.transform };
        triciclo.ruedas = ruedas;
        triciclo.speed = 3.5f;
        manager.triciclo = triciclo;
        tri.SetActive(false);

        // --- Grid de navegación (los clientes esquivan estanterías) ---
        // Cubre también las alas Este/Oeste para que los muebles nuevos sean accesibles.
        GameObject navGo = new GameObject("NavigationGrid");
        NavigationGrid nav = navGo.AddComponent<NavigationGrid>();
        nav.min = new Vector2(-16.5f, -7f);
        nav.max = new Vector2(16.5f, 11f);
        nav.Build();

        // --- Clientes ---
        GameObject spawnerGo = new GameObject("SpawnerClientes");
        spawnerGo.transform.position = new Vector3(-8f, 0f, 10.5f); // junto a la entrada ampliada, dentro de la tienda
        CustomerSpawner spawner = spawnerGo.AddComponent<CustomerSpawner>();
        GameObject salida = new GameObject("Salida");
        salida.transform.position = new Vector3(-8f, 0f, 10.8f); // vuelven a salir por la misma puerta
        spawner.exitPoint = salida.transform;
        spawner.interval = 7f;
        spawner.maxCustomers = 4;
        manager.spawner = spawner;

        // --- Empleados repositores (inactivos hasta contratarlos) ---
        for (int i = 0; i < manager.maxEmpleados; i++)
        {
            GameObject empGo = new GameObject("Empleado " + (i + 1));
            empGo.transform.position = new Vector3(-6f + i * 2f, 0f, -4.6f); // dentro del almacén, tras la puerta

            GameObject cuerpo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cuerpo.name = "Cuerpo";
            cuerpo.transform.SetParent(empGo.transform, false);
            cuerpo.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            cuerpo.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            cuerpo.GetComponent<Renderer>().sharedMaterial = empleadoMat;
            Object.DestroyImmediate(cuerpo.GetComponent<Collider>());

            GameObject delantal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            delantal.name = "Delantal";
            delantal.transform.SetParent(empGo.transform, false);
            delantal.transform.localPosition = new Vector3(0f, 0.85f, -0.28f);
            delantal.transform.localScale = new Vector3(0.38f, 0.5f, 0.05f);
            delantal.GetComponent<Renderer>().sharedMaterial = delantalMat;
            Object.DestroyImmediate(delantal.GetComponent<Collider>());

            Employee emp = empGo.AddComponent<Employee>();
            emp.id = i;
            manager.employeesNpc.Add(emp);
            empGo.SetActive(false);
        }

        // --- Jugador ---
        GameObject playerGo = new GameObject("Jugador");
        playerGo.transform.position = new Vector3(0f, 0f, -1.5f);
        CharacterController cc = playerGo.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.35f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.stepOffset = 0.3f;

        GameObject camGo = new GameObject("Camara");
        camGo.transform.SetParent(playerGo.transform, false);
        camGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        Camera cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 70f;
        camGo.AddComponent<AudioListener>();

        GameObject carried = GameObject.CreatePrimitive(PrimitiveType.Cube);
        carried.name = "Producto en mano";
        carried.transform.SetParent(camGo.transform, false);
        carried.transform.localPosition = new Vector3(0.5f, -0.42f, 0.9f);
        carried.transform.localScale = new Vector3(0.22f, 0.34f, 0.22f);
        Object.DestroyImmediate(carried.GetComponent<Collider>());
        carried.SetActive(false);

        PlayerController player = playerGo.AddComponent<PlayerController>();
        player.playerCamera = cam;
        player.carriedVisual = carried;

        // --- Interfaz ---
        BuildUI(player);

        // --- Guardar ---
        EditorSceneManager.MarkSceneDirty(scene);
        string path = Scenes + "/DemoMypime.unity";
        EditorSceneManager.SaveScene(scene, path);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
        Selection.activeGameObject = playerGo;

        Debug.Log("Escena demo generada en: " + path);
    }

    // ---------------------------------------------------------------- helpers

    static void EnsureFolder(string parent, string name)
    {
        string path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
    }

    static Material Mat(string nombre, Color color)
    {
        string path = Materials + "/" + nombre + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = MaterialFactory.CurrentShader;
            if (shader == null) shader = Shader.Find("Standard");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static ProductData Product(string nombre, float precioUSD, float costeUSD, Color color, Vector3 size, Material mat,
        bool frio = false, float demanda = 1f)
    {
        string path = Data + "/" + nombre + ".asset";
        ProductData p = AssetDatabase.LoadAssetAtPath<ProductData>(path);
        if (p == null)
        {
            p = ScriptableObject.CreateInstance<ProductData>();
            AssetDatabase.CreateAsset(p, path);
        }
        p.nombre = nombre;
        p.precioUSD = precioUSD;
        p.costePedidoUSD = costeUSD;
        p.precio = Currency.CUP(precioUSD);
        p.costePedido = Currency.CUP(costeUSD);
        p.color = color;
        p.tamaño = size;
        p.material = mat;
        p.necesitaFrio = frio;
        p.demanda = demanda;
        EditorUtility.SetDirty(p);
        return p;
    }

    /// <summary>Crea (o actualiza) el catálogo de productos pedibles de la computadora.</summary>
    static OrderCatalog CreateCatalogo(ProductData[] productos)
    {
        string path = Data + "/CatalogoPedidos.asset";
        OrderCatalog catalogo = AssetDatabase.LoadAssetAtPath<OrderCatalog>(path);
        if (catalogo == null)
        {
            catalogo = ScriptableObject.CreateInstance<OrderCatalog>();
            AssetDatabase.CreateAsset(catalogo, path);
        }

        List<OrderCatalog.Entry> anteriores = new List<OrderCatalog.Entry>(catalogo.productos);
        catalogo.productos.Clear();
        foreach (ProductData p in productos)
        {
            if (p == null) continue;
            OrderCatalog.Entry anterior = anteriores.Find(e => e != null && e.producto == p);
            catalogo.productos.Add(new OrderCatalog.Entry
            {
                producto = p,
                activo = anterior == null || anterior.activo,
                lote = 8,
                maximo = 32
            });
        }
        EditorUtility.SetDirty(catalogo);
        return catalogo;
    }

    static GameObject Cube(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        SetMat(go, mat);
        return go;
    }

    static TextMesh Texto(string name, string contenido, Vector3 pos, float characterSize, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.position = pos;
        TextMesh txt = go.AddComponent<TextMesh>();
        txt.text = contenido;
        txt.fontSize = 64;
        txt.characterSize = characterSize;
        txt.anchor = TextAnchor.MiddleCenter;
        txt.alignment = TextAlignment.Center;
        txt.color = color;
        return txt;
    }

    static GameObject CubeChild(GameObject parent, string name, Vector3 localPos, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        SetMat(go, mat);
        return go;
    }

    static GameObject CylinderChild(GameObject parent, string name, Vector3 localPos,
        Vector3 localEuler, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localEulerAngles = localEuler;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        SetMat(go, mat);
        return go;
    }

    static GameObject SphereChild(GameObject parent, string name, Vector3 localPos,
        Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        SetMat(go, mat);
        return go;
    }

    /// <summary>
    /// Instancia un modelo exportado desde Cartoon_Supermarket_Collection.blend.
    /// Los FBX están centrados en X/Z y con la base en Y=0, listos para colocar en el suelo.
    /// Se quitan todos los colisionadores para no alterar las rutas A* ni la navegación.
    /// </summary>
    static GameObject CartoonModel(string archivo, string nombre, Vector3 pos, float yaw = 0f, float escala = 1f)
    {
        string path = CartoonModels + "/" + archivo + ".fbx";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning("Modelo cartoon no encontrado: " + path);
            return null;
        }
        GameObject go = Object.Instantiate(prefab);
        go.name = nombre;
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        go.transform.localScale = Vector3.one * escala;
        foreach (Collider c in go.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(c);
        return go;
    }

    /// <summary>
    /// Coloca un modelo cartoon sobre un objeto jugable (estantería, nevera o caja)
    /// manteniendo la lógica y los puntos de interacción del objeto original.
    /// </summary>
    static void AdjuntarModelo(GameObject objetivo, string archivo, float yaw = 0f)
    {
        if (objetivo == null) return;
        GameObject modelo = CartoonModel(archivo, archivo + " (" + objetivo.name + ")",
            objetivo.transform.position, yaw);
        if (modelo == null) return;
        modelo.transform.SetParent(objetivo.transform, false);
        modelo.transform.localPosition = Vector3.zero;
        modelo.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
    }

    /// <summary>
    /// Distribución de los elementos del collection de Blender por la tienda.
    /// Sustituye a los primitivos genéricos por los modelos cartoon reales.
    /// </summary>
    static void CreateCartoonSupermarket()
    {
        // Los 8 estantes, la nevera y la caja jugables ya recibieron su modelo
        // en CreateShelf/CreateCheckout. Aquí solo queda la decoración suelta.

        // Zona fría y mesa de frescos, reordenadas en el pasillo central.
        CartoonModel("Congelador", "Congelador (cartoon)", new Vector3(5.6f, 0f, 6.2f));
        CartoonModel("Vitrina_Frescos", "Vitrina frescos (cartoon)", new Vector3(8.0f, 0f, 4.6f));
        CartoonModel("Mesa_Productos", "Mesa productos (cartoon)", new Vector3(-3.5f, 0f, 3.2f));

        // Mostrador de devoluciones cerca de la caja jugable (x=5, z=-0.5).
        CartoonModel("Devolucion", "Mostrador devoluciones (cartoon)", new Vector3(2.6f, 0f, -0.6f), 180f);

        // Zona de entrada.
        CartoonModel("Carrito", "Carrito compra (cartoon)", new Vector3(-7.2f, 0f, 10.4f), 180f);
        CartoonModel("Cesta_Mano", "Cesta mano (cartoon)", new Vector3(-6.2f, 0f, 10.4f));
        CartoonModel("Expositor_Oferta", "Expositor oferta (cartoon)", new Vector3(2.6f, 0f, 10.6f));

        // Señalización de pasillos.
        CartoonModel("Senal_Pasillo", "Senal pasillo (cartoon)", new Vector3(-5f, 0f, 8.6f));
        CartoonModel("Senal_Promo", "Senal promo (cartoon)", new Vector3(0f, 0f, 8.6f));
        CartoonModel("Senal_Salida", "Senal salida (cartoon)", new Vector3(4.5f, 0f, 8.6f));

        // Cajas de frescos sobre la mesa de productos (altura 0,72 m).
        CartoonModel("Caja_Manzanas", "Caja manzanas (cartoon)", new Vector3(-6.2f, 0.72f, 3.2f));
        CartoonModel("Caja_Naranjas", "Caja naranjas (cartoon)", new Vector3(-5.0f, 0.72f, 3.2f));
        CartoonModel("Caja_Verduras", "Caja verduras (cartoon)", new Vector3(-3.8f, 0.72f, 3.2f));

        // Productos sueltos sobre la mesa.
        CartoonModel("Bolsa_Snacks", "Bolsa snacks (cartoon)", new Vector3(-1.6f, 0.72f, 3.2f));
        CartoonModel("Caja_Cereales", "Caja cereales (cartoon)", new Vector3(-1.2f, 0.72f, 3.2f));
        CartoonModel("Brik_Leche", "Brik leche (cartoon)", new Vector3(-0.8f, 0.72f, 3.2f));
        CartoonModel("Botella_Zumo", "Botella zumo (cartoon)", new Vector3(-0.4f, 0.72f, 3.2f));
        CartoonModel("Lata_Refresco", "Lata refresco (cartoon)", new Vector3(0f, 0.72f, 3.2f));
        CartoonModel("Pack_Agua", "Pack agua (cartoon)", new Vector3(0.6f, 0.72f, 3.2f));

        // Cajas de cartón en el almacén (el blend no incluye este elemento).
        Material carton = Mat("CartonAlmacen", new Color(0.68f, 0.50f, 0.30f));
        for (int i = 0; i < 4; i++)
            Cube("Caja carton " + (i + 1),
                new Vector3(-4.5f + (i % 2) * 0.7f, 0.35f + (i / 2) * 0.7f, -6.4f),
                new Vector3(0.65f, 0.65f, 0.65f), carton);
    }

    /// <summary>
    /// Añade puesto de frutas y verduras, panadería, carritos y carteles de sección.
    /// Todo se crea sin colisionadores para no cambiar las rutas A* de clientes y empleados.
    /// </summary>
    static void CreateSupermarketDecor()
    {
        Material madera = Mat("DecorMadera", new Color(0.72f, 0.56f, 0.36f));
        Material cesta = Mat("DecorCesta", new Color(0.90f, 0.87f, 0.78f));
        Material rojo = Mat("DecorTomate", new Color(0.86f, 0.18f, 0.15f));
        Material verde = Mat("DecorLechuga", new Color(0.28f, 0.68f, 0.28f));
        Material amarillo = Mat("DecorPlatano", new Color(0.96f, 0.79f, 0.18f));
        Material pan = Mat("DecorPan", new Color(0.84f, 0.58f, 0.30f));
        Material alambre = Mat("DecorAlambre", new Color(0.66f, 0.70f, 0.74f));
        Material neumatico = Mat("DecorRuedaCarrito", new Color(0.12f, 0.13f, 0.15f));

        // --- Puesto de frutas y verduras ---
        GameObject frutas = new GameObject("Zona Frutas y Verduras");
        frutas.transform.position = new Vector3(-5.5f, 0f, 3.2f);
        CubeChild(frutas, "Mesa", new Vector3(0f, 0.45f, 0f), new Vector3(2.8f, 0.12f, 1.0f), madera);
        CubeChild(frutas, "PataIzq", new Vector3(-1.22f, 0.22f, 0f), new Vector3(0.12f, 0.44f, 0.9f), madera);
        CubeChild(frutas, "PataDer", new Vector3(1.22f, 0.22f, 0f), new Vector3(0.12f, 0.44f, 0.9f), madera);
        Material[] coloresFruta = { rojo, verde, amarillo };
        for (int i = 0; i < 3; i++)
        {
            float x = -0.88f + i * 0.88f;
            CubeChild(frutas, "Caja " + (i + 1), new Vector3(x, 0.58f, 0f), new Vector3(0.76f, 0.16f, 0.74f), cesta);
            for (int j = 0; j < 4; j++)
                SphereChild(frutas, "Fruta " + (i + 1) + "-" + (j + 1),
                    new Vector3(x - 0.18f + (j % 2) * 0.36f, 0.76f, -0.16f + (j / 2) * 0.32f),
                    new Vector3(0.22f, 0.22f, 0.22f), coloresFruta[i]);
        }

        // --- Panadería ---
        GameObject panaderia = new GameObject("Zona Panaderia");
        panaderia.transform.position = new Vector3(5.5f, 0f, 3.8f);
        CubeChild(panaderia, "Mostrador", new Vector3(0f, 0.5f, 0f), new Vector3(2.8f, 1.0f, 1.0f), madera);
        CubeChild(panaderia, "Cristal", new Vector3(0f, 1.18f, -0.38f), new Vector3(2.6f, 0.5f, 0.04f),
            Mat("DecorCristalPan", new Color(0.80f, 0.90f, 0.95f)));
        for (int i = 0; i < 5; i++)
            SphereChild(panaderia, "Pan " + (i + 1), new Vector3(-0.9f + i * 0.45f, 1.12f, -0.10f),
                new Vector3(0.34f, 0.22f, 0.26f), pan);

        // --- Carritos de compra junto a la entrada ---
        GameObject carritos = new GameObject("Carritos de compra");
        carritos.transform.position = new Vector3(-7f, 0f, 11.2f);
        for (int i = 0; i < 3; i++)
        {
            GameObject carro = new GameObject("Carrito " + (i + 1));
            carro.transform.SetParent(carritos.transform, false);
            carro.transform.localPosition = new Vector3(0f, 0f, -i * 0.58f);
            CubeChild(carro, "Cesta", new Vector3(0f, 0.58f, 0f), new Vector3(0.9f, 0.5f, 1.15f), alambre);
            CubeChild(carro, "Asa", new Vector3(0f, 0.98f, -0.66f), new Vector3(0.85f, 0.06f, 0.06f), alambre);
            for (int w = 0; w < 4; w++)
                CylinderChild(carro, "Rueda " + (w + 1),
                    new Vector3(w % 2 == 0 ? -0.38f : 0.38f, 0.16f, w < 2 ? -0.42f : 0.42f),
                    new Vector3(0f, 0f, 90f), new Vector3(0.28f, 0.06f, 0.28f), neumatico);
        }

        // --- Nevera de bebidas (puerta de cristal y botellas de colores) ---
        Material cristalFrio = Mat("DecorCristalFrio", new Color(0.72f, 0.88f, 0.95f));
        Material[] botellas = {
            Mat("DecorBotellaRoja", new Color(0.80f, 0.12f, 0.12f)),
            Mat("DecorBotellaVerde", new Color(0.16f, 0.52f, 0.24f)),
            Mat("DecorBotellaAzul", new Color(0.14f, 0.36f, 0.78f))
        };
        GameObject bebidas = new GameObject("Nevera Bebidas");
        bebidas.transform.position = new Vector3(-2.5f, 0f, 4.4f);
        CubeChild(bebidas, "Cuerpo", new Vector3(0f, 1.1f, 0f), new Vector3(2.2f, 2.2f, 0.9f), alambre);
        CubeChild(bebidas, "Cristal", new Vector3(0f, 1.15f, -0.47f), new Vector3(2.0f, 1.9f, 0.05f), cristalFrio);
        for (int n = 0; n < 3; n++)
        {
            CubeChild(bebidas, "Balda " + (n + 1), new Vector3(0f, 0.55f + n * 0.6f, -0.25f),
                new Vector3(2.0f, 0.06f, 0.6f), alambre);
            for (int b = 0; b < 5; b++)
                CylinderChild(bebidas, "Botella " + n + "-" + b,
                    new Vector3(-0.8f + b * 0.4f, 0.78f + n * 0.6f, -0.25f),
                    Vector3.zero, new Vector3(0.12f, 0.22f, 0.12f), botellas[(n + b) % 3]);
        }

        // --- Congelados: arca horizontal con tapa de cristal ---
        GameObject congelados = new GameObject("Congelados");
        congelados.transform.position = new Vector3(1.6f, 0f, 4.4f);
        CubeChild(congelados, "Arca", new Vector3(0f, 0.55f, 0f), new Vector3(2.6f, 1.1f, 1.2f), alambre);
        CubeChild(congelados, "TapaCristal", new Vector3(0f, 1.14f, 0f), new Vector3(2.5f, 0.06f, 1.1f), cristalFrio);
        Material[] helados = {
            Mat("DecorHeladoBlanco", new Color(0.95f, 0.95f, 0.92f)),
            Mat("DecorHeladoChocolate", new Color(0.45f, 0.24f, 0.14f)),
            Mat("DecorHeladoFresa", new Color(0.92f, 0.44f, 0.48f))
        };
        for (int i = 0; i < 6; i++)
            CubeChild(congelados, "Helado " + (i + 1),
                new Vector3(-0.95f + (i % 3) * 0.95f, 0.86f, -0.28f + (i / 3) * 0.56f),
                new Vector3(0.55f, 0.30f, 0.38f), helados[i % 3]);

        // --- Estante central de ofertas (decorativo, sin colisionador) ---
        GameObject ofertas = new GameObject("Estante Ofertas");
        ofertas.transform.position = new Vector3(0f, 0f, 1.6f);
        for (int i = 0; i < 3; i++)
            CubeChild(ofertas, "Tablero " + (i + 1), new Vector3(0f, 0.35f + i * 0.6f, 0f),
                new Vector3(2.0f, 0.08f, 0.7f), madera);
        CubeChild(ofertas, "LateralIzq", new Vector3(-0.95f, 0.95f, 0f), new Vector3(0.08f, 1.9f, 0.7f), madera);
        CubeChild(ofertas, "LateralDer", new Vector3(0.95f, 0.95f, 0f), new Vector3(0.08f, 1.9f, 0.7f), madera);
        for (int i = 0; i < 6; i++)
            CubeChild(ofertas, "Producto " + (i + 1),
                new Vector3(-0.7f + (i % 3) * 0.7f, 0.58f + (i / 3) * 0.6f, 0f),
                new Vector3(0.28f, 0.34f, 0.28f), coloresFruta[i % 3]);

        // --- Cajas de cartón apiladas junto al almacén ---
        Material carton = Mat("DecorCarton", new Color(0.68f, 0.50f, 0.30f));
        GameObject pila = new GameObject("Cajas Carton");
        pila.transform.position = new Vector3(-4.5f, 0f, -2.4f);
        for (int i = 0; i < 4; i++)
            CubeChild(pila, "Caja " + (i + 1),
                new Vector3((i % 2) * 0.7f - 0.35f, 0.35f + (i / 2) * 0.7f, 0f),
                new Vector3(0.65f, 0.65f, 0.65f), carton);

        // --- Segunda caja registradora decorativa (la jugable sigue siendo manager.checkout) ---
        GameObject caja2 = new GameObject("Caja 2 decorativa");
        caja2.transform.position = new Vector3(-6f, 0f, -0.5f);
        CubeChild(caja2, "Mostrador", new Vector3(0f, 0.5f, 0f), new Vector3(2.4f, 1f, 1f), madera);
        CubeChild(caja2, "Cinta", new Vector3(-0.2f, 1.03f, 0f), new Vector3(1.9f, 0.06f, 0.7f), neumatico);
        CubeChild(caja2, "Lector", new Vector3(0.95f, 1.12f, 0f), new Vector3(0.45f, 0.22f, 0.45f), verde);
    }

    /// <summary>Rueda completa; el Transform devuelto es el que gira en DeliveryTricycle.</summary>
    static Transform CreateWheel(GameObject parent, string name, Vector3 localPos,
        Material tire, Material rim, Material hub, Material spoke)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent.transform, false);
        root.transform.localPosition = localPos;

        CylinderChild(root, "Neumatico", Vector3.zero, new Vector3(0f, 0f, 90f),
            new Vector3(0.72f, 0.10f, 0.72f), tire);
        CylinderChild(root, "Rin", Vector3.zero, new Vector3(0f, 0f, 90f),
            new Vector3(0.56f, 0.115f, 0.56f), rim);
        CylinderChild(root, "Cubo", Vector3.zero, new Vector3(0f, 0f, 90f),
            new Vector3(0.18f, 0.14f, 0.18f), hub);

        for (int i = 0; i < 6; i++)
        {
            GameObject radio = GameObject.CreatePrimitive(PrimitiveType.Cube);
            radio.name = "Radio " + (i + 1);
            radio.transform.SetParent(root.transform, false);
            radio.transform.localRotation = Quaternion.Euler(i * 30f, 0f, 0f);
            radio.transform.localScale = new Vector3(0.025f, 0.30f, 0.025f);
            Object.DestroyImmediate(radio.GetComponent<Collider>());
            SetMat(radio, spoke);
        }
        return root.transform;
    }

    static void SetMat(GameObject go, Material mat)
    {
        if (mat == null) return;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = mat;
    }

    static Shelf CreateShelf(string name, Vector3 pos, ProductData product, int stock, Material mat,
        bool nevera = false, Material cristal = null, Material indicador = null)
    {
        GameObject go = new GameObject(name);
        go.transform.position = pos;

        BoxCollider col = go.AddComponent<BoxCollider>();
        col.size = new Vector3(2.4f, 1.9f, 0.8f);
        col.center = new Vector3(0f, 0.95f, 0f);

        // La malla del mueble la aporta el modelo cartoon; aquí solo queda la lógica,
        // los huecos de producto y el indicador de la nevera.
        GameObject luzIndicador = null;
        if (nevera)
            luzIndicador = CubeChild(go, "IndicadorNevera", new Vector3(0.48f, 2.08f, -0.2f),
                new Vector3(0.14f, 0.14f, 0.14f), indicador);

        // 8 huecos para los productos (2 filas x 4 columnas), mirando al interior (-Z)
        Transform[] slots = new Transform[8];
        int idx = 0;
        for (int row = 0; row < 2; row++)
        {
            for (int c = 0; c < 4; c++)
            {
                GameObject slot = new GameObject("Slot_" + idx);
                slot.transform.SetParent(go.transform, false);
                slot.transform.localPosition = new Vector3(-0.42f + c * 0.28f, 0.62f + row * 0.70f, -0.36f);
                slots[idx++] = slot.transform;
            }
        }

        GameObject spot = new GameObject("PuntoCliente");
        spot.transform.SetParent(go.transform, false);
        spot.transform.localPosition = new Vector3(0f, 0f, -1.5f);

        Shelf shelf = go.AddComponent<Shelf>();
        shelf.product = product;
        shelf.capacity = 8;
        shelf.stock = stock;
        shelf.slots = slots;
        shelf.customerSpot = spot.transform;
        if (nevera)
        {
            shelf.esNevera = true;
            shelf.costeLuz = Currency.CUP(0.5f);
            shelf.indicadorLuz = luzIndicador;
        }
        shelf.RefreshVisuals();
        return shelf;
    }

    static void CreateDelivery(string name, Vector3 pos, ProductData product, Material mat)
    {
        GameObject go = new GameObject(name);
        go.transform.position = pos;

        BoxCollider col = go.AddComponent<BoxCollider>();
        col.size = new Vector3(0.9f, 0.6f, 0.9f);
        col.center = new Vector3(0f, 0.3f, 0f);

        CubeChild(go, "Caja", new Vector3(0f, 0.3f, 0f), new Vector3(0.9f, 0.6f, 0.9f), mat);
        GameObject label = CubeChild(go, "Etiqueta", new Vector3(0f, 0.62f, 0f),
            new Vector3(0.5f, 0.03f, 0.5f), product.material);

        DeliveryBox box = go.AddComponent<DeliveryBox>();
        box.product = product;
        box.amount = 8;
        box.labelVisual = label;
    }

    /// <summary>Mesa y monitor de la computadora desde la que se hacen pedidos al proveedor.</summary>
    static void CreateOrderComputer(OrderCatalog catalogo, Material wood)
    {
        GameObject go = new GameObject("ComputadoraPedidos");
        go.transform.position = new Vector3(7.5f, 0f, -4.2f);

        CubeChild(go, "Mesa", new Vector3(0f, 0.38f, 0f), new Vector3(1.5f, 0.76f, 0.75f), wood);
        Material pantalla = Mat("PantallaPedidos", new Color(0.10f, 0.42f, 0.55f));
        Material basePc = Mat("BaseComputadora", new Color(0.14f, 0.16f, 0.19f));
        CubeChild(go, "Monitor", new Vector3(0f, 1.05f, -0.10f), new Vector3(1.05f, 0.68f, 0.10f), basePc);
        CubeChild(go, "Pantalla", new Vector3(0f, 1.06f, -0.17f), new Vector3(0.90f, 0.52f, 0.04f), pantalla);
        CubeChild(go, "Teclado", new Vector3(0f, 0.79f, 0.30f), new Vector3(0.85f, 0.05f, 0.28f), basePc);

        BoxCollider col = go.AddComponent<BoxCollider>();
        col.size = new Vector3(1.7f, 1.5f, 1f);
        col.center = new Vector3(0f, 0.75f, 0f);

        OrderComputer pc = go.AddComponent<OrderComputer>();
        pc.catalogo = catalogo;
    }

    /// <summary>
    /// Estante metálico de almacén con dos niveles. Sus mallas no tienen colisionador
    /// (como el resto de la decoración): solo las cajas jugables bloquean la ruta A*.
    /// </summary>
    static void CreateStorageRack(string name, Vector3 pos, Material metal, Material tablero, Material bulto)
    {
        GameObject go = new GameObject(name);
        go.transform.position = pos;

        const float ancho = 2.6f;
        const float profundo = 1.2f;

        // Cuatro postes y dos bandejas.
        foreach (float x in new[] { -ancho / 2f + 0.06f, ancho / 2f - 0.06f })
            foreach (float z in new[] { -profundo / 2f + 0.06f, profundo / 2f - 0.06f })
                CubeChild(go, "Poste", new Vector3(x, 0.95f, z), new Vector3(0.10f, 1.9f, 0.10f), metal);

        CubeChild(go, "Nivel inferior", new Vector3(0f, 0.45f, 0f),
            new Vector3(ancho, 0.10f, profundo), tablero);
        CubeChild(go, "Nivel superior", new Vector3(0f, 1.45f, 0f),
            new Vector3(ancho, 0.10f, profundo), tablero);

        // Bultos decorativos en el nivel alto: refuerzan el aspecto de almacén.
        for (int i = 0; i < 2; i++)
            CubeChild(go, "Bulto alto " + (i + 1), new Vector3(-0.65f + i * 1.3f, 1.80f, 0f),
                new Vector3(0.75f, 0.60f, 0.80f), bulto);
    }

    /// <summary>Palé con bultos: decoración del almacén donde se deja la mercancía.</summary>
    static void CreatePalletStack(string name, Vector3 pos, Material pallet, Material bulto)
    {
        GameObject go = new GameObject(name);
        go.transform.position = pos;

        BoxCollider col = go.AddComponent<BoxCollider>();
        col.size = new Vector3(1.4f, 0.9f, 1.1f);
        col.center = new Vector3(0f, 0.45f, 0f);

        CubeChild(go, "BasePalet", new Vector3(0f, 0.08f, 0f), new Vector3(1.4f, 0.16f, 1.1f), pallet);
        for (int i = 0; i < 3; i++)
        {
            Vector3 p = new Vector3(-0.32f + (i % 2) * 0.64f, 0.45f + (i / 2) * 0.42f, 0f);
            CubeChild(go, "Bulto " + (i + 1), p, new Vector3(0.55f, 0.38f, 0.80f), bulto);
        }
    }

    /// <summary>
    /// Hueco de obra con cartel: al interactuar se abre la tienda de muebles y,
    /// al comprar, el mueble se construye justo aquí. La zona táctil es un trigger
    /// para que el jugador pueda atravesar el cartel mientras no hay mueble.
    /// </summary>
    static BuildSpot CreateBuildSpot(int id, string etiqueta, Vector3 pos, float yaw, Transform parent)
    {
        Material madera = Mat("ObraMadera", new Color(0.60f, 0.46f, 0.28f));
        Material amarillo = Mat("ObraAmarillo", new Color(0.94f, 0.72f, 0.12f));
        Material negro = Mat("ObraNegro", new Color(0.13f, 0.13f, 0.15f));

        GameObject go = new GameObject("HuecoObra_" + id + "_" + etiqueta);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (parent != null) go.transform.SetParent(parent, true);

        GameObject obra = new GameObject("Obra");
        obra.transform.SetParent(go.transform, false);

        CubeChild(obra, "Base", new Vector3(0f, 0.04f, 0f), new Vector3(2.4f, 0.08f, 0.95f), madera);
        CubeChild(obra, "PosteIzq", new Vector3(-1.05f, 0.70f, 0f), new Vector3(0.08f, 1.4f, 0.08f), madera);
        CubeChild(obra, "PosteDer", new Vector3(1.05f, 0.70f, 0f), new Vector3(0.08f, 1.4f, 0.08f), madera);
        CubeChild(obra, "Cartel", new Vector3(0f, 1.42f, 0f), new Vector3(2.1f, 0.60f, 0.07f), amarillo);
        CubeChild(obra, "FranjaIzq", new Vector3(-0.88f, 1.42f, -0.05f), new Vector3(0.18f, 0.60f, 0.02f), negro);
        CubeChild(obra, "FranjaDer", new Vector3(0.88f, 1.42f, -0.05f), new Vector3(0.18f, 0.60f, 0.02f), negro);

        GameObject textoGo = new GameObject("TextoObra");
        textoGo.transform.SetParent(obra.transform, false);
        textoGo.transform.localPosition = new Vector3(0f, 1.42f, -0.06f);
        textoGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        TextMesh tm = textoGo.AddComponent<TextMesh>();
        tm.text = "OBRA";
        tm.fontSize = 64;
        tm.characterSize = 0.07f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.10f, 0.08f, 0.04f);

        BoxCollider zona = go.AddComponent<BoxCollider>();
        zona.size = new Vector3(2.5f, 2.3f, 1.8f);
        zona.center = new Vector3(0f, 1.15f, 0f);
        zona.isTrigger = true;

        BuildSpot spot = go.AddComponent<BuildSpot>();
        spot.id = id;
        spot.etiqueta = etiqueta;
        spot.obra = obra;
        spot.zona = zona;
        return spot;
    }

    /// <summary>
    /// Ala nueva de 6 × 9 m pegada al local. Nace cerrada por una persiana
    /// metálica; al comprarla se retira y quedan jugables sus huecos de muebles.
    /// </summary>
    static ExpansionZone CreateExpansion(int id, bool este, List<BuildSpot> todos)
    {
        float sx = este ? 1f : -1f;
        string lado = este ? "Este" : "Oeste";

        Material suelo = Mat("SueloAmpliacion", new Color(0.70f, 0.66f, 0.58f));
        Material pared = Mat("Pared", new Color(0.90f, 0.90f, 0.94f));
        Material persiana = Mat("PersianaMetal", new Color(0.66f, 0.70f, 0.74f));
        Material persianaOsc = Mat("PersianaOscura", new Color(0.40f, 0.44f, 0.48f));
        Material cartel = Mat("CartelAmpliacion", new Color(0.78f, 0.56f, 0.16f));

        GameObject root = new GameObject("Ampliacion " + lado);

        Cube("Suelo ampliacion " + lado, new Vector3(sx * 13f, -0.15f, 6.5f),
            new Vector3(6f, 0.3f, 9f), suelo).transform.SetParent(root.transform, true);
        Cube("Pared ampliacion " + lado + " exterior", new Vector3(sx * 16f, 1.75f, 6.5f),
            new Vector3(0.3f, 3.5f, 9f), pared).transform.SetParent(root.transform, true);
        Cube("Pared ampliacion " + lado + " norte", new Vector3(sx * 13f, 1.75f, 11f),
            new Vector3(6f, 3.5f, 0.3f), pared).transform.SetParent(root.transform, true);
        Cube("Pared ampliacion " + lado + " sur", new Vector3(sx * 13f, 1.75f, 2f),
            new Vector3(6f, 3.5f, 0.3f), pared).transform.SetParent(root.transform, true);

        // Persiana que sella el paso hasta que se compre la ampliación.
        GameObject persianaGo = new GameObject("Persiana " + lado);
        persianaGo.transform.position = new Vector3(sx * 10f, 0f, 7.25f);
        persianaGo.transform.SetParent(root.transform, true);
        BoxCollider pcol = persianaGo.AddComponent<BoxCollider>();
        pcol.size = new Vector3(0.36f, 2.35f, 3.5f);
        pcol.center = new Vector3(0f, 1.175f, 0f);
        for (int i = 0; i < 7; i++)
            CubeChild(persianaGo, "Lamina " + (i + 1), new Vector3(0f, 0.22f + i * 0.30f, 0f),
                new Vector3(0.30f, 0.25f, 3.44f), i % 2 == 0 ? persiana : persianaOsc);
        CubeChild(persianaGo, "GuiaIzq", new Vector3(0f, 1.18f, -1.78f),
            new Vector3(0.40f, 2.36f, 0.10f), persianaOsc);
        CubeChild(persianaGo, "GuiaDer", new Vector3(0f, 1.18f, 1.78f),
            new Vector3(0.40f, 2.36f, 0.10f), persianaOsc);
        CubeChild(persianaGo, "CartelObra", new Vector3(-sx * 0.30f, 2.55f, 0f),
            new Vector3(0.10f, 0.55f, 3.1f), cartel);

        TextMesh rotulo = Texto("Rotulo ampliacion " + lado,
            "AMPLIACIÓN " + lado.ToUpperInvariant(),
            new Vector3(sx * 9.58f, 2.56f, 7.25f), 0.075f, new Color(1f, 0.94f, 0.70f));
        rotulo.transform.rotation = Quaternion.Euler(0f, sx > 0f ? -90f : 90f, 0f);
        rotulo.fontStyle = FontStyle.Bold;
        rotulo.gameObject.transform.SetParent(root.transform, true);

        // Dos huecos de mueble dentro del ala, inactivos hasta comprarla.
        BuildSpot a = CreateBuildSpot(10 + id * 2, "Ala " + lado + " · puesto 1",
            new Vector3(sx * 13f, 0f, 9.2f), 0f, root.transform);
        BuildSpot b = CreateBuildSpot(11 + id * 2, "Ala " + lado + " · puesto 2",
            new Vector3(sx * 13f, 0f, 5.6f), 0f, root.transform);
        a.gameObject.SetActive(false);
        b.gameObject.SetActive(false);

        // Zona táctil delante de la persiana: se interactúa para abrir la tienda.
        GameObject zonaGo = new GameObject("Zona ampliacion " + lado);
        zonaGo.transform.position = new Vector3(sx * 8.45f, 0f, 7.25f);
        BoxCollider zcol = zonaGo.AddComponent<BoxCollider>();
        zcol.size = new Vector3(2.6f, 2.8f, 3.7f);
        zcol.center = new Vector3(0f, 1.4f, 0f);
        zcol.isTrigger = true;

        ExpansionZone zona = zonaGo.AddComponent<ExpansionZone>();
        zona.id = id;
        zona.nombre = "Ampliación " + lado;
        zona.descripcion = "6 × 9 m nuevos con 2 huecos de muebles";
        zona.indiceRequerido = este ? -1 : 0;
        zona.clientesExtra = 1;
        if (zona.spots == null) zona.spots = new List<BuildSpot>();
        if (zona.obstaculos == null) zona.obstaculos = new List<GameObject>();
        zona.spots.Add(a);
        zona.spots.Add(b);
        zona.obstaculos.Add(persianaGo);
        zona.obstaculos.Add(rotulo.gameObject);

        if (todos != null)
        {
            todos.Add(a);
            todos.Add(b);
        }
        return zona;
    }

    /// <summary>Tablet sobre una mesa para abrir la tienda sin buscar un hueco de obra.</summary>
    static void CreateStoreTablet(Material wood)
    {
        Material pantalla = Mat("PantallaTienda", new Color(0.12f, 0.56f, 0.66f));
        Material marco = Mat("MarcoTablet", new Color(0.12f, 0.14f, 0.18f));
        Material cartel = Mat("CartelTiendaMuebles", new Color(0.78f, 0.56f, 0.16f));

        GameObject go = new GameObject("TabletTienda");
        go.transform.position = new Vector3(-4.5f, 0f, -0.6f);

        BoxCollider col = go.AddComponent<BoxCollider>();
        col.size = new Vector3(1.1f, 1.65f, 0.7f);
        col.center = new Vector3(0f, 0.82f, 0f);

        CubeChild(go, "Mesa", new Vector3(0f, 0.40f, 0f), new Vector3(1.0f, 0.8f, 0.6f), wood);
        CubeChild(go, "Marco", new Vector3(0f, 1.02f, -0.02f), new Vector3(0.72f, 0.50f, 0.08f), marco);
        CubeChild(go, "Pantalla", new Vector3(0f, 1.02f, -0.07f), new Vector3(0.62f, 0.40f, 0.02f), pantalla);
        CubeChild(go, "Cartel", new Vector3(0f, 1.42f, -0.30f), new Vector3(1.05f, 0.22f, 0.04f), cartel);

        TextMesh tm = Texto("Texto tablet", "TIENDA Y AMPLIACIONES",
            new Vector3(-4.5f, 1.42f, -0.34f), 0.05f, new Color(1f, 0.94f, 0.70f));
        tm.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        tm.fontStyle = FontStyle.Bold;

        go.AddComponent<StoreTablet>();
    }

    static Checkout CreateCheckout(Vector3 pos, Material wood, Material cinta, Material lector)
    {
        GameObject go = new GameObject("Caja registradora");
        go.transform.position = pos;

        BoxCollider col = go.AddComponent<BoxCollider>();
        col.size = new Vector3(2.4f, 1f, 1f);
        col.center = new Vector3(0f, 0.5f, 0f);

        // La malla del mostrador la aporta el modelo cartoon Caja_01.
        AdjuntarModelo(go, "Caja_01", 180f);

        // Huecos donde aparecen los productos que faltan por escanear
        Transform[] slots = new Transform[4];
        float[] xs = { -0.42f, -0.14f, 0.14f, 0.42f };
        for (int i = 0; i < slots.Length; i++)
        {
            GameObject slot = new GameObject("SlotCinta_" + i);
            slot.transform.SetParent(go.transform, false);
            slot.transform.localPosition = new Vector3(xs[i], 1.02f, 0f);
            slots[i] = slot.transform;
        }

        GameObject spot = new GameObject("PuntoCliente");
        spot.transform.SetParent(go.transform, false);
        spot.transform.localPosition = new Vector3(0f, 0f, -1.6f);

        Checkout checkout = go.AddComponent<Checkout>();
        checkout.customerSpot = spot.transform;
        checkout.scanSlots = slots;
        return checkout;
    }

    static void BuildUI(PlayerController player)
    {
        GameObject canvasGo = new GameObject("UI");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        canvasGo.AddComponent<GraphicRaycaster>();

        Font font = null;
        try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        if (font == null) { try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { } }

        Text money = TextUI(canvasGo.transform, "Dinero", "Día 1   46.500 CUP   Ventas: 0   USD: 930,00 CUP   Precios ×1.00",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(25f, -18f), new Vector2(900f, 45f),
            20, TextAnchor.MiddleLeft, font, Color.white);

        Text carried = TextUI(canvasGo.transform, "EnMano", "Manos vacías",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(25f, -62f), new Vector2(460f, 40f),
            19, TextAnchor.MiddleLeft, font, new Color(0.85f, 0.95f, 1f));

        Text goal = TextUI(canvasGo.transform, "Meta", "Meta: 0/27.900 CUP   Alquiler: 18.600   Luz: 465   Sueldos: 0   Rep: 70%   Atendidos: 0   Satisfechos: 0   Perdidos: 0",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(25f, -105f), new Vector2(1220f, 40f),
            16, TextAnchor.MiddleLeft, font, new Color(0.65f, 1f, 0.72f));

        Text time = TextUI(canvasGo.transform, "Tiempo", "Tiempo: 03:00",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-25f, -18f), new Vector2(260f, 45f),
            24, TextAnchor.MiddleRight, font, Color.white);

        Text hint = TextUI(canvasGo.transform, "Ayuda",
            "LA MYPIME GAME · MINIMERCADO · PC: WASD + E + TAB ventas + B tienda + H contratar  |  MÓVIL: joystick + INTERACTUAR  |  PC del almacén: pedir mercancía",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(1200f, 35f),
            15, TextAnchor.MiddleCenter, font, new Color(1f, 1f, 1f, 0.75f));

        Text prompt = TextUI(canvasGo.transform, "AvisoInteraccion", "",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 165f), new Vector2(900f, 50f),
            26, TextAnchor.MiddleCenter, font, new Color(1f, 0.92f, 0.35f));

        Text message = TextUI(canvasGo.transform, "Mensaje", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(800f, 50f),
            22, TextAnchor.MiddleCenter, font, Color.white);

        // Fondo oscuro del aviso de quiebra
        GameObject quiebraFondo = new GameObject("QuiebraFondo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        quiebraFondo.transform.SetParent(canvasGo.transform, false);
        RectTransform qf = quiebraFondo.GetComponent<RectTransform>();
        qf.anchorMin = qf.anchorMax = new Vector2(0.5f, 0.5f);
        qf.pivot = new Vector2(0.5f, 0.5f);
        qf.anchoredPosition = Vector2.zero;
        qf.sizeDelta = new Vector2(1280f, 720f);
        quiebraFondo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);

        Text gameOver = TextUI(quiebraFondo.transform, "Quiebra",
            "¡QUIEBRA!\nNo pudiste pagar alquiler, luz y sueldos\nToca la pantalla para empezar de nuevo",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1150f, 320f),
            46, TextAnchor.MiddleCenter, font, new Color(1f, 0.42f, 0.38f));
        quiebraFondo.SetActive(false);

        // Panel de analítica de ventas
        GameObject anaFondo = new GameObject("AnaliticaPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        anaFondo.transform.SetParent(canvasGo.transform, false);
        RectTransform af = anaFondo.GetComponent<RectTransform>();
        af.anchorMin = af.anchorMax = new Vector2(0.5f, 0.5f);
        af.pivot = new Vector2(0.5f, 0.5f);
        af.anchoredPosition = Vector2.zero;
        af.sizeDelta = new Vector2(1280f, 720f);
        anaFondo.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.10f, 0.93f);

        Text anaText = TextUI(anaFondo.transform, "AnaliticaTexto", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1080f, 620f),
            22, TextAnchor.UpperLeft, font, new Color(0.80f, 0.92f, 1f));
        anaText.horizontalOverflow = HorizontalWrapMode.Wrap;
        anaText.verticalOverflow = VerticalWrapMode.Truncate;
        anaFondo.SetActive(false);

        GameObject anaBtn = new GameObject("BotonAnalitica", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        anaBtn.transform.SetParent(canvasGo.transform, false);
        RectTransform ab = anaBtn.GetComponent<RectTransform>();
        ab.anchorMin = ab.anchorMax = new Vector2(1f, 1f);
        ab.pivot = new Vector2(1f, 1f);
        ab.anchoredPosition = new Vector2(-25f, -70f);
        ab.sizeDelta = new Vector2(180f, 45f);
        anaBtn.GetComponent<Image>().color = new Color(0.18f, 0.42f, 0.62f, 0.78f);
        TextUI(anaBtn.transform, "TextoAnalitica", "VENTAS (TAB)",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180f, 40f),
            15, TextAnchor.MiddleCenter, font, Color.white);

        Text empText = TextUI(canvasGo.transform, "Empleados",
            "Empleados: 0/2   Sueldos: 0 CUP/día   Contratar: 13.950 CUP",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(25f, -145f), new Vector2(760f, 35f),
            16, TextAnchor.MiddleLeft, font, new Color(1f, 0.85f, 0.55f));

        GameObject hireGo = new GameObject("BotonContratar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        hireGo.transform.SetParent(canvasGo.transform, false);
        RectTransform hire = hireGo.GetComponent<RectTransform>();
        hire.anchorMin = hire.anchorMax = new Vector2(1f, 1f);
        hire.pivot = new Vector2(1f, 1f);
        hire.anchoredPosition = new Vector2(-25f, -122f);
        hire.sizeDelta = new Vector2(180f, 45f);
        hireGo.GetComponent<Image>().color = new Color(0.22f, 0.55f, 0.28f, 0.80f);
        TextUI(hireGo.transform, "TextoContratar", "CONTRATAR (H)",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180f, 40f),
            14, TextAnchor.MiddleCenter, font, Color.white);

        GameObject fireGo = new GameObject("BotonDespedir", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fireGo.transform.SetParent(canvasGo.transform, false);
        RectTransform fire = fireGo.GetComponent<RectTransform>();
        fire.anchorMin = fire.anchorMax = new Vector2(1f, 1f);
        fire.pivot = new Vector2(1f, 1f);
        fire.anchoredPosition = new Vector2(-25f, -174f);
        fire.sizeDelta = new Vector2(180f, 45f);
        fireGo.GetComponent<Image>().color = new Color(0.62f, 0.24f, 0.24f, 0.80f);
        TextUI(fireGo.transform, "TextoDespedir", "DESPEDIR (SHIFT+H)",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180f, 40f),
            13, TextAnchor.MiddleCenter, font, Color.white);

        // Botón de la tienda de muebles y ampliaciones
        GameObject tiendaGo = new GameObject("BotonTienda", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        tiendaGo.transform.SetParent(canvasGo.transform, false);
        RectTransform tienda = tiendaGo.GetComponent<RectTransform>();
        tienda.anchorMin = tienda.anchorMax = new Vector2(1f, 1f);
        tienda.pivot = new Vector2(1f, 1f);
        tienda.anchoredPosition = new Vector2(-25f, -226f);
        tienda.sizeDelta = new Vector2(180f, 45f);
        tiendaGo.GetComponent<Image>().color = new Color(0.62f, 0.46f, 0.16f, 0.82f);
        TextUI(tiendaGo.transform, "TextoBotonTienda", "TIENDA (B)",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180f, 40f),
            15, TextAnchor.MiddleCenter, font, Color.white);

        // Círculo base del joystick
        GameObject joyBase = new GameObject("JoystickBase", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        joyBase.transform.SetParent(canvasGo.transform, false);
        RectTransform jr = joyBase.GetComponent<RectTransform>();
        jr.anchorMin = jr.anchorMax = new Vector2(0f, 0f);
        jr.pivot = new Vector2(0f, 0f);
        jr.anchoredPosition = new Vector2(30f, 30f);
        jr.sizeDelta = new Vector2(180f, 180f);
        joyBase.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);

        GameObject knob = new GameObject("JoystickKnob", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        knob.transform.SetParent(joyBase.transform, false);
        RectTransform kr = knob.GetComponent<RectTransform>();
        kr.anchorMin = kr.anchorMax = new Vector2(0.5f, 0.5f);
        kr.pivot = new Vector2(0.5f, 0.5f);
        kr.anchoredPosition = Vector2.zero;
        kr.sizeDelta = new Vector2(70f, 70f);
        knob.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.45f);

        Joystick joystick = canvasGo.AddComponent<Joystick>();
        joystick.knob = kr;
        joystick.radio = 70f;
        player.joystick = joystick;

        // Botón de interactuar (abajo derecha)
        GameObject btn = new GameObject("BotonInteractuar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        btn.transform.SetParent(canvasGo.transform, false);
        RectTransform br = btn.GetComponent<RectTransform>();
        br.anchorMin = br.anchorMax = new Vector2(1f, 0f);
        br.pivot = new Vector2(1f, 0f);
        br.anchoredPosition = new Vector2(-30f, 30f);
        br.sizeDelta = new Vector2(150f, 150f);
        btn.GetComponent<Image>().color = new Color(0.20f, 0.62f, 0.32f, 0.45f);

        TextUI(btn.transform, "TextoBoton", "INTERACTUAR\n(E)",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150f, 70f),
            17, TextAnchor.MiddleCenter, font, Color.white);

        canvasGo.AddComponent<OrderComputerUI>();
        canvasGo.AddComponent<StoreUI>();
        HUD hud = canvasGo.AddComponent<HUD>();
        hud.moneyText = money;
        hud.goalText = goal;
        hud.timeText = time;
        hud.carriedText = carried;
        hud.promptText = prompt;
        hud.messageText = message;
        hud.gameOverPanel = quiebraFondo;
        hud.analyticsPanel = anaFondo;
        hud.analyticsText = anaText;
        hud.analyticsButton = ab;
        hud.empleadosText = empText;
        hud.hireButton = hire;
        hud.fireButton = fire;
        hud.tiendaButton = tienda;
    }

    static Text TextUI(Transform parent, string name, string content,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size,
        int fontSize, TextAnchor align, Font font, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Text text = go.GetComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = align;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.75f);
        outline.effectDistance = new Vector2(2f, -2f);
        return text;
    }
}
#endif
