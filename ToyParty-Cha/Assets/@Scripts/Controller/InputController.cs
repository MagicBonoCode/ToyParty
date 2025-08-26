using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour
{
    private const float DRAG_THRESHOLD = 10.0f;
    private const string BLOCK_LAYER_NAME = "Block";

    private Camera _cam;
    private LayerMask _blockLayer;

    /// <summary>선택된 블럭</summary>
    private Block _pickedBlock;
    private Vector2 _downScreen;
    private bool _swapFlag;

    private void Awake()
    {
        _cam = Camera.main;
        _blockLayer = LayerMask.GetMask(BLOCK_LAYER_NAME);
    }

    private void Update()
    {
        if(_swapFlag || Managers.Match == null)
        { 
            return;
        }

        if(Input.GetMouseButtonDown(0))
        {
            _downScreen = Input.mousePosition;
            _pickedBlock = RaycastBlock(_downScreen);
        }
        else if(Input.GetMouseButton(0) && _pickedBlock != null)
        {
            Vector2 delta = (Vector2)Input.mousePosition - _downScreen;
            if(delta.sqrMagnitude >= DRAG_THRESHOLD * DRAG_THRESHOLD)
            {
                TryDragSwap(_pickedBlock, delta);
                _pickedBlock = null;
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            _pickedBlock = null;
        }
    }

    private Block RaycastBlock(Vector2 screenPos)
    {
        Vector2 world = _cam.ScreenToWorldPoint(screenPos);
        var hit = Physics2D.Raycast(world, Vector2.zero, 0f, _blockLayer);
        if(hit.collider == null)
        { 
            return null;
        }

        return hit.collider.GetComponent<Block>();
    }

    private void TryDragSwap(Block picked, Vector2 dragDelta)
    {
        HexPos from = new HexPos(picked.Pos.Q, picked.Pos.R);
        HexPos dir = Managers.Match.GetNearestHexDir(dragDelta);
        HexPos to = new HexPos(from.Q + dir.Q, from.R + dir.R);
        if(!Managers.Match.Inside(to))
        { 
            return;
        }

        StartCoroutine(CoSwapAndResolve(from, to));
    }

    private IEnumerator CoSwapAndResolve(HexPos from, HexPos to)
    {
        _swapFlag = true;

        bool success = false;
        yield return StartCoroutine(Managers.Match.CoTrySwap(from, to, (isSwapped) => success = isSwapped));

        if(success)
        {
            yield return StartCoroutine(Managers.Match.CoMatchStepLoop());
        }

        _swapFlag = false;
    }
}
