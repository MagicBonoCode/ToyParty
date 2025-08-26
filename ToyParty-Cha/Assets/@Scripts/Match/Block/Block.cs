using System.Collections;
using UnityEngine;

public abstract class Block : MonoBehaviour
{
    public enum BlockType { Gem, Boom, Rocket, Rainbow, Spinning, None, }
    public enum BlockColor { None, Red, Green, Blue, Yellow, Purple, Orange, }

    /// <summary>현재 이동 중인 블록 수</summary>
    public static int MovingCount = 0;

    /// <summary>이동 딜레이</summary>
    private const float MOVE_DELAY = 0.2f;

    [SerializeField]
    public SpriteRenderer _spriteRenderer;

    public HexPos Pos { get; protected set; }
    public BlockType Type { get; protected set; }
    public BlockColor Color { get; protected set; }

    /// <summary>블록 스왑 가능 여부 *현재는 전부 스왑 가능</summary>
    public virtual bool IsSwappable { get { return true; } }
    /// <summary>블록 중력 적용 여부 *현재는 전부 적용 가능</summary>
    public virtual bool IsBlocksFalling { get { return true; } }
    /// <summary>블록 매칭 가능 여부</summary>
    public virtual bool IsMatchable { get; protected set; }

    /// <summary>레인보우 블럭 확인 *레인보우 블럭 구분을 위해 일단은..</summary>
    public virtual bool IsRainbow { get; protected set; }

    /// <summary>블록 매칭 활성화 딜레이 (이펙트 재생용)</summary>
    protected float _activeDelay = 0.2f;
    /// <summary>현재 이동 중인지 여부</summary>
    private bool _isMoving = false;

    private Coroutine _coSmoothToMove;
    private Coroutine _coActiveBlock;
    private Coroutine _coScaleEffect;

    private void OnDisable()
    {
        // 만약 이동 중이였다면 카운트를 빼줌
        if(_isMoving)
        {
            MovingCount--;
            _isMoving = false;
        }

        StopAllCoroutines();
    }

    protected abstract Sprite GetSprite();

    /// <summary>
    /// 블록을 부드럽게 이동시키는 함수입니다.
    /// </summary>
    /// <param name="pos">이동 좌표</param>
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
        // 만약 이동 중이였다면 카운트를 빼고 다시 시작
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
    /// 블록 활성화(매칭, 삭제) 함수입니다.
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
    /// 블록 활성화 함수입니다.
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
    /// 블록 활성화 완료 함수입니다.
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
