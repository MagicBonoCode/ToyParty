using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MissionType { Spinning, }
public enum LineType { Vertical, DiagDownLeft, DiagDownRight, }

/// <summary>
/// ��ġ�� ����
/// </summary>
public class MatchLine
{
    /// <summary>��ġ�� ��ǥ ���</summary>
    public List<HexPos> Cells { get; private set; } = new List<HexPos>();
    /// <summary>��ġ�� ����</summary>
    public Block.BlockColor Color { get; set; }

    /// <summary>��ġ�� ����</summary>
    public int Length { get { return Cells.Count; } }
}

/// <summary>
/// ��ġ ���
/// </summary>
public class MatchResult
{
    /// <summary>��ġ�� ���� ���</summary>
    public List<MatchLine> Lines = new List<MatchLine>();

    public List<MatchLine> RockectLine => Lines.FindAll(x => x.Length == 4);
    public List<MatchLine> RainbowLine => Lines.FindAll(x => x.Length >= 5);

    /// <summary>��ġ�� ��ü ��ǥ ����</summary>
    public HashSet<HexPos> AllhexPos = new HashSet<HexPos>();

    /// <summary>��ġ�� �� �ϳ��� �ִ��� ����</summary>
    public bool HasAny { get { return AllhexPos.Count > 0; } }

    /// <summary>��ġ ���� �߰��մϴ�.</summary>
    public void AddLine(MatchLine line)
    {
        Lines.Add(line);
        for(int i = 0; i < line.Cells.Count; i++)
        {
            AllhexPos.Add(line.Cells[i]);
        }
    }

    /// <summary>��ġ������ �����մϴ�. *����(Reference)�� Ȯ���սô�.</summary>
    /// <param name="line"></param>
    public void RemoveLine(MatchLine line)
    {
        Lines.Remove(line);
        for(int i = 0; i < line.Cells.Count; i++)
        {
            AllhexPos.Remove(line.Cells[i]);
        }
    }

    /// <summary>Ư�� ��ġ������ ��ġ������ ��ȯ�մϴ�.</summary>
    /// <returns></returns>
    public List<MatchLine> GetSpecialCandidates()
    {
        List<MatchLine> list = new List<MatchLine>();
        for(int i = 0; i < Lines.Count; i++)
        {
            if(Lines[i].Length >= 4)
            {
                list.Add(Lines[i]);
            }
        }

        return list;
    }
}

public class MatchManager
{
    public const float HEX_RADIUS = 0.5f;
    public const float STEP_DELAY = 0.02f;
    public const float ACTIVE_DELAY = 0.3f;

    // 6�� ����: (+1,0) = ����, (+1,-1) = �밢, (0,-1) = ����
    private readonly int[] _deltaQ = { +1, +1,  0 };
    private readonly int[] _deltaR = {  0, -1, -1 };

    // �ִ� ���� ũ��
    public int MaxWidth { get; private set; }
    public int MaxHeight { get; private set; }

    // Ÿ�� / ����
    private Tile[,] _tiles => Managers.Object.Tiles;
    private Block[,] _blocks => Managers.Object.Blocks;

    // ���� ����
    public int Level { get; private set; }

    private int _moveCount;
    public int MoveCount
    {
        get { return _moveCount; }
        private set
        {
            if(_moveCount == 0)
            {
                UI_GameResultPopup popup = Managers.UI.ShowPopupUI<UI_GameResultPopup>();
                popup.SetInfo(false);
                return;
            }

            _moveCount = value;
            Managers.Event.TriggerEvent(EEventType.Update_MoveCount);
        }
    }

    public List<MissionData> MissionList { get; set; } = new List<MissionData>();
    public bool IsCleared
    {
        get
        {
            foreach(MissionData mission in MissionList)
            {
                if(mission.Count > 0)
                {
                    return false;
                }
            }
            return true;
        }
    }

    // ���� �¿� ��ȯ Flag
    private int _stepParity = 0;

    public void Initialize(LevelData levelData)
    {
        // Info ����
        MaxWidth = levelData.MaxWidth;
        MaxHeight = levelData.MaxHeight;
        Level = levelData.Level;
        _moveCount = 999; // TODO: ���� ������ ���ÿ�..
        MissionList.Clear();
        MissionList = levelData.MissionList;

        // Tile / Block �迭 �غ�
        Managers.Object.ClearAll();
        Managers.Object.SetTileAndBlock(MaxWidth, MaxHeight);

        // Tile ����
        int tileListIndex = 0;
        for(int q = 0; q < MaxWidth; q++)
        {
            for(int r = 0; r < MaxHeight; r++)
            {
                Managers.Object.SpawnTile(levelData.TileList[tileListIndex]);
                tileListIndex++;
            }
        }

        // Block ����
        int blockListIndex = 0;
        for(int q = 0; q < MaxWidth; q++)
        {
            for(int r = 0; r < MaxHeight; r++)
            {
                Managers.Object.SpawnBlock(levelData.BlockList[blockListIndex]);
                blockListIndex++;
            }
        }
    }

    #region Util

    /// <summary>
    /// �ش� ��ǥ�� �ٷ� ��Ī ���� �ʴ� ���� �̽��ϴ�.
    /// </summary>
    /// <param name="q">Q</param>
    /// <param name="r">R</param>
    /// <param name="tryCount">�� �̱� Ƚ�� �ʰ��� �������� ����</param>
    /// <returns>��Ī ���� �ʴ� ��</returns>
    private Block.BlockColor PickColorForSpawn(int q, int r, int tryCount)
    {
        for(int count = 0; count < tryCount; count++)
        {
            Block.BlockColor color = (Block.BlockColor)Util.GetRandomInt((int)Block.BlockColor.Red, (int)Block.BlockColor.Orange);
            if(IsSafeToPlaceColor(q, r, color)) return color;
        }

        // ���н� �׳� ����
        return (Block.BlockColor)Util.GetRandomInt((int)Block.BlockColor.Red, (int)Block.BlockColor.Orange);
    }


    /// <summary>
    /// �ش� ��ǥ�� �ش� ���� ������ �� ��Ī�� �Ǵ��� �˻��մϴ�.
    /// </summary>
    /// <param name="q">Q</param>
    /// <param name="r">R</param>
    /// <param name="color">��</param>
    /// <returns>�ٷ� ��Ī�� �Ǵ��� ���</returns>
    private bool IsSafeToPlaceColor(int q, int r, Block.BlockColor color)
    {
        for(int dir = 0; dir < 3; dir++)
        {
            int deltaQ = _deltaQ[dir];
            int deltaR = _deltaR[dir];

            // ���� ���� ���ӵǴ� ���� (�ڽ� ����)
            int count = 1;

            // ����(-) ����
            int currentQ = q - deltaQ;
            int currentR = r - deltaR;
            while(Inside(currentQ, currentR) && _tiles[currentQ, currentR] != null)
            {
                Block b = _blocks[currentQ, currentR];
                if(b == null || !b.IsMatchable || b.Color != color) break;
                count++;
                currentQ -= deltaQ; currentR -= deltaR;
            }

            // �ݴ�(+) ����
            currentQ = q + deltaQ; currentR = r + deltaR;
            while(Inside(currentQ, currentR) && _tiles[currentQ, currentR] != null)
            {
                Block b = _blocks[currentQ, currentR];
                if(b == null || !b.IsMatchable || b.Color != color) break;
                count++;
                currentQ += deltaQ; currentR += deltaR;
            }

            if(count >= 3) return false; // ��� ��ġ�� ���� �� �������� �ʴ�
        }

        return true; // ��� �࿡�� ����
    }

    // �ش� ��ǥ�� ���� ���� �ִ��� �˻� (���� �� + Ÿ���� Ȱ��ȭ�� ��)
    public bool Inside(HexPos p) { return Inside(p.Q, p.R); }
    public bool Inside(int q, int r) { return q >= 0 && q < MaxWidth && r >= 0 && r < MaxHeight && _tiles[q, r].IsActive; }

    // ���� ���� �� �ִ� �� �������� �˻� (���� �� + Ÿ���� Ȱ��ȭ�� �� + �� ����)
    public bool IsEmptyPlayable(HexPos p) { return Inside(p) && _blocks[p.Q, p.R] == null; }

    // ���� ���� ��ǥ ��ȯ
    public Vector2 AxialToWorld(HexPos p) { return AxialToWorld(p.Q, p.R); }
    public Vector2 AxialToWorld(int q, int r)
    {
        float x = HEX_RADIUS * (1.5f) * q;
        float y = HEX_RADIUS * (Mathf.Sqrt(3f) * (r + q * 0.5f));
        return new Vector2(x, y);
    }

    /// <summary>
    /// �ش� ��ǥ�� 6���� �̿� ��ǥ ��� ��ȯ
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public List<HexPos> GetNeighbors(HexPos pos)
    {
        List<HexPos> list = new List<HexPos>(6);
        for(int i = 0; i < 3; i++)
        {
            // +����
            HexPos plus = new HexPos(pos.Q + _deltaQ[i], pos.R + _deltaR[i]);
            if(Inside(plus)) list.Add(plus);

            // -����
            HexPos minus = new HexPos(pos.Q - _deltaQ[i], pos.R - _deltaR[i]);
            if(Inside(minus)) list.Add(minus);
        }

        return list;
    }

    /// <summary>
    /// ȭ�� �巡�� ��Ÿ(��ũ�� ����)�� �޾Ƽ�, 6�� ���� �� ���� ����� ���� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="dragScreenDelta">�巹�� ����</param>
    /// <returns>6�� ���� �� ���� ����� ��</returns>
    public HexPos GetNearestHexDir(Vector2 dragScreenDelta)
    {
        Vector2 worldDelta = Camera.main.ScreenToWorldPoint((Vector3)dragScreenDelta) - Camera.main.ScreenToWorldPoint(Vector3.zero);

        float bestDot = float.NegativeInfinity;
        HexPos nearestHexPos = new HexPos(_deltaQ[0], _deltaR[0]);

        for(int i = 0; i < 3; i++)
        {
            // +����
            Vector2 offsetPlus = AxialToWorld(new HexPos(_deltaQ[i], _deltaR[i])) - AxialToWorld(new HexPos(0, 0));
            float dotPlus = Vector2.Dot(worldDelta.normalized, offsetPlus.normalized);
            if(dotPlus > bestDot)
            {
                bestDot = dotPlus;
                nearestHexPos = new HexPos(_deltaQ[i], _deltaR[i]);
            }

            // -����
            Vector2 offsetMinus = AxialToWorld(new HexPos(-_deltaQ[i], -_deltaR[i])) - AxialToWorld(new HexPos(0, 0));
            float dotMinus = Vector2.Dot(worldDelta.normalized, offsetMinus.normalized);
            if(dotMinus > bestDot)
            {
                bestDot = dotMinus;
                nearestHexPos = new HexPos(-_deltaQ[i], -_deltaR[i]);
            }
        }

        return nearestHexPos;
    }

    /// <summary>
    /// Ư�� ������ ��ü ��ǥ ����Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="color">����</param>
    /// <returns>��ü ��ǥ ����Ʈ</returns>
    public List<HexPos> GetSameColorHexPosList(Block.BlockColor color)
    {
        List<HexPos> sameColorHexPosList = new List<HexPos>();
        for(int q = 0; q < MaxWidth; q++)
        {
            for(int r = 0; r < MaxHeight; r++)
            {
                if(_tiles[q, r] == null || !_tiles[q, r].IsActive)
                {
                    continue;
                }

                if(_blocks[q, r] != null && _blocks[q, r].Color == color)
                {
                    sameColorHexPosList.Add(new HexPos(q, r));
                }
            }
        }

        return sameColorHexPosList;
    }

    /// <summary>
    /// Ư�� ��ǥ���� Ư�� ���� ���� �� ���� ���� ��ǥ�� �����մϴ�.
    /// </summary>
    /// <param name="centerPos"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public List<HexPos> GetCollectLine(HexPos centerPos, LineType type)
    {
        List<HexPos> list = new List<HexPos>();
        list.Add(centerPos);

        int forwardDeltaQ = 0;
        int forwardDeltaR = 0;
        int backwardDeltaQ = 0;
        int backwardDeltaR = 0;

        switch(type)
        {
            case LineType.Vertical:
                forwardDeltaQ = 0; forwardDeltaR = -1; backwardDeltaQ = 0; backwardDeltaR = +1;
                break;
            case LineType.DiagDownLeft:
                forwardDeltaQ = -1; forwardDeltaR = 0; backwardDeltaQ = +1; backwardDeltaR = 0;
                break;
            case LineType.DiagDownRight:
                forwardDeltaQ = +1; forwardDeltaR = -1; backwardDeltaQ = -1; backwardDeltaR = +1;
                break;

            default:
                break;
        }

        HexPos pos = centerPos;
        while(true)
        {
            pos = new HexPos(pos.Q + forwardDeltaQ, pos.R + forwardDeltaR);
            if(!Inside(pos) || _tiles[pos.Q, pos.R] == null)
            {
                break;
            }

            list.Add(pos);
        }

        pos = centerPos;
        while(true)
        {
            pos = new HexPos(pos.Q + backwardDeltaQ, pos.R + backwardDeltaR);
            if(!Inside(pos) || _tiles[pos.Q, pos.R] == null)
            {
                break;
            }

            list.Add(pos);
        }

        return list;
    }

    /// <summary>
    /// Ư�� ��(q)���� ���� ���� �ִ� ���� ������ ��(r)�� ��ȯ�մϴ�. (������ -1)
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    public int GetTopSpawnableRow(int q)
    {
        for(int r = MaxHeight - 1; r >= 0; r--)
        {
            if(_tiles[q, r] != null && _tiles[q, r].IsSpawnPoint)
            {
                return r;
            }
        }

        return -1;
    }

    #endregion

    #region System

    /// <summary>
    /// ��Ī ����(���μ���)�� �����մϴ�.
    /// </summary>
    public IEnumerator CoMatchStepLoop()
    {
        MoveCount--;

        while(true)
        {
            // ��ġ �˻�
            MatchResult matchResult = FindMatches();

            // ��ġ�� ������ ����
            if(!matchResult.HasAny)
            {
                yield break;
            }

            // Ư�� ����
            foreach(var line in matchResult.RainbowLine)
            {
                // ��Ī�� �� Ȱ��ȭ
                ActivateBlockList(line.Cells);
                yield return new WaitForSeconds(ACTIVE_DELAY);

                // ���κ��� ����
                HexPos hexPos = line.Cells[Util.GetRandomInt(0, line.Length - 1)];
                Managers.Object.SpawnRainbowBlock(hexPos);
            }

            foreach(var line in matchResult.RockectLine)
            {
                // ��Ī�� �� Ȱ��ȭ
                ActivateBlockList(line.Cells);
                yield return new WaitForSeconds(ACTIVE_DELAY);

                // ���� ����
                HexPos hexPos = line.Cells[Util.GetRandomInt(0, line.Length - 1)];
                Managers.Object.SpawnRocketBlock(hexPos, line.Color);
            }

            // Ư�� �� ��Ī ����
            foreach(var line in matchResult.RainbowLine.ToList())
            {
                matchResult.RemoveLine(line);
            }
            foreach(var line in matchResult.RockectLine.ToList())
            {
                matchResult.RemoveLine(line);
            }

            // �� ��Ī Ȱ��ȭ
            bool isActivate = ActivateMatches(matchResult);
            if(isActivate)
            {
                yield return new WaitForSeconds(ACTIVE_DELAY);
            }

            while(true)
            {
                bool stepped = ApplyGravityAndSpawnOneStep();

                if(!stepped) break; // �� �̻� �� �� ������ ����

                // �̵�/������ ���۵�����, ��� ���� ������ �� ���� ���
                while(Block.MovingCount > 0) yield return null;
                if(STEP_DELAY > 0f) yield return new WaitForSeconds(STEP_DELAY);
            }
        }
    }

    /// <summary>
    /// �� ����Ʈ�� �޾Ƽ� �ϰ� Ȱ��ȭ(��Ī, ����) �մϴ�.
    /// </summary>
    /// <param name="hexPosList"></param>
    public void ActivateBlockList(List<HexPos> hexPosList)
    {
        for(int i = 0; i < hexPosList.Count; i++)
        {
            HexPos pos = hexPosList[i];
            if(!Inside(pos))
            {
                continue;
            }

            Block block = _blocks[pos.Q,pos.R];
            if(block == null)
            {
                continue;
            }

            block.ActivateBlock();
        }
    }

    /// <summary>
    /// �� ��ǥ�� ���� ���� �õ��մϴ�. (��ġ�� ���� ������ ���ҹ�)
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    public IEnumerator CoTrySwap(HexPos from, HexPos to, Action<bool> onComplete)
    {
        if(!Inside(from) || !Inside(to))
        {
            onComplete?.Invoke(false);
            yield break;
        }

        Block fromBlock = _blocks[from.Q, from.R];
        Block toBlock = _blocks[to.Q, to.R];

        if(fromBlock == null || toBlock == null || !fromBlock.IsSwappable || !toBlock.IsSwappable)
        {
            onComplete?.Invoke(false);
            yield break;
        }

        // === Rainbow ���� Ư�� ó�� ===
        // ���κ��� + ���� �� => ������ "�� ��ü" ���� (���� �ִϸ��̼� ���� ��� �ߵ�)
        if(fromBlock.IsRainbow || toBlock.IsRainbow)
        {
            // �� ��Ī Ȱ��ȭ
            if(fromBlock.IsRainbow)
            {
                RainbowBlock rainbow = fromBlock as RainbowBlock;
                rainbow.ActiveRainbowBlock(toBlock.Color);
                toBlock.ActivateBlock();
            }
            else
            {
                RainbowBlock rainbow = toBlock as RainbowBlock;
                rainbow.ActiveRainbowBlock(fromBlock.Color);
                fromBlock.ActivateBlock();
            }

            yield return new WaitForSeconds(ACTIVE_DELAY);

            onComplete?.Invoke(false);

            while(true)
            {
                bool stepped = ApplyGravityAndSpawnOneStep();

                if(!stepped) break; // �� �̻� �� �� ������ ����

                // �̵�/������ ���۵�����, ��� ���� ������ �� ���� ���
                while(Block.MovingCount > 0) yield return null;
                if(STEP_DELAY > 0f) yield return new WaitForSeconds(STEP_DELAY);
            }

            yield break;
        }

        // �� ����
        _blocks[from.Q, from.R] = toBlock;
        _blocks[to.Q, to.R] = fromBlock;

        fromBlock.SmoothToMove(to);
        toBlock.SmoothToMove(from);

        // �̵��� �������� ���
        while(Block.MovingCount > 0)
        {
            yield return null;
        }

        // ��ġ �˻�
        MatchResult matchResult = FindMatches();
        bool valid = matchResult != null && matchResult.HasAny;
        if(valid)
        {
            onComplete?.Invoke(true);
            yield break;
        }

        // ��ġ�� �ȵǸ� ���ҹ�
        _blocks[from.Q, from.R] = fromBlock;
        _blocks[to.Q, to.R] = toBlock;
        fromBlock.SmoothToMove(from);
        toBlock.SmoothToMove(to);
        while(Block.MovingCount > 0)
        {
            yield return null;
        }

        onComplete?.Invoke(false);
    }

    /// <summary>
    /// ��ġ �˻縦 Ȯ���ϰ�, ��ġ ����� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>��ġ ���</returns>
    public MatchResult FindMatches()
    {
        MatchResult result = new MatchResult();

        for(int dir = 0; dir < 3; dir++)
        {
            int deltaQ = _deltaQ[dir];
            int deltaR = _deltaR[dir];

            for(int q = 0; q < MaxWidth; q++)
            {
                for(int r = 0; r < MaxHeight; r++)
                {
                    Block block = _blocks[q,r];
                    if(block == null || !block.IsMatchable)
                    {
                        continue;
                    }

                    int prevQ = q - deltaQ;
                    int prevR = r - deltaR;
                    if(Inside(prevQ, prevR))
                    {
                        Block prevBlock = _blocks[prevQ,prevR];
                        // ���� ���� ���� ���̸� ��ŵ (�ߺ� ���� ����)
                        if(prevBlock != null && prevBlock.IsMatchable && prevBlock.Color == block.Color)
                        {
                            continue;
                        }
                    }

                    MatchLine matchLine = new MatchLine();
                    matchLine.Color = block.Color;

                    int currentQ = q;
                    int currentR = r;
                    while(Inside(currentQ, currentR))
                    {
                        Block currentBlock = _blocks[currentQ,currentR];
                        if(currentBlock == null || !currentBlock.IsMatchable || currentBlock.Color != block.Color)
                        {
                            break;
                        }

                        matchLine.Cells.Add(new HexPos(currentQ, currentR));
                        currentQ += deltaQ;
                        currentR += deltaR;
                    }

                    if(matchLine.Length >= 3)
                    {
                        result.AddLine(matchLine);
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// ��ġ�� ���� Ȱ��ȭ�մϴ�.
    /// </summary>
    /// <param name="matchResult">��Ī ���</param>
    public bool ActivateMatches(MatchResult matchResult)
    {
        HashSet<HexPos> activates = matchResult.AllhexPos.ToHashSet();

        // ���� : �ֺ��� ������ ���� ����
        // ��Ī Ÿ�� �ֺ��� ���̰� ������ ��ġ�� �߰�
        foreach(HexPos pos in matchResult.AllhexPos)
        {
            List<HexPos> neighborPosList = GetNeighbors(pos);
            for(int index = 0; index < neighborPosList.Count; index++)
            {
                HexPos neighborPos = neighborPosList[index];
                Block neighborBlock = _blocks[neighborPos.Q, neighborPos.R];
                if(neighborBlock != null && neighborBlock.Type == Block.BlockType.Spinning)
                {
                    if(!matchResult.AllhexPos.Contains(neighborPos))
                    {
                        activates.Add(neighborPos);
                    }
                }
            }
        }

        // ��Ī Ȱ��ȭ
        ActivateBlockList(activates.ToList());

        return activates.Count > 0;
    }

    /// <summary>
    /// �߷¿� ����Ǵ� ���� �̵���ŵ�ϴ�.
    /// </summary>
    /// <returns>�̵� Ȯ��</returns>
    public bool ApplyGravityAndSpawnOneStep()
    {
        // ���� �̵��� ���������� ��ŵ(��ħ ����)
        if(Block.MovingCount > 0)
        {
            return false;
        }

        bool any = false;
        var movedThisStep = new HashSet<Block>();

        bool down = ApplyGravityDownStep(movedThisStep);
        any = any || down;

        bool diag = ApplyGravityDiagStep(movedThisStep);
        any = any || diag;

        bool spawned = SpawnTopStep();
        any = any || spawned;

        // ���� �¿� ��ȯ
        _stepParity++;

        return any;
    }

    /// <summary>
    /// ���� �̵���ŵ�ϴ�.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public void MoveBlock(HexPos from, HexPos to)
    {
        Block block = _blocks[from.Q,from.R];
        _blocks[from.Q, from.R] = null;
        _blocks[to.Q, to.R] = block;
        block.SmoothToMove(to);
    }

    public bool ApplyGravityDownStep(HashSet<Block> movedThisStep)
    {
        // ���� �̵��� ���������� ��ŵ(��ħ ����)
        if(Block.MovingCount > 0)
        {
            return false;
        }

        bool movedAny = false;
        // ������ ������ ���� Ʃ��
        List<(HexPos from, HexPos to, Block block)> moveInfoList = new List<(HexPos, HexPos, Block)>();
        HashSet<(int q,int r)> reserved = new HashSet<(int,int)>();

        for(int r = 1; r < MaxHeight; r++)
        {
            bool leftToRight = (_stepParity % 2) == 0;
            int qStart = leftToRight ? 0 : MaxWidth - 1;
            int qEnd = leftToRight ? MaxWidth : -1;
            int qStep = leftToRight ? 1 : -1;

            for(int q = qStart; q != qEnd; q += qStep)
            {
                Block block = _blocks[q, r];
                if(block == null || block.Type == Block.BlockType.None)
                {
                    continue;
                }

                HexPos from = new HexPos(q, r);
                HexPos down = new HexPos(q, r - 1);

                if(IsEmptyPlayable(down) && reserved.Add((down.Q, down.R)))
                {
                    moveInfoList.Add((from, down, block));
                }
            }
        }

        for(int i = 0; i < moveInfoList.Count; i++)
        {
            var moveInfo = moveInfoList[i];
            if(_blocks[moveInfo.from.Q, moveInfo.from.R] != moveInfo.block)
            {
                continue;
            }
            if(_blocks[moveInfo.to.Q, moveInfo.to.R] != null)
            {
                continue;
            }

            MoveBlock(moveInfo.from, moveInfo.to);
            movedAny = true;
            if(movedThisStep != null)
            {
                movedThisStep.Add(moveInfo.block);
            }
        }

        return movedAny;
    }

    public bool ApplyGravityDiagStep(HashSet<Block> movedThisStep)
    {
        // ���� �̵��� ���������� ��ŵ(��ħ ����)
        if(Block.MovingCount > 0)
        {
            return false;
        }

        bool movedAny = false;
        List<(HexPos from, HexPos to, Block block)> moveInfoList = new List<(HexPos, HexPos, Block)>();
        HashSet<(int q,int r)> reserved = new HashSet<(int,int)>();

        for(int r = 1; r < MaxHeight; r++)
        {
            bool leftToRight = (_stepParity % 2) == 0;
            int qStart = leftToRight ? 0 : MaxWidth - 1;
            int qEnd = leftToRight ? MaxWidth : -1;
            int qStep = leftToRight ? 1 : -1;

            for(int q = qStart; q != qEnd; q += qStep)
            {
                Block block = _blocks[q, r];
                if(block == null || block.Type == Block.BlockType.None)
                {
                    continue;
                }

                if(movedThisStep != null && movedThisStep.Contains(block))
                {
                    continue;
                }

                HexPos from = new HexPos(q, r);
                HexPos downLeft = new HexPos(q - 1, r);
                HexPos downRight = new HexPos(q + 1, r - 1);

                if(IsEmptyPlayable(downLeft) && reserved.Add((downLeft.Q, downLeft.R)))
                {
                    moveInfoList.Add((from, downLeft, block));
                }
                else if(IsEmptyPlayable(downRight) && reserved.Add((downRight.Q, downRight.R)))
                {
                    moveInfoList.Add((from, downRight, block));
                }
            }
        }

        for(int i = 0; i < moveInfoList.Count; i++)
        {
            var moveInfo = moveInfoList[i];
            if(_blocks[moveInfo.from.Q, moveInfo.from.R] != moveInfo.block)
            {
                continue;
            }
            if(_blocks[moveInfo.to.Q, moveInfo.to.R] != null)
            {
                continue;
            }

            MoveBlock(moveInfo.from, moveInfo.to);
            movedAny = true;
        }

        return movedAny;
    }

    /// <summary>
    /// �ֻ�ܿ��� ���� �����մϴ�.
    /// </summary>
    /// <returns>���� Ȯ��</returns>
    public bool SpawnTopStep()
    {
        bool spawnedAny = false;
        for(int q = 0; q < MaxWidth; q++)
        {
            int spawnableTop = GetTopSpawnableRow(q);
            if(spawnableTop < 0)
            {
                continue;
            }

            if(_blocks[q, spawnableTop] != null)
            {
                continue;   // �ֻ�� ĭ�� ��� ���� ����
            }

            Block.BlockColor color = (Block.BlockColor)Util.GetRandomInt((int)global::Block.BlockColor.Red, (int)global::Block.BlockColor.Orange);
            if(color == global::Block.BlockColor.None)
            {
                color = global::Block.BlockColor.Red;
            }

            Block Block = Managers.Object.SpawnGemBlock(q, spawnableTop, color);

            Vector2 start = AxialToWorld(q, MaxHeight + 1); // ȭ�� ������ ��������
            Block.transform.localPosition = start;

            Block.SmoothToMove(new HexPos(q, spawnableTop));

            spawnedAny = true;
        }

        return spawnedAny;
    }

    #endregion
}
