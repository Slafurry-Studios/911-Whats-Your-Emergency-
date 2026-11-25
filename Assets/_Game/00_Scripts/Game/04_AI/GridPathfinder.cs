using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// A* over the level's floor tilemap. A cell is walkable when it has a floor tile, no wall
/// tile and no solid static prop (table, box, hide spot, outlet). Doors stay walkable: the
/// kidnapper forces closed doors open. Built once on Start; call Rebuild() if the level changes.
/// </summary>
public class GridPathfinder : MonoBehaviour
{
    [SerializeField] private Tilemap _floor;
    [SerializeField] private Tilemap _walls;
    [Tooltip("Layers whose solid colliders block a cell (props). Doors, items and characters are ignored.")]
    [SerializeField] private LayerMask _obstacleMask = ~0;
    [Tooltip("Fraction of a cell a prop must overlap to block it.")]
    [SerializeField, Range(0.1f, 1f)] private float _probeSize = 0.6f;

    public static GridPathfinder Instance { get; private set; }

    private BoundsInt _bounds;
    private bool[,] _walkable;
    private readonly List<Vector3Int> _walkableCells = new();

    // A* scratch buffers
    private float[,] _g;
    private Vector3Int[,] _parent;
    private int[,] _visit;
    private int _visitStamp;

    public bool IsReady => _walkable != null;
    public float CellWorldSize => _floor != null ? _floor.layoutGrid.cellSize.x * _floor.transform.lossyScale.x : 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start() => Rebuild();

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Rebuild()
    {
        if (_floor == null)
        {
            Debug.LogError("[GridPathfinder] Floor tilemap not assigned.", this);
            return;
        }

        Physics2D.SyncTransforms();
        _floor.CompressBounds();
        _bounds = _floor.cellBounds;
        _walkable = new bool[_bounds.size.x, _bounds.size.y];
        _g = new float[_bounds.size.x, _bounds.size.y];
        _parent = new Vector3Int[_bounds.size.x, _bounds.size.y];
        _visit = new int[_bounds.size.x, _bounds.size.y];
        _walkableCells.Clear();

        Vector2 probe = Vector2.one * CellWorldSize * _probeSize;
        foreach (var cell in _bounds.allPositionsWithin)
        {
            bool ok = _floor.HasTile(cell) && (_walls == null || !_walls.HasTile(cell));
            if (ok && BlockedByProp(_floor.GetCellCenterWorld(cell), probe)) ok = false;
            _walkable[cell.x - _bounds.xMin, cell.y - _bounds.yMin] = ok;
            if (ok) _walkableCells.Add(cell);
        }
    }

    private bool BlockedByProp(Vector2 center, Vector2 size)
    {
        foreach (var col in Physics2D.OverlapBoxAll(center, size, 0f, _obstacleMask))
        {
            if (col.isTrigger) continue;
            if (col is TilemapCollider2D || col is CompositeCollider2D) continue; // walls: handled by tiles
            if (col.GetComponentInParent<Door>() != null) continue;
            if (col.GetComponentInParent<InteractItem>() != null) continue;
            if (col.GetComponentInParent<PlayerMovement>() != null) continue;
            if (col.GetComponentInParent<KidnapperActions>() != null) continue;
            return true;
        }
        return false;
    }

    // ------------------------------------------------------------------ queries
    public bool IsWalkable(Vector2 world)
    {
        if (!IsReady) return false;
        return Walkable(_floor.WorldToCell(world));
    }

    private bool Walkable(Vector3Int c)
    {
        int x = c.x - _bounds.xMin, y = c.y - _bounds.yMin;
        return x >= 0 && y >= 0 && x < _bounds.size.x && y < _bounds.size.y && _walkable[x, y];
    }

    /// <summary>A random walkable point within radius of center (or anywhere if radius &lt;= 0).</summary>
    public bool TryRandomPoint(Vector2 center, float radius, out Vector2 point)
    {
        point = center;
        if (!IsReady || _walkableCells.Count == 0) return false;
        for (int i = 0; i < 30; i++)
        {
            var cell = _walkableCells[Random.Range(0, _walkableCells.Count)];
            Vector2 p = _floor.GetCellCenterWorld(cell);
            if (radius > 0f && Vector2.Distance(p, center) > radius) continue;
            point = p;
            return true;
        }
        return false;
    }

    /// <summary>A* path (cell centers, start excluded, exact goal last). False when unreachable.</summary>
    public bool FindPath(Vector2 from, Vector2 to, List<Vector2> result)
    {
        result.Clear();
        if (!IsReady) return false;

        Vector3Int start = _floor.WorldToCell(from);
        Vector3Int goal = _floor.WorldToCell(to);
        if (!Walkable(start) && !TryNearestWalkable(start, out start)) return false;
        bool goalWalkable = Walkable(goal);
        if (!goalWalkable && !TryNearestWalkable(goal, out goal)) return false;

        _visitStamp++;
        var open = new List<(Vector3Int cell, float f)>();
        Set(start, 0f, start);
        open.Add((start, Heuristic(start, goal)));

        bool found = false;
        int guard = _bounds.size.x * _bounds.size.y * 4;
        while (open.Count > 0 && guard-- > 0)
        {
            int best = 0;
            for (int i = 1; i < open.Count; i++) if (open[i].f < open[best].f) best = i;
            var current = open[best].cell;
            open.RemoveAt(best);
            if (current == goal) { found = true; break; }

            float gCur = G(current);
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var next = new Vector3Int(current.x + dx, current.y + dy, 0);
                if (!Walkable(next)) continue;
                // no corner cutting
                if (dx != 0 && dy != 0 &&
                    (!Walkable(new Vector3Int(current.x + dx, current.y, 0)) ||
                     !Walkable(new Vector3Int(current.x, current.y + dy, 0)))) continue;

                float cost = gCur + (dx != 0 && dy != 0 ? 1.4142f : 1f);
                if (Visited(next) && cost >= G(next)) continue;
                Set(next, cost, current);
                open.Add((next, cost + Heuristic(next, goal)));
            }
        }

        if (!found) return false;

        var cells = new List<Vector3Int>();
        for (var c = goal; c != start; c = Parent(c)) cells.Add(c);
        cells.Reverse();
        foreach (var c in cells) result.Add(_floor.GetCellCenterWorld(c));
        if (goalWalkable && result.Count > 0) result[result.Count - 1] = to;
        return true;
    }

    private bool TryNearestWalkable(Vector3Int around, out Vector3Int cell)
    {
        for (int r = 1; r <= 4; r++)
        for (int dx = -r; dx <= r; dx++)
        for (int dy = -r; dy <= r; dy++)
        {
            var c = new Vector3Int(around.x + dx, around.y + dy, 0);
            if (Walkable(c)) { cell = c; return true; }
        }
        cell = around;
        return false;
    }

    private static float Heuristic(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x), dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) + (1.4142f - 2f) * Mathf.Min(dx, dy);
    }

    private bool Visited(Vector3Int c) => _visit[c.x - _bounds.xMin, c.y - _bounds.yMin] == _visitStamp;
    private float G(Vector3Int c) => _g[c.x - _bounds.xMin, c.y - _bounds.yMin];
    private Vector3Int Parent(Vector3Int c) => _parent[c.x - _bounds.xMin, c.y - _bounds.yMin];

    private void Set(Vector3Int c, float g, Vector3Int parent)
    {
        int x = c.x - _bounds.xMin, y = c.y - _bounds.yMin;
        _visit[x, y] = _visitStamp;
        _g[x, y] = g;
        _parent[x, y] = parent;
    }

    private void OnDrawGizmosSelected()
    {
        if (!IsReady) return;
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.25f);
        float s = CellWorldSize * 0.9f;
        foreach (var c in _walkableCells) Gizmos.DrawCube(_floor.GetCellCenterWorld(c), new Vector3(s, s, 0.01f));
    }
}
