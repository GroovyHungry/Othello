using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using AK.Wwise;

/// <summary>
/// オセロAIの思考・手番実行を管理するクラス
/// 難易度easy：完全ランダムな選択
/// 難易度normal：着手の場所重みとひっくり返す駒数の総合評価
/// 難易度hard：深さ2のminmaxを行い，その後の状況（プレイヤーの有効手数，駒差，駒数）と重みテーブルの総合評価
/// </summary>
public class OthelloAI : MonoBehaviour
{
    /// <summary>
    /// グローバルアクセス用インスタンス
    /// </summary>
    public static OthelloAI Instance;

    /// <summary>
    /// AI難易度 ("easy","normal","hard","secret")
    /// </summary>
    public string difficulty = "easy";

    /// <summary>
    /// 難易度normal：重みテーブル
    /// </summary>
    private static readonly int[,] normalDifficultyTable = new int[8, 8]
    {
        { 100, -20, 10, 5, 5, 10, -20, 100 },
        { -20, 0, 0, 0, 0, 0, 0, -20 },
        { 10,  0,  0,  0,  0,  0,  0,  10 },
        { 5,   0,  0,  0,  0,  0,  0,   5 },
        { 5,   0,  0,  0,  0,  0,  0,   5 },
        { 10,  0,  0,  0,  0,  0,  0,  10 },
        { -20, 0, 0, 0, 0, 0, 0, -20 },
        { 100, -20, 10, 5, 5, 10, -20, 100 }
    };
    /// <summary>
    /// 難易度normal：枚数重み
    /// </summary>
    private const int normalWeightFlip = 10;
    /// <summary>
    /// 難易度normal：位置重み
    /// </summary>
    private const int normalWeightPos = 1;

    /// <summary>
    /// 難易度hard：序盤重みテーブル
    /// </summary>
    private static readonly int[,] openingTable = new int[8,8]
    {
        {  28,  -9,  -1,   3,   1,  -3,  -9,  28 },
        { -14, -18,   0,  -4,   1,   0, -16, -14 },
        {   0,   0,  -5,  -1,   4,   0,  -5,   3 },
        {   2,  -5,   2,   1,  -1,   3,   0,  -1 },
        {   0,  -4,   2,  -1,  -2,   0,   0,  -2 },
        {   1,  -4,  -2,   1,  -2,  -3,  -5,   4 },
        { -12, -18,  -6,  -2,  -4,  -1, -14, -11 },
        {  30, -11,   3,  -5,   3,  -4, -11,  32 }
    };
    /// <summary>
    /// 難易度hard：序盤枚数重み
    /// </summary>
    private static readonly float openingWeightFlip     = 8.27f;
    /// <summary>
    /// 難易度hard：序盤有効手数重み
    /// </summary>
    private static readonly float openingWeightMobility = 1.00f;
    /// <summary>
    /// 難易度hard：序盤枚数差重み
    /// </summary>
    private static readonly float openingWeightDiff     = 0.22f;

    /// <summary>
    /// 難易度hard：中盤重みテーブル
    /// </summary>
    private static readonly int[,] midgameTable = new int[8,8]
    {
        { 23,  -4,  11,   5,  11,  10,  -4,  21 },
        { -6,  -5,  -7,   1,   4,  -3, -10,  -2 },
        {  7,  -4,   5,   6,  -1,   5,  -8,  11 },
        {  9,   4,   6,  -1,  -1,   1,  -2,   8 },
        {  6,  -2,  -1,  -2,   0,   0,  -2,  10 },
        { 12,  -7,   2,   0,   1,   1,  -4,  11 },
        { -4,  -3,  -2,   2,   4,  -3,  -6,  -7 },
        { 17,  -3,  14,  10,  10,  13,  -3,  22 }
    };
    /// <summary>
    /// 難易度hard：中盤枚数重み
    /// </summary>
    private static readonly float midgameWeightFlip     = 8.99f;
    /// <summary>
    /// 難易度hard：中盤有効手数重み
    /// </summary>
    private static readonly float midgameWeightMobility = 0.48f;
    /// <summary>
    /// 難易度hard：中盤枚数差重み
    /// </summary>
    private static readonly float midgameWeightDiff     = 0.45f;

    /// <summary>
    /// 難易度hard：終盤重みテーブル
    /// </summary>
    private static readonly int[,] endgameTable = new int[8,8]
    {
        {  2,   3,   2,   0,   2,   5,   4,  -2 },
        {  3,   3,  -2,   2,   1,  -3,  -3,   2 },
        {  0,   0,  -1,   4,  -2,   2,  -1,  -2 },
        { -3,  -1,  -2,  -1,   5,   2,  -1,  -2 },
        { -1,  -1,   3,   0,  -3,   0,   2,   4 },
        { -1,   3,  -3,  -1,   0,  -1,   0,   1 },
        {  0,   4,   4,  -1,  -2,  -3,  -2,  -4 },
        {  0,  -4,   1,   1,  -5,   1,  -3,   0 }
    };
    /// <summary>
    /// 難易度hard：終盤枚数重み
    /// </summary>
    private static readonly float endgameWeightFlip     = 7.97f;
    /// <summary>
    /// 難易度hard：終盤有効手数重み
    /// </summary>
    private static readonly float endgameWeightMobility = 0.60f;
    /// <summary>
    /// 難易度hard：終盤枚数差重み
    /// </summary>
    private static readonly float endgameWeightDiff     = 0.70f;

    /// <summary>
    /// ゲーム進行段階 (序盤/中盤/終盤)
    /// </summary>
    private enum Phase { Opening, Midgame, Endgame }

    /// <summary>
    /// 8方向探索用オフセット配列
    /// </summary>
    private static readonly (int dx,int dy)[] directions = {
        (1, 0), (-1, 0), (0, 1), (0, -1),
        (1, 1), (-1, -1), (1, -1), (-1, 1)
    };

    /// <summary>
    /// インスタンス設定を行う
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// AIの手番を難易度に応じて非同期で実行する
    /// </summary>
    public async UniTask PlayAITurn()
    {
        OthelloManager.isAIPlaying = true;
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.2));

        string[,] board = OthelloBoard.Instance.GetBoardState();
        string aiTag = OthelloManager.Instance.isAIWhite ? "White" : "Black";
        List<Vector2Int> validMoves = GetValidMoves(board, aiTag);
        if (validMoves.Count == 0)
        {
            OthelloManager.isAIPlaying = false;
            return;
        }

        Vector2Int aiMove = difficulty switch
        {
            "easy" => EasyAI(validMoves),
            "normal" => NormalAI(validMoves, aiTag),
            "hard" => HardAI(validMoves),
            "secret" => SecretAI(validMoves),
            _ => EasyAI(validMoves)
        };

        Vector3 pos = new Vector3(aiMove.x - 3.5f, aiMove.y - 3.5f, 0);
        await OthelloManager.Instance.PlacePiece(aiMove.x, aiMove.y, aiTag, pos);

        OthelloManager.isAIPlaying = false;
    }

    /// <summary>
    /// 難易度easy：合法手からランダムに選択する
    /// </summary>
    private Vector2Int EasyAI(List<Vector2Int> validMoves)
    {
        return validMoves[Random.Range(0, validMoves.Count)];
    }

    /// <summary>
    /// 難易度normal：フリップ枚数と位置スコアで最良手を選択する
    /// </summary>
    /// <param name="validMoves">合法手のリスト</param>
    /// <param name="aiTag">AIの駒色</param>
    private Vector2Int NormalAI(List<Vector2Int> validMoves, string aiTag)
    {
        int maxScore = int.MinValue;
        Vector2Int bestMove = validMoves[0];
        string[,] board = OthelloBoard.Instance.GetBoardState();

        foreach (var move in validMoves)
        {
            int flipCountScore = CountFlippablePieces(move.x, move.y, aiTag, board);
            int positionScore  = normalDifficultyTable[move.y, move.x];
            int totalScore     = flipCountScore * normalWeightFlip + positionScore * normalWeightPos;
            if (totalScore > maxScore)
            {
                maxScore = totalScore;
                bestMove = move;
            }
        }
        return bestMove;
    }

    /// <summary>
    /// 難易度hard：深さ2ミニマックスで最良手を選択する
    /// </summary>
    /// <param name="validMoves">合法手のリスト</param>
    private Vector2Int HardAI(List<Vector2Int> validMoves)
    {
        string aiTag     = OthelloManager.Instance.isAIWhite ? "White" : "Black";
        string playerTag = aiTag == "White" ? "Black" : "White";
        float maxScore   = float.MinValue;
        Vector2Int bestMove = validMoves[0];
        string[,] board = OthelloBoard.Instance.GetBoardState();

        foreach (var move in validMoves)
        {
            var boardAfterAIMove = CloneBoardState(board);
            int flipA = CountFlippablePieces(move.x, move.y, aiTag, boardAfterAIMove);
            SimulateMove(boardAfterAIMove, move.x, move.y, aiTag);
            List<Vector2Int> playerMoves = GetValidMoves(boardAfterAIMove, playerTag);
            float score;
            if (playerMoves.Count == 0)
            {
                score = EvaluateMove(move.x, move.y, flipA, boardAfterAIMove, aiTag);
            }
            else
            {
                float worst = float.MaxValue;
                foreach (var pm in playerMoves)
                {
                    var boardAfterPM = CloneBoardState(boardAfterAIMove);
                    int flipP = CountFlippablePieces(pm.x, pm.y, playerTag, boardAfterPM);
                    SimulateMove(boardAfterPM, pm.x, pm.y, playerTag);
                    float ps = -EvaluateMove(pm.x, pm.y, flipP, boardAfterPM, playerTag);
                    if (ps < worst) worst = ps;
                }
                score = worst;
            }
            if (score > maxScore)
            {
                maxScore = score;
                bestMove = move;
            }
        }
        return bestMove;
    }

    /// <summary>
    /// シークレット難易度：easyAIと同じランダム選択
    /// </summary>
    private Vector2Int SecretAI(List<Vector2Int> validMoves)
    {
        return EasyAI(validMoves);
    }

    /// <summary>
    /// 盤面位置でひっくり返せる駒の数をカウントする
    /// </summary>
    /// <param name="x">列インデックス (0-7)</param>
    /// <param name="y">行インデックス (0-7)</param>
    /// <param name="currentTag">現在のプレイヤー色 ("White"/"Black")</param>
    /// <param name="board">盤面状態配列</param>
    public int CountFlippablePieces(int x, int y, string currentTag, string[,] board)
    {
        int count = 0;
        foreach (var (dx, dy) in directions)
        {
            int cx = x + dx, cy = y + dy, temp = 0;
            while (OthelloBoard.Instance.IsValidPosition(cx, cy))
            {
                string state = board[cx, cy];
                if (state == null) break;
                if (state != currentTag) temp++;
                else { count += temp; break; }
                cx += dx; cy += dy;
            }
        }
        return count;
    }

    /// <summary>
    /// 盤面状態をディープコピーする
    /// </summary>
    /// <param name="original">コピー元の盤面状態配列</param>
    private string[,] CloneBoardState(string[,] original)
    {
        var clone = new string[8,8];
        for (int i=0;i<8;i++) for(int j=0;j<8;j++) clone[i,j] = original[i,j];
        return clone;
    }

    /// <summary>
    /// 指定盤面に仮手を実行し，駒をひっくり返す
    /// </summary>
    /// <param name="board">仮想盤面状態配列</param>
    /// <param name="x">置く列インデックス</param>
    /// <param name="y">置く行インデックス</param>
    /// <param name="tag">置く駒の色</param>
    private void SimulateMove(string[,] board, int x, int y, string tag)
    {
        board[x,y] = tag;
        foreach (var (dx,dy) in directions)
        {
            int cx=x+dx, cy=y+dy;
            var toFlip = new List<Vector2Int>();
            while (OthelloBoard.Instance.IsValidPosition(cx, cy))
            {
                string cur = board[cx,cy];
                if (cur==null) break;
                if (cur!=tag) toFlip.Add(new Vector2Int(cx,cy));
                else { foreach(var p in toFlip) board[p.x,p.y]=tag; break; }
                cx+=dx; cy+=dy;
            }
        }
    }

    /// <summary>
    /// 合法手を取得する
    /// </summary>
    /// <param name="board">盤面状態配列</param>
    /// <param name="tag">プレイヤー色 ("White"/"Black")</param>
    private List<Vector2Int> GetValidMoves(string[,] board, string tag)
    {
        var moves = new List<Vector2Int>();
        for(int x=0;x<8;x++) for(int y=0;y<8;y++)
            if(board[x,y]==null && OthelloBoard.Instance.IsValidMove(x,y,tag,board))
                moves.Add(new Vector2Int(x,y));
        return moves;
    }

    /// <summary>
    /// 手を評価し，その総合スコアを返す
    /// </summary>
    /// <param name="x">置く列インデックス</param>
    /// <param name="y">置く行インデックス</param>
    /// <param name="flipCount">ひっくり返せる枚数</param>
    /// <param name="board">仮想盤面状態配列</param>
    /// <param name="aiTag">AIの駒色</param>
    private float EvaluateMove(int x, int y, int flipCount, string[,] board, string aiTag)
    {
        Phase phase = GetPhase();
        int[,] table; float wf, wm, wd;
        switch(phase)
        {
            case Phase.Opening:
                table = openingTable; wf=openingWeightFlip; wm=openingWeightMobility; wd=openingWeightDiff; break;
            case Phase.Midgame:
                table = midgameTable; wf=midgameWeightFlip; wm=midgameWeightMobility; wd=midgameWeightDiff; break;
            case Phase.Endgame:
                table = endgameTable; wf=endgameWeightFlip; wm=endgameWeightMobility; wd=endgameWeightDiff; break;
            default:
                throw new System.ArgumentOutOfRangeException();
        }
        string oppTag = aiTag=="White"?"Black":"White";
        int myMob = GetValidMoves(board, aiTag).Count;
        int opMob = GetValidMoves(board, oppTag).Count;
        int myCnt = OthelloBoard.Instance.CountPieces(aiTag=="White");
        int opCnt = OthelloBoard.Instance.CountPieces(oppTag=="White");
        float idxScore = table[y,x];
        float flipScore = flipCount * wf;
        float mobScore = (myMob-opMob) * wm;
        float diffScore= (myCnt-opCnt) * wd;
        return idxScore + flipScore + mobScore + diffScore;
    }

    /// <summary>
    /// 現在のターン数から局面の何手目かを判定する
    /// </summary>
    private Phase GetPhase()
    {
        int turn = OthelloBoard.Instance.CountPieces(true) + OthelloBoard.Instance.CountPieces(false) - 4;
        if (turn < 20) return Phase.Opening;
        if (turn < 50) return Phase.Midgame;
        return Phase.Endgame;
    }
}
