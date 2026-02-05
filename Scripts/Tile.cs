using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    [Header("Grid Info")]
    public int x;
    public int y;
    public int colorIndex;

    [Header("Move / Fall Settings")]
    [SerializeField] private float fallSpeed = 900f;  // 🔥 VERY FAST DROP
    [SerializeField] private float snapDist = 2f;     // 🔥 snaps quicker

    [Header("Pop FX (FAST BAM BAM)")]
    [SerializeField] private float popUpTime = 0.04f;     // fast grow
    [SerializeField] private float popDownTime = 0.06f;   // fast shrink
    [SerializeField] private float popScale = 1.12f;      // little punch

    private BoardManager board;

    private RectTransform _rectTransform;
    private Image _image;

    private Vector2 _targetUIPos;
    private bool _hasTarget;
    private bool _isDead;

    // Icon set (Default / A / B / C)
    private Sprite _defaultIcon;
    private Sprite _iconA;
    private Sprite _iconB;
    private Sprite _iconC;

    private Coroutine _popRoutine;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();

        if (_image == null)
            _image = gameObject.AddComponent<Image>();
    }

    // OLD INIT (backward compatible)
    public void Init(int gx, int gy, int cIndex, Sprite sprite, BoardManager bm)
    {
        x = gx;
        y = gy;
        colorIndex = cIndex;
        board = bm;

        _defaultIcon = sprite;
        _iconA = sprite;
        _iconB = sprite;
        _iconC = sprite;

        SetSprite(sprite);

        _hasTarget = false;
        _isDead = false;

        if (_image != null) _image.raycastTarget = true;
        transform.localScale = Vector3.one;
    }

    // NEW INIT (Default + A + B + C)
    public void Init(int gx, int gy, int cIndex,
        Sprite defaultIcon, Sprite iconA, Sprite iconB, Sprite iconC,
        BoardManager bm)
    {
        x = gx;
        y = gy;
        colorIndex = cIndex;
        board = bm;

        _defaultIcon = defaultIcon;
        _iconA = iconA;
        _iconB = iconB;
        _iconC = iconC;

        SetSprite(_defaultIcon);

        _hasTarget = false;
        _isDead = false;

        if (_image != null) _image.raycastTarget = true;
        transform.localScale = Vector3.one;
    }

    public void SetIconSet(Sprite defaultIcon, Sprite iconA, Sprite iconB, Sprite iconC)
    {
        _defaultIcon = defaultIcon;
        _iconA = iconA;
        _iconB = iconB;
        _iconC = iconC;
    }

    // BoardAnalyzer call this one sheett
    public void SetSprite(Sprite sprite)
    {
        if (_image != null)
            _image.sprite = sprite;
    }

    // 0=Default, 1=A, 2=B, 3=C
    public void SetTier(int tier)
    {
        if (_image == null) return;

        tier = Mathf.Clamp(tier, 0, 3);

        Sprite s = tier switch
        {
            1 => _iconA != null ? _iconA : _defaultIcon,
            2 => _iconB != null ? _iconB : (_iconA != null ? _iconA : _defaultIcon),
            3 => _iconC != null ? _iconC : (_iconB != null ? _iconB : (_iconA != null ? _iconA : _defaultIcon)),
            _ => _defaultIcon
        };

        if (s != null)
            _image.sprite = s;
    }

    public void SetGridPosition(int gx, int gy)
    {
        x = gx;
        y = gy;
    }

    public void SetAnchoredPositionImmediate(Vector2 uiPos)
    {
        if (_rectTransform == null) return;

        _rectTransform.anchoredPosition = uiPos;
        _targetUIPos = uiPos;
        _hasTarget = false;
    }

    public void SetTargetAnchoredPosition(Vector2 uiPos)
    {
        _targetUIPos = uiPos;
        _hasTarget = true;
    }

    public bool IsSettled()
    {
        if (!_hasTarget) return true;
        if (_rectTransform == null) return true;

        return Vector2.Distance(_rectTransform.anchoredPosition, _targetUIPos) <= snapDist;
    }

    private void Update()
    {
        if (_isDead) return;
        if (!_hasTarget) return;
        if (_rectTransform == null) return;

        Vector2 current = _rectTransform.anchoredPosition;

        // 🔥 FAST DROP
        Vector2 next = Vector2.MoveTowards(current, _targetUIPos, fallSpeed * Time.deltaTime);
        _rectTransform.anchoredPosition = next;

        if (Vector2.Distance(next, _targetUIPos) <= snapDist)
        {
            _rectTransform.anchoredPosition = _targetUIPos;
            _hasTarget = false;
        }
    }

    // ✅ FAST POP + DESTROY (bam bam)
    public void PlayPopAndDestroy(float delaySeconds)
    {
        if (_isDead) return;
        _isDead = true;

        if (_image != null) _image.raycastTarget = false;
        _hasTarget = false; // stop movement instantly

        if (_popRoutine != null) StopCoroutine(_popRoutine);
        _popRoutine = StartCoroutine(PopAndDestroyRoutine());
    }

    private IEnumerator PopAndDestroyRoutine()
    {
        Vector3 start = Vector3.one;
        Vector3 up = Vector3.one * popScale;

        // UP (FAST)
        float t = 0f;
        while (t < popUpTime)
        {
            t += Time.deltaTime;
            float a = popUpTime <= 0f ? 1f : (t / popUpTime);
            transform.localScale = Vector3.LerpUnclamped(start, up, a);
            yield return null;
        }
        transform.localScale = up;

        // DOWN (FAST)
        t = 0f;
        while (t < popDownTime)
        {
            t += Time.deltaTime;
            float a = popDownTime <= 0f ? 1f : (t / popDownTime);
            transform.localScale = Vector3.LerpUnclamped(up, Vector3.zero, a);
            yield return null;
        }

        Destroy(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isDead) return;
        if (board == null) return;

        board.HandleTileClick(this);
    }
}
