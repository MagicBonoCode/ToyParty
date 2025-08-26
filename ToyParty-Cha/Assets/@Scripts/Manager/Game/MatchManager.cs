using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MissionType { Spinning, }
public enum LineType { Vertical, DiagDownLeft, DiagDownRight, }

/// <summary>
/// 매치된 라인
/// </summary>
public class MatchLine
{
    /// <summary>매치된 좌표 목록</summary>
    public List<HexPos> Cells { get; private set; } = new List<HexPos>();
    /// <summary>매치된 색상</summary>
    public Block.BlockColor Color { get; set; }

    /// <summary>매치된 길이</summary>
    public int Length { get { return Cells.Count; } }
}

/// <summary>
/// 매치 결과
/// </summary>
public class MatchResult
{
    /// <summary>매치된 라인 목록</summary>
    public List<MatchLine> Lines = new List<MatchLine>();

    public List<MatchLine> RockectLine => Lines.FindAll(x => x.Length == 4);
    public List<MatchLine> RainbowLine => Lines.FindAll(x => x.Length >= 5);

    /// <summary>매치된 전체 좌표 집합</summary>
    public HashSet<HexPos> AllhexPos = new HashSet<HexPos>();

    /// <summary>매치된 게 하나라도 있는지 여부</summary>
    public bool HasAny { get { return AllhexPos.Count > 0; } }

    /// <summary>매치 라인 추가합니다.</summary>
    public void AddLine(MatchLine line)
    {
        Lines.Add(line);
        for(int i = 0; i < line.Cells.Count; i++)
        {
            AllhexPos.Add(line.Cells[i]);
        }
    }

    /// <summary>매치라인을 제거합니다. *참조(Reference)로 확인합시다.</summary>
    /// <param name="line"></param>
    public void RemoveLine(MatchLine line)
    {
        Lines.Remove(line);
        for(int i = 0; i < line.Cells.Count; i++)
        {
            AllhexPos.Remove(line.Cells[i]);
        }
    }

    /// <summary>특수 매치가능한 매치라인을 반환합니다.</summary>
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

    // 6각 방향: (+1,0) = 가로, (+1,-1) = 대각, (0,-1) = 세로
    private readonly int[] _deltaQ = { +1, +1,  0 };
    private readonly int[] _deltaR = {  0, -1, -1 };

    // 최대 보드 크기
    public int MaxWidth { get; private set; }
    public int MaxHeight { get; private set; }

    // 타일 / 보드
    private Tile[,] _tiles => Managers.Object.Tiles;
    private Block[,] _blocks => Managers.Object.Blocks;

    // 게임 정보
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

    // 낙하 좌우 순환 Flag
    private int _stepParity = 0;

    public void Initialize(LevelData levelData)
    {
        // Info 세팅
        MaxWidth = levelData.MaxWidth;
        MaxHeight = levelData.MaxHeight;
        Level = levelData.Level;
        _moveCount = 999; // TODO: 추후 데이터 빼시오..
        MissionList.Clear();
        MissionList = levelData.MissionList;

        // Tile / Block 배열 준비
        Managers.Object.ClearAll();
        Managers.Object.SetTileAndBlock(MaxWidth, MaxHeight);

        // Tile 설정
        int tileListIndex = 0;
        for(int q = 0; q < MaxWidth; q++)
        {
            for(int r = 0; r < MaxHeight; r++)
            {
                Managers.Object.SpawnTile(levelData.TileList[tileListIndex]);
                tileListIndex++;
            }
        }

        // Block 설정
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
    /// 해당 좌표에 바로 매칭 되지 않는 색을 뽑습니다.
    /// </summary>
    /// <param name="q">Q</param>
    /// <param name="r">R</param>
    /// <param name="tryCount">색 뽑기 횟수 초과시 랜덤으로 전달</param>
    /// <returns>매칭 되지 않는 색</returns>
    private Block.BlockColor PickColorForSpawn(int q, int r, int tryCount)
    {
        for(int count = 0; count < tryCount; count++)
        {
            Block.BlockColor color = (Block.BlockColor)Util.GetRandomInt((int)Block.BlockColor.Red, (int)Block.BlockColor.Orange);
            if(IsSafeToPlaceColor(q, r, color)) return color;
        }

        // 실패시 그냥 랜덤
        return (Block.BlockColor)Util.GetRandomInt((int)Block.BlockColor.Red, (int)Block.BlockColor.Orange);
    }


    /// <summary>
    /// 해당 좌표에 해당 색을 놓았을 때 매칭이 되는지 검사합니다.
    /// </summary>
    /// <param name="q">Q</param>
    /// <param name="r">R</param>
    /// <param name="color">색</param>
    /// <returns>바로 매칭이 되는지 결과</returns>
    private bool IsSafeToPlaceColor(int q, int r, Block.BlockColor color)
    {
        for(int dir = 0; dir < 3; dir++)
        {
            int deltaQ = _deltaQ[dir];
            int deltaR = _deltaR[dir];

            // 같은 색이 연속되는 길이 (자신 포함)
            int count = 1;

            // 한쪽(-) 방향
            int currentQ = q - deltaQ;
            int currentR = r - deltaR;
            while(Inside(currentQ, currentR) && _tiles[currentQ, currentR] != null)
            {
                Block b = _blocks[currentQ, currentR];
                if(b == null || !b.IsMatchable || b.Color != color) break;
                count++;
                currentQ -= deltaQ; currentR -= deltaR;
            }

            // 반대(+) 방향
            currentQ = q + deltaQ; currentR = r + deltaR;
            while(Inside(currentQ, currentR) && _tiles[currentQ, currentR] != null)
            {
                Block b = _blocks[currentQ, currentR];
                if(b == null || !b.IsMatchable || b.Color != color) break;
                count++;
                currentQ += deltaQ; currentR += deltaR;
            }

            if(count >= 3) return false; // 즉시 매치가 생김 → 안전하지 않다
        }

        return true; // 모든 축에서 안전
    }

    // 해당 좌표가 보드 내에 있는지 검사 (보드 내 + 타일이 활성화된 곳)
    public bool Inside(HexPos p) { return Inside(p.Q, p.R); }
    public bool Inside(int q, int r) { return q >= 0 && q < MaxWidth && r >= 0 && r < MaxHeight && _tiles[q, r].IsActive; }

    // 블럭이 놓일 수 있는 빈 공간인지 검사 (보드 내 + 타일이 활성화된 곳 + 블럭 없음)
    public bool IsEmptyPlayable(HexPos p) { return Inside(p) && _blocks[p.Q, p.R] == null; }

    // 블럭의 월드 좌표 반환
    public Vector2 AxialToWorld(HexPos p) { return AxialToWorld(p.Q, p.R); }
    public Vector2 AxialToWorld(int q, int r)
    {
        float x = HEX_RADIUS * (1.5f) * q;
        float y = HEX_RADIUS * (Mathf.Sqrt(3f) * (r + q * 0.5f));
        return new Vector2(x, y);
    }

    /// <summary>
    /// 해당 좌표의 6방향 이웃 좌표 목록 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public List<HexPos> GetNeighbors(HexPos pos)
    {
        List<HexPos> list = new List<HexPos>(6);
        for(int i = 0; i < 3; i++)
        {
            // +방향
            HexPos plus = new HexPos(pos.Q + _deltaQ[i], pos.R + _deltaR[i]);
            if(Inside(plus)) list.Add(plus);

            // -방향
            HexPos minus = new HexPos(pos.Q - _deltaQ[i], pos.R - _deltaR[i]);
            if(Inside(minus)) list.Add(minus);
        }

        return list;
    }

    /// <summary>
    /// 화면 드래그 델타(스크린 기준)를 받아서, 6각 방향 중 가장 가까운 축을 반환합니다.
    /// </summary>
    /// <param name="dragScreenDelta">드레그 방향</param>
    /// <returns>6각 방향 중 가장 가까운 축</returns>
    public HexPos GetNearestHexDir(Vector2 dragScreenDelta)
    {
        Vector2 worldDelta = Camera.main.ScreenToWorldPoint((Vector3)dragScreenDelta) - Camera.main.ScreenToWorldPoint(Vector3.zero);

        float bestDot = float.NegativeInfinity;
        HexPos nearestHexPos = new HexPos(_deltaQ[0], _deltaR[0]);

        for(int i = 0; i < 3; i++)
        {
            // +방향
            Vector2 offsetPlus = AxialToWorld(new HexPos(_deltaQ[i], _deltaR[i])) - AxialToWorld(new HexPos(0, 0));
            float dotPlus = Vector2.Dot(worldDelta.normalized, offsetPlus.normalized);
            if(dotPlus > bestDot)
            {
                bestDot = dotPlus;
                nearestHexPos = new HexPos(_deltaQ[i], _deltaR[i]);
            }

            // -방향
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
    /// 특정 색상의 전체 좌표 리시트를 반환합니다.
    /// </summary>
    /// <param name="color">색상</param>
    /// <returns>전체 좌표 리스트</returns>
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
    /// 특정 좌표에서 특정 축을 따라 쭉 뻗은 라인 좌표를 수집합니다.
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
    /// 특정 열(q)에서 가장 위에 있는 스폰 가능한 행(r)을 반환합니다. (없으면 -1)
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
    /// 매칭 스텝(프로세스)을 실행합니다.
    /// </summary>
    public IEnumerator CoMatchStepLoop()
    {
        MoveCount--;

        while(true)
        {
            // 매치 검사
            MatchResult matchResult = FindMatches();

            // 매치가 없으면 종료
            if(!matchResult.HasAny)
            {
                yield break;
            }

            // 특수 생성
            foreach(var line in matchResult.RainbowLine)
            {
                // 매칭된 블럭 활성화
                ActivateBlockList(line.Cells);
                yield return new WaitForSeconds(ACTIVE_DELAY);

                // 레인보우 생성
                HexPos hexPos = line.Cells[Util.GetRandomInt(0, line.Length - 1)];
                Managers.Object.SpawnRainbowBlock(hexPos);
            }

            foreach(var line in matchResult.RockectLine)
            {
                // 매칭된 블럭 활성화
                ActivateBlockList(line.Cells);
                yield return new WaitForSeconds(ACTIVE_DELAY);

                // 로켓 생성
                HexPos hexPos = line.Cells[Util.GetRandomInt(0, line.Length - 1)];
                Managers.Object.SpawnRocketBlock(hexPos, line.Color);
            }

            // 특수 블럭 매칭 삭제
            foreach(var line in matchResult.RainbowLine.ToList())
            {
                matchResult.RemoveLine(line);
            }
            foreach(var line in matchResult.RockectLine.ToList())
            {
                matchResult.RemoveLine(line);
            }

            // 블럭 매칭 활성화
            bool isActivate = ActivateMatches(matchResult);
            if(isActivate)
            {
                yield return new WaitForSeconds(ACTIVE_DELAY);
            }

            while(true)
            {
                bool stepped = ApplyGravityAndSpawnOneStep();

                if(!stepped) break; // 더 이상 할 게 없으면 종료

                // 이동/스폰이 시작됐으니, 모두 끝날 때까지 한 번만 대기
                while(Block.MovingCount > 0) yield return null;
                if(STEP_DELAY > 0f) yield return new WaitForSeconds(STEP_DELAY);
            }
        }
    }

    /// <summary>
    /// 블럭 리스트를 받아서 일괄 활성화(매칭, 삭제) 합니다.
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
    /// 두 좌표의 블럭을 스왑 시도합니다. (매치가 되지 않으면 스왑백)
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

        // === Rainbow 스왑 특수 처리 ===
        // 레인보우 + 유색 블럭 => 유색의 "색 전체" 제거 (스왑 애니메이션 없이 즉시 발동)
        if(fromBlock.IsRainbow || toBlock.IsRainbow)
        {
            // 블럭 매칭 활성화
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

                if(!stepped) break; // 더 이상 할 게 없으면 종료

                // 이동/스폰이 시작됐으니, 모두 끝날 때까지 한 번만 대기
                while(Block.MovingCount > 0) yield return null;
                if(STEP_DELAY > 0f) yield return new WaitForSeconds(STEP_DELAY);
            }

            yield break;
        }

        // 블럭 스왑
        _blocks[from.Q, from.R] = toBlock;
        _blocks[to.Q, to.R] = fromBlock;

        fromBlock.SmoothToMove(to);
        toBlock.SmoothToMove(from);

        // 이동이 끝날때까 대기
        while(Block.MovingCount > 0)
        {
            yield return null;
        }

        // 매치 검사
        MatchResult matchResult = FindMatches();
        bool valid = matchResult != null && matchResult.HasAny;
        if(valid)
        {
            onComplete?.Invoke(true);
            yield break;
        }

        // 매치가 안되면 스왑백
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
    /// 매치 검사를 확인하고, 매치 결과를 반환합니다.
    /// </summary>
    /// <returns>매치 결과</returns>
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
                        // 이전 블럭이 같은 색이면 스킵 (중복 라인 방지)
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
    /// 매치된 블럭을 활성화합니다.
    /// </summary>
    /// <param name="matchResult">매칭 결과</param>
    public bool ActivateMatches(MatchResult matchResult)
    {
        HashSet<HexPos> activates = matchResult.AllhexPos.ToHashSet();

        // 팽이 : 주변이 터지면 같이 터짐
        // 매칭 타일 주변에 팽이가 있으면 매치에 추가
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

        // 매칭 활성화
        ActivateBlockList(activates.ToList());

        return activates.Count > 0;
    }

    /// <summary>
    /// 중력에 적용되는 블럭을 이동시킵니다.
    /// </summary>
    /// <returns>이동 확인</returns>
    public bool ApplyGravityAndSpawnOneStep()
    {
        // 이전 이동이 남아있으면 스킵(겹침 방지)
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

        // 낙하 좌우 순환
        _stepParity++;

        return any;
    }

    /// <summary>
    /// 블럭을 이동시킵니다.
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
        // 이전 이동이 남아있으면 스킵(겹침 방지)
        if(Block.MovingCount > 0)
        {
            return false;
        }

        bool movedAny = false;
        // 움직임 정보를 담은 튜플
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
        // 이전 이동이 남아있으면 스킵(겹침 방지)
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
    /// 최상단에서 블럭을 스폰합니다.
    /// </summary>
    /// <returns>스폰 확인</returns>
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
                continue;   // 최상단 칸이 비어 있을 때만
            }

            Block.BlockColor color = (Block.BlockColor)Util.GetRandomInt((int)global::Block.BlockColor.Red, (int)global::Block.BlockColor.Orange);
            if(color == global::Block.BlockColor.None)
            {
                color = global::Block.BlockColor.Red;
            }

            Block Block = Managers.Object.SpawnGemBlock(q, spawnableTop, color);

            Vector2 start = AxialToWorld(q, MaxHeight + 1); // 화면 위에서 내려오게
            Block.transform.localPosition = start;

            Block.SmoothToMove(new HexPos(q, spawnableTop));

            spawnedAny = true;
        }

        return spawnedAny;
    }

    #endregion
}
