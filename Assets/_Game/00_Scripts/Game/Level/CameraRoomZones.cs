using Cinemachine;
using UnityEngine;

/// <summary>
/// Keeps the Cinemachine camera inside the room the player is in: the camera follows the
/// player but stops at that room's walls/corners. Rooms are cell rectangles on the level
/// Grid (walls included). When the player stands in more than one zone the smallest wins;
/// in a gap between zones (a corridor) the current room is kept.
/// </summary>
public class CameraRoomZones : MonoBehaviour
{
    [SerializeField] private CinemachineConfiner2D _confiner;
    [SerializeField] private Grid _levelGrid;
    [SerializeField] private Transform _target;
    [Tooltip("Room rectangles in level cell coordinates, walls included.")]
    [SerializeField] private RectInt[] _rooms;

    private PolygonCollider2D[] _shapes;
    private int _current = -1;

    public int CurrentRoom => _current;

    private void Awake() => BuildShapes();

    private void BuildShapes()
    {
        if (_levelGrid == null || _rooms == null) return;

        // World-space holder (not under the moving camera).
        var holder = new GameObject("CameraRoomZones (runtime)");
        holder.layer = LayerMask.NameToLayer("Ignore Raycast");

        _shapes = new PolygonCollider2D[_rooms.Length];
        for (int i = 0; i < _rooms.Length; i++)
        {
            var r = _rooms[i];
            var go = new GameObject($"Room_{i}");
            go.layer = holder.layer;
            go.transform.SetParent(holder.transform, false);

            var poly = go.AddComponent<PolygonCollider2D>();
            poly.isTrigger = true;
            poly.points = new Vector2[]
            {
                _levelGrid.CellToWorld(new Vector3Int(r.xMin, r.yMin, 0)),
                _levelGrid.CellToWorld(new Vector3Int(r.xMax, r.yMin, 0)),
                _levelGrid.CellToWorld(new Vector3Int(r.xMax, r.yMax, 0)),
                _levelGrid.CellToWorld(new Vector3Int(r.xMin, r.yMax, 0)),
            };
            _shapes[i] = poly;
        }
    }

    private void LateUpdate()
    {
        if (_confiner == null || _shapes == null) return;
        if (_target == null && PlayerInteract.Instance != null) _target = PlayerInteract.Instance.transform;
        if (_target == null) return;

        int room = FindRoom(_target.position);
        if (room < 0 || room == _current) return;

        _current = room;
        _confiner.m_BoundingShape2D = _shapes[room];
        _confiner.InvalidateCache();
    }

    private int FindRoom(Vector2 world)
    {
        if (_levelGrid == null) return -1;
        Vector3Int cell = _levelGrid.WorldToCell(world);
        int best = -1;
        int bestArea = int.MaxValue;
        for (int i = 0; i < _rooms.Length; i++)
        {
            var r = _rooms[i];
            if (cell.x < r.xMin || cell.x >= r.xMax || cell.y < r.yMin || cell.y >= r.yMax) continue;
            int area = r.width * r.height;
            if (area < bestArea) { bestArea = area; best = i; }
        }
        return best;
    }

    private void OnDrawGizmosSelected()
    {
        if (_levelGrid == null || _rooms == null) return;
        for (int i = 0; i < _rooms.Length; i++)
        {
            var r = _rooms[i];
            Vector3 a = _levelGrid.CellToWorld(new Vector3Int(r.xMin, r.yMin, 0));
            Vector3 b = _levelGrid.CellToWorld(new Vector3Int(r.xMax, r.yMax, 0));
            Gizmos.color = i == _current ? Color.yellow : new Color(0.3f, 0.8f, 1f, 0.6f);
            Gizmos.DrawWireCube((a + b) * 0.5f, b - a);
        }
    }
}
