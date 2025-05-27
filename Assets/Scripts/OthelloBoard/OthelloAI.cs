using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class OthelloAI : MonoBehaviour
{
    public static OthelloAI Instance;
    public string difficulty = "easy";
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
    const int normalWeightFlip = 10;
    const int normalWeightPos = 1;
    private static readonly int[,] openingTable = new int[8,8]
    {
        {  28,  -9,  -1,   3,   1,  -3,  -9,  28 },
        { -14, -18,   0,  -4,   1,   0, -16, -14 },
        {   0,   0,  -5,  -1,   4,   0,  -5,   3 },
        {   2,  -5,   2,   1,  -1,   3,   0,  -1 },
        {   0,  -4,   2,  -1,  -2,   0,   0,  -2 },
        {   1,  -4,  -2,   1,  -2,  -3,  -5,   4 },
        { -12, -18,  -6,  -2,  -4,  -1, -14, -11 },
        {  30, -11,   3,  -5,   3,  -4, -11,  32 },
    };
    private static readonly float openingWeightFlip     =  8.27f;
    private static readonly float openingWeightMobility =  1.00f;
    private static readonly float openingWeightDiff     =  0.22f;

    private static readonly int[,] midgameTable = new int[8,8]
    {
        { 23,  -4,  11,   5,  11,  10,  -4,  21 },
        { -6,  -5,  -7,   1,   4,  -3, -10,  -2 },
        {  7,  -4,   5,   6,  -1,   5,  -8,  11 },
        {  9,   4,   6,  -1,  -1,   1,  -2,   8 },
        {  6,  -2,  -1,  -2,   0,   0,  -2,  10 },
        { 12,  -7,   2,   0,   1,   1,  -4,  11 },
        { -4,  -3,  -2,   2,   4,  -3,  -6,  -7 },
        { 17,  -3,  14,  10,  10,  13,  -3,  22 },
    };
    private static readonly float midgameWeightFlip     =  8.99f;
    private static readonly float midgameWeightMobility =  0.48f;
    private static readonly float midgameWeightDiff     =  0.45f;

    private static readonly int[,] endgameTable = new int[8,8]
    {
        {  2,   3,   2,   0,   2,   5,   4,  -2 },
        {  3,   3,  -2,   2,   1,  -3,  -3,   2 },
        {  0,   0,  -1,   4,  -2,   2,  -1,  -2 },
        { -3,  -1,  -2,  -1,   5,   2,  -1,  -2 },
        { -1,  -1,   3,   0,  -3,   0,   2,   4 },
        { -1,   3,  -3,  -1,   0,  -1,   0,   1 },
        {  0,   4,   4,  -1,  -2,  -3,  -2,  -4 },
        {  0,  -4,   1,   1,  -5,   1,  -3,   0 },
    };
    private static readonly float endgameWeightFlip     =  7.97f;
    private static readonly float endgameWeightMobility =  0.60f;
    private static readonly float endgameWeightDiff     =  0.70f;
    private enum Phase {Opening, Midgame, Endgame}
    private static readonly (int dx,int dy)[] directions = {
    ( 1, 0),(-1, 0),( 0, 1),( 0,-1),
    ( 1, 1),(-1,-1),( 1,-1),(-1, 1),};
    private void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}
    public async UniTask PlayAITurn()
    {
        OthelloManager.isAIPlaying = true;
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.2f));
        string[,] board = OthelloBoard.Instance.GetBoardState();
        string aiTag = OthelloManager.Instance.IsAIWhite() ? "White" :"Black";


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
    private Vector2Int EasyAI(List<Vector2Int> validMoves)
    {
        return validMoves[Random.Range(0, validMoves.Count)];
    }
    private Vector2Int NormalAI(List<Vector2Int> validMoves, string aiTag)
    {
        int maxScore = int.MinValue;
        Vector2Int bestMove = validMoves[0];
        string[,] board = OthelloBoard.Instance.GetBoardState();

        foreach (var move in validMoves)
        {
            int flipCountScore = CountFlippablePieces(move.x, move.y, aiTag, board);
            int positionScore = normalDifficultyTable[move.y, move.x];
            int totalScore = flipCountScore * normalWeightFlip + positionScore * normalWeightPos;

            if (totalScore > maxScore)
            {
                maxScore = totalScore;
                bestMove = move;
            }
        }
        return bestMove;
    }
    private Vector2Int HardAI(List<Vector2Int> validMoves)
    {
        string aiTag = OthelloManager.Instance.isAIWhite ? "White" : "Black";
        string playerTag = aiTag == "White" ? "Black" : "White";
        float maxScore = float.MinValue;
        Vector2Int bestMove = validMoves[0];
        string[,] board = OthelloBoard.Instance.GetBoardState();
        foreach (var move in validMoves)
        {
            var boardAfterAIMove = CloneBoardState(board);
            int flipCountScoreAI = CountFlippablePieces(move.x, move.y, aiTag, boardAfterAIMove);
            SimulateMove(boardAfterAIMove, move.x, move.y, aiTag);

            List<Vector2Int> playerMoves = GetValidMoves(boardAfterAIMove, playerTag);
            if (playerMoves.Count == 0)
            {
                float aiScore = EvaluateMove(move.x, move.y, flipCountScoreAI, boardAfterAIMove, aiTag);
                if (aiScore > maxScore)
                {
                    maxScore = aiScore;
                    bestMove = move;
                }
                continue;
            }
            float worstScore = float.MaxValue;
            foreach (var playerMove in playerMoves)
            {
                string[,] boardAfterPlayer = CloneBoardState(boardAfterAIMove);
                int flipCountScorePlayer = CountFlippablePieces(playerMove.x, playerMove.y, playerTag, boardAfterPlayer);
                SimulateMove(boardAfterPlayer, playerMove.x, playerMove.y, playerTag);

                float playerScore = -EvaluateMove(playerMove.x, playerMove.y, flipCountScorePlayer, boardAfterPlayer, playerTag);
                if (playerScore < worstScore)
                {
                    worstScore = playerScore;
                }
            }
            if (worstScore > maxScore)
            {
                maxScore = worstScore;
                bestMove = move;
            }
        }
        Debug.Log($"Best Move: {bestMove}, Score: {maxScore}");
        return bestMove;
    }
    private Vector2Int SecretAI(List<Vector2Int> validMoves)
    {
        return validMoves[Random.Range(0, validMoves.Count)];
    }
    public int CountFlippablePieces(int x, int y, string currentTag, string[,] board)
    {
        int count = 0;

        foreach (var (dx, dy) in directions)
        {
            int checkX = x + dx;
            int checkY = y + dy;
            int tempCount = 0;

            while (OthelloBoard.Instance.IsValidPosition(checkX, checkY))
            {
                string state = board[checkX, checkY];
                if (state == null)
                {
                    break;
                }
                else if (state != currentTag)
                {
                    tempCount++;
                }
                else
                {
                    count += tempCount;
                    break;
                }

                checkX += dx;
                checkY += dy;
            }
        }
        return count;
    }
    private string[,] CloneBoardState(string[,] original)
    {
        string[,] clone = new string[8, 8];

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                clone[x, y] = original[x, y];
            }
        }
        return clone;
    }
    private void SimulateMove(string[,] board, int x, int y, string tag)
    {
        board[x, y] = tag;

        foreach (var (dx, dy) in directions)
        {
            int checkX = x + dx;
            int checkY = y + dy;
            List<Vector2Int> toFlip = new List<Vector2Int>();

            while (OthelloBoard.Instance.IsValidPosition(checkX, checkY))
            {
                string current = board[checkX, checkY];

                if (current == null)
                {
                    break;
                }
                else if (current != tag)
                {
                    toFlip.Add(new Vector2Int(checkX, checkY));
                }
                else
                {
                    foreach (var pos in toFlip)
                    {
                        board[pos.x, pos.y] = tag;
                    }
                    break;
                }

                checkX += dx;
                checkY += dy;
            }
        }
    }
    private List<Vector2Int> GetValidMoves(string[,] board, string tag)
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if(board[x, y] != null) continue;
                if(OthelloBoard.Instance.IsValidMove(x, y, tag, board))
                {
                    validMoves.Add(new Vector2Int(x, y));
                }
            }
        }
        return validMoves;
    }
    private float EvaluateMove(int x, int y, int flipCount, string[,] board, string aiTag)
    {
        Phase phase = GetPhase();
        int[,] table;
        float wFlip, wMob, wDiff;
        switch (phase)
        {
            case Phase.Opening:
                table = openingTable;
                wFlip = openingWeightFlip;
                wMob = openingWeightMobility;
                wDiff = openingWeightDiff;
                break;
            case Phase.Midgame:
                table = midgameTable;
                wFlip = midgameWeightFlip;
                wMob = midgameWeightMobility;
                wDiff = midgameWeightDiff;
                break;
            case Phase.Endgame:
                table = endgameTable;
                wFlip = endgameWeightFlip;
                wMob = endgameWeightMobility;
                wDiff = endgameWeightDiff;
                break;
                default:
                // 想定外のフェーズが来たら例外を投げて検知できるように
                throw new System.ArgumentOutOfRangeException(nameof(phase), phase, "Unknown phase");
        }
        string playerTag = aiTag == "White" ? "Black" : "White";
        int myMobility = GetValidMoves(board, aiTag).Count;
        int opponentMobility = GetValidMoves(board, playerTag).Count;

        int myCount = OthelloBoard.Instance.CountPieces(aiTag == "White");
        int opponentCount = OthelloBoard.Instance.CountPieces(playerTag == "White");

        float idxScore = table[y, x];
        float flipScore = flipCount * wFlip;
        float mobilityScore = (myMobility - opponentMobility) * wMob;
        float diffScore = (myCount - opponentCount) * wDiff;
        return idxScore + flipScore + mobilityScore + diffScore;
    }
    private Phase GetPhase()
    {
        int turn = OthelloBoard.Instance.CountPieces(true) + OthelloBoard.Instance.CountPieces(false) - 4;
        if (turn < 20) return Phase.Opening;
        if (turn < 50) return Phase.Midgame;
        return Phase.Endgame;
    }
}
