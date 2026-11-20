using UnityEngine;

/// <summary>
/// Thrown state for a world item. Prefers reusing the original InteractItem
/// GameObject (same sprite/scale) and makes it pickable again once at rest.
/// </summary>
public class ThrownItem : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Collider2D _col;
    private Collider2D _playerCol;
    private bool _thrown;

    private static Sprite _placeholder;

    /// <summary>Throw an existing world item — reuses its GameObject.</summary>
    public static ThrownItem Throw(InteractItem worldItem, Vector2 position, Vector2 dir, float speed, GameObject thrower)
    {
        var go = worldItem.gameObject;
        go.transform.position = position;
        if (!go.activeSelf) go.SetActive(true);

        var ti = go.GetComponent<ThrownItem>();
        if (ti == null) ti = go.AddComponent<ThrownItem>();

        ti._col = go.GetComponent<Collider2D>();
        if (ti._col == null)
        {
            var c = go.AddComponent<CircleCollider2D>();
            c.isTrigger = true;
            ti._col = c;
        }

        ti._rb = go.GetComponent<Rigidbody2D>();
        if (ti._rb == null) ti._rb = go.AddComponent<Rigidbody2D>();
        ti._rb.bodyType = RigidbodyType2D.Dynamic;
        ti._rb.gravityScale = 0f;
        ti._rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        ti._rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        ti._rb.drag = 0.7f; // decays → comes to rest on its own
        ti._rb.velocity = dir.normalized * speed;

        // never collide with the thrower in flight
        ti._playerCol = thrower != null ? thrower.GetComponentInChildren<Collider2D>() : null;
        if (ti._playerCol != null)
            Physics2D.IgnoreCollision(ti._col, ti._playerCol, true);

        ti._thrown = true;
        return ti;
    }

    /// <summary>Fallback: temp object when no world item exists for this data.</summary>
    public static ThrownItem Spawn(ItemData item, Vector2 position, Vector2 dir, float speed)
    {
        var go = new GameObject($"Thrown_{item.ItemName}");
        go.transform.position = position;
        var ti = go.AddComponent<ThrownItem>();

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = item.Icon != null ? item.Icon : Placeholder();
        sr.sortingOrder = 100;

        ti._col = go.AddComponent<CircleCollider2D>();
        ti._col.isTrigger = true;

        ti._rb = go.AddComponent<Rigidbody2D>();
        ti._rb.gravityScale = 0f;
        ti._rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        ti._rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        ti._rb.drag = 0.7f;
        ti._rb.velocity = dir.normalized * speed;
        ti._thrown = true;

        return ti;
    }

    private void Update()
    {
        // drag exhausted → at rest → stop and become pickable again
        if (!_thrown || _rb == null || _rb.bodyType != RigidbodyType2D.Dynamic) return;
        if (_rb.velocity.sqrMagnitude < 0.01f) Stop();
    }

    private void OnTriggerEnter2D(Collider2D other) => HandleContact(other);

    private void OnCollisionEnter2D(Collision2D other) => HandleContact(other.collider);

    private void HandleContact(Collider2D other)
    {
        if (!_thrown) return;
        if (other.isTrigger) return; // trigger zones don't stop it
        if (other.GetComponentInParent<PlayerThrow>() != null) return; // ignore thrower
        Stop();
    }

    private void Stop()
    {
        if (_rb != null)
        {
            _rb.velocity = Vector2.zero;
            _rb.bodyType = RigidbodyType2D.Static;
        }
        _thrown = false;

        var worldItem = GetComponent<InteractItem>();
        if (worldItem != null) worldItem.OnLanded();

        if (_col != null && _playerCol != null)
            Physics2D.IgnoreCollision(_col, _playerCol, false);
    }

    private static Sprite Placeholder()
    {
        if (_placeholder != null) return _placeholder;

        var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        var pixels = new Color[64];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();

        // pixelsPerUnit chosen so placeholder world size ≈ 0.4 units.
        _placeholder = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f / 0.4f);
        return _placeholder;
    }
}
