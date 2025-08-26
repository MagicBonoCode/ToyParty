using System.Collections;
using UnityEngine;

public abstract class Block : MonoBehaviour
{
    public enum BlockType { Gem, Boom, Rocket, Rainbow, Spinning, None, }
    public enum BlockColor { None, Red, Green, Blue, Yellow, Purple, Orange, }

    /// <summary>���� �̵� ���� ��� ��</summary>
    public static int MovingCount = 0;

    /// <summary>�̵� ������</summary>
    private const float MOVE_DELAY = 0.2f;

    [SerializeField]
    public SpriteRenderer _spriteRenderer;

    public HexPos Pos { get; protected set; }
    public BlockType Type { get; protected set; }
    public BlockColor Color { get; protected set; }

    /// <summary>��� ���� ���� ���� *����� ���� ���� ����</summary>
    public virtual bool IsSwappable { get { return true; } }
    /// <summary>��� �߷� ���� ���� *����� ���� ���� ����</summary>
    public virtual bool IsBlocksFalling { get { return true; } }
    /// <summary>��� ��Ī ���� ����</summary>
    public virtual bool IsMatchable { get; protected set; }

    /// <summary>���κ��� �� Ȯ�� *���κ��� �� ������ ���� �ϴ���..</summary>
    public virtual bool IsRainbow { get; protected set; }

    /// <summary>��� ��Ī Ȱ��ȭ ������ (����Ʈ �����)</summary>
    protected float _activeDelay = 0.2f;
    /// <summary>���� �̵� ������ ����</summary>
    private bool _isMoving = false;

    private Coroutine _coSmoothToMove;
    private Coroutine _coActiveBlock;
    private Coroutine _coScaleEffect;

    private void OnDisable()
    {
        // ���� �̵� ���̿��ٸ� ī��Ʈ�� ����
        if(_isMoving)
        {
            MovingCount--;
            _isMoving = false;
        }

        StopAllCoroutines();
    }

    protected abstract Sprite GetSprite();

    /// <summary>
    /// ����� �ε巴�� �̵���Ű�� �Լ��Դϴ�.
    /// </summary>
    /// <param name="pos">�̵� ��ǥ</param>
    public virtual void SmoothToMove(HexPos pos)
    {
        Pos = pos;
        Vector2 target = Managers.Match.AxialToWorld(pos);

        if(_coSmoothToMove != null)
        {
            StopCoroutine(_coSmoothToMove);
            _coSmoothToMove = null;
        }

        _coSmoothToMove = StartCoroutine(CoSmoothToMove(target));
    }

    private IEnumerator CoSmoothToMove(Vector2 target)
    {
        // ���� �̵� ���̿��ٸ� ī��Ʈ�� ���� �ٽ� ����
        if(_isMoving)
        {
            MovingCount--;
            _isMoving = false;
        }

        MovingCount++;
        _isMoving = true;

        Vector2 start = transform.localPosition;

        float timer = 0.0f;
        while(timer < 1.0f)
        {
            timer += Time.deltaTime / MOVE_DELAY;
            transform.localPosition = Vector2.Lerp(start, target, Mathf.Clamp01(timer));
            yield return null;
        }

        transform.localPosition = target;

        MovingCount--;
        _isMoving = false;
    }

    /// <summary>
    /// ��� Ȱ��ȭ(��Ī, ����) �Լ��Դϴ�.
    /// </summary>
    public void ActivateBlock()
    {
        if(_coActiveBlock != null)
        {
            StopCoroutine(_coActiveBlock);
            _coActiveBlock = null;
        }

        _coActiveBlock = StartCoroutine(CoActivateBlock());
    }

    private IEnumerator CoActivateBlock()
    {
        ActiveBlock();

        yield return new WaitForSeconds(_activeDelay);

        CompleteBlock();
    }

    /// <summary>
    /// ��� Ȱ��ȭ �Լ��Դϴ�.
    /// </summary>
    protected virtual void ActiveBlock()
    {
        // TOOD: Some..

        if(_coScaleEffect != null)
        {
            StopCoroutine(_coScaleEffect);
            _coScaleEffect = null;
        }

        _coScaleEffect = StartCoroutine(CoScaleEffect());
    }

    /// <summary>
    /// ��� Ȱ��ȭ �Ϸ� �Լ��Դϴ�.
    /// </summary>
    protected virtual void CompleteBlock()
    {
        // TODO: Some..
    }

    protected IEnumerator CoScaleEffect()
    {
        float scaleUp = 1.2f;
        float durationUp = 0.08f;
        float durationDown = 0.10f;

        Vector3 originalScale = transform.localScale;
        float timer = 0.0f;
        while(timer < 1.0f)
        {
            timer += Time.deltaTime / durationUp;
            float s = Mathf.Lerp(1.0f, scaleUp, timer);
            transform.localScale = originalScale * s;
            yield return null;
        }

        timer = 0.0f;
        while(timer < 1.0f)
        {
            timer += Time.deltaTime / durationDown;
            float s = Mathf.Lerp(scaleUp, 1.0f, timer);
            transform.localScale = originalScale * s;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
