using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grid de navegación con A* casero. Marca como bloqueadas las celdas que
/// solapan estanterías, pedidos, caja y paredes (menos los dinteles de puerta).
/// No necesita NavMesh ni paquetes adicionales de Unity.
/// </summary>
public class NavigationGrid : MonoBehaviour
{
    public static NavigationGrid Instance { get; private set; }

    [Header("Zona caminable (X, Z)")]
    public Vector2 min = new Vector2(-9f, -7f);
    public Vector2 max = new Vector2(9f, 11f);
    public float cellSize = 0.5f;

    [Tooltip("Margen extra alrededor de los obstáculos para no rozarlos")]
    public float inflation = 0.35f;

    [Tooltip("Mostrar las celdas bloqueadas en la vista Scene")]
    public bool mostrarGrid = false;

    [SerializeField] private bool[] walkable;
    private int width;
    private int height;

    void Awake()
    {
        Instance = this;
        if (walkable == null || walkable.Length == 0) Build();
    }

    [ContextMenu("Recalcular grid")]
    public void Build()
    {
        width = Mathf.Max(1, Mathf.CeilToInt((max.x - min.x) / cellSize));
        height = Mathf.Max(1, Mathf.CeilToInt((max.y - min.y) / cellSize));
        walkable = new bool[width * height];

        for (int i = 0; i < walkable.Length; i++) walkable[i] = true;

        // Bloqueamos muebles, pedidos, caja y paredes (el dintel no: está por encima de la puerta).
        foreach (Collider col in Object.FindObjectsByType<Collider>())
        {
            if (col == null) continue;
            string nombre = col.gameObject.name;
            bool esPared = nombre.StartsWith("Pared") && !nombre.Contains("dintel");
            bool esPersiana = nombre.StartsWith("Persiana");
            if (col.GetComponentInParent<Shelf>() == null &&
                col.GetComponentInParent<DeliveryBox>() == null &&
                col.GetComponentInParent<Checkout>() == null &&
                col.GetComponentInParent<StoreTablet>() == null &&
                !esPared && !esPersiana) continue;

            Bounds b = col.bounds;
            b.Expand(new Vector3(inflation * 2f, 0f, inflation * 2f));

            int x0 = CellX(b.min.x), x1 = CellX(b.max.x);
            int y0 = CellY(b.min.z), y1 = CellY(b.max.z);
            for (int x = x0; x <= x1; x++)
                for (int y = y0; y <= y1; y++)
                    walkable[Index(x, y)] = false;
        }
    }

    /// <summary>Calcula una ruta desde 'from' hasta 'to'. Devuelve null si no hay camino.</summary>
    public List<Vector3> FindPath(Vector3 from, Vector3 to)
    {
        if (walkable == null || walkable.Length == 0) Build();

        int start = IndexOfWorld(from);
        int goal = IndexOfWorld(to);
        if (!walkable[start] || !walkable[goal])
        {
            start = NearestWalkable(start);
            goal = NearestWalkable(goal);
            if (start < 0 || goal < 0) return null;
        }

        float[] g = new float[walkable.Length];
        int[] cameFrom = new int[walkable.Length];
        bool[] closed = new bool[walkable.Length];
        List<int> open = new List<int>();

        for (int i = 0; i < g.Length; i++) { g[i] = float.PositiveInfinity; cameFrom[i] = -1; }
        g[start] = 0f;
        open.Add(start);

        int[] dx = { 1, -1, 0, 0, 1, 1, -1, -1 };
        int[] dy = { 0, 0, 1, -1, 1, -1, 1, -1 };

        while (open.Count > 0)
        {
            int current = open[0];
            float bestF = g[current] + Heuristic(current, goal);
            for (int i = 1; i < open.Count; i++)
            {
                float f = g[open[i]] + Heuristic(open[i], goal);
                if (f < bestF) { current = open[i]; bestF = f; }
            }

            open.Remove(current);
            if (current == goal) return BuildPath(start, goal, cameFrom);

            closed[current] = true;
            int cx = current % width;
            int cy = current / width;

            for (int n = 0; n < 8; n++)
            {
                int nx = cx + dx[n], ny = cy + dy[n];
                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;

                int next = Index(nx, ny);
                if (!walkable[next] || closed[next]) continue;

                // En diagonal solo si los dos lados están libres (no cortar esquinas).
                if (n >= 4)
                {
                    if (!walkable[Index(cx + dx[n], cy)] || !walkable[Index(cx, cy + dy[n])]) continue;
                }

                float cost = g[current] + (n >= 4 ? 1.4142f : 1f);
                if (cost >= g[next]) continue;

                cameFrom[next] = current;
                g[next] = cost;
                if (!open.Contains(next)) open.Add(next);
            }
        }
        return null;
    }

    List<Vector3> BuildPath(int start, int goal, int[] cameFrom)
    {
        List<Vector2> celdas = new List<Vector2>();
        int actual = goal;
        while (actual != start && actual >= 0)
        {
            celdas.Add(new Vector2(actual % width, actual / width));
            actual = cameFrom[actual];
        }
        celdas.Reverse();

        List<Vector2> suavizadas = Smooth(celdas);
        List<Vector3> resultado = new List<Vector3>();
        foreach (Vector2 c in suavizadas)
            resultado.Add(ToWorld(Mathf.RoundToInt(c.x), Mathf.RoundToInt(c.y)));
        return resultado;
    }

    /// <summary>Salta puntos intermedios cuando hay línea recta despejada.</summary>
    List<Vector2> Smooth(List<Vector2> points)
    {
        if (points.Count < 2) return points;

        List<Vector2> result = new List<Vector2>();
        int i = 0;
        while (i < points.Count)
        {
            result.Add(points[i]);
            int farthest = i;
            for (int j = points.Count - 1; j > i; j--)
            {
                if (LineOfSight(points[i], points[j])) { farthest = j; break; }
            }
            i = farthest == i ? i + 1 : farthest;
        }
        return result;
    }

    bool LineOfSight(Vector2 a, Vector2 b)
    {
        float dist = Vector2.Distance(a, b);
        int steps = Mathf.Max(1, Mathf.CeilToInt(dist / (cellSize * 0.5f)));
        for (int i = 0; i <= steps; i++)
        {
            Vector2 p = Vector2.Lerp(a, b, i / (float)steps);
            int ix = Mathf.Clamp(Mathf.FloorToInt((p.x - min.x) / cellSize), 0, width - 1);
            int iy = Mathf.Clamp(Mathf.FloorToInt((p.y - min.y) / cellSize), 0, height - 1);
            if (!walkable[Index(ix, iy)]) return false;
        }
        return true;
    }

    float Heuristic(int a, int b)
    {
        int ax = a % width, ay = a / width;
        int bx = b % width, by = b / width;
        int dx = Mathf.Abs(ax - bx), dy = Mathf.Abs(ay - by);
        return Mathf.Max(dx, dy) + (1.4142f - 1f) * Mathf.Min(dx, dy);
    }

    int NearestWalkable(int origin)
    {
        if (origin < 0 || origin >= walkable.Length) return -1;
        if (walkable[origin]) return origin;

        int ox = origin % width, oy = origin / width;
        for (int r = 1; r < 12; r++)
        {
            for (int x = ox - r; x <= ox + r; x++)
                for (int y = oy - r; y <= oy + r; y++)
                {
                    if (x < 0 || y < 0 || x >= width || y >= height) continue;
                    if (walkable[Index(x, y)]) return Index(x, y);
                }
        }
        return -1;
    }

    int CellX(float worldX) => Mathf.Clamp(Mathf.FloorToInt((worldX - min.x) / cellSize), 0, width - 1);
    int CellY(float worldZ) => Mathf.Clamp(Mathf.FloorToInt((worldZ - min.y) / cellSize), 0, height - 1);
    int Index(int x, int y) => y * width + x;
    int IndexOfWorld(Vector3 p) => Index(CellX(p.x), CellY(p.z));
    Vector3 ToWorld(int x, int y) => new Vector3(min.x + (x + 0.5f) * cellSize, 0f, min.y + (y + 0.5f) * cellSize);

    void OnDrawGizmos()
    {
        if (!mostrarGrid || walkable == null) return;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (walkable[Index(x, y)]) continue;
                Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
                Vector3 c = ToWorld(x, y);
                Gizmos.DrawCube(c + Vector3.up * 0.05f, new Vector3(cellSize, 0.04f, cellSize));
            }
        }
    }
}
