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
        { 30, -12, 0, -1, -1,  0, -12, 30 },
        { -12, -15, -3, -3, -3, -3, -15, -12 },
        { 0, -3,  0, -1, -1,  0, -3,  0 },
        { -1, -3, -1, -1, -1, -1, -3, -1 },
        { -1, -3, -1, -1, -1, -1, -3, -1 },
        { 0, -3,  0, -1, -1,  0, -3,  0 },
        { -12, -15, -3, -3, -3, -3, -15, -12 },
        { 30, -12, 0, -1, -1, 0, -12, 30 }
    };

    private static readonly int[,] midgameTable = new int[8,8]
    {
        { 20, -3, 11, 8, 8, 11, -3, 20 },
        { -3, -7, -4, 1, 1, -4, -7, -3 },
        { 11, -4, 2, 2, 2, 2, -4, 11 },
        { 8, 1, 2, -3, -3, 2, 1, 8 },
        { 8, 1, 2, -3, -3, 2, 1, 8 },
        { 11, -4, 2, 2, 2, 2, -4, 11 },
        { -3, -7, -4, 1, 1, -4, -7, -3 },
        { 20, -3, 11, 8, 8, 11, -3, 20 }
    };
    private static readonly int[,] endgameTable = new int[8,8];
    const int openingFlipWeight = 3;
    const int midgameFlipWeight = 10;
    const int endgameFlipWeight = 12;
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
        int maxScore = int.MinValue;
        Vector2Int bestMove = validMoves[0];
        string[,] board = OthelloBoard.Instance.GetBoardState();
        int turn = OthelloBoard.Instance.CountPieces(true) + OthelloBoard.Instance.CountPieces(false);

        int[,] table = turn < 20 ? openingTable : turn < 50 ? midgameTable : endgameTable;
        int flipWeight = turn < 20 ? openingFlipWeight : turn < 50 ? midgameFlipWeight : endgameFlipWeight;

        foreach (var move in validMoves)
        {
            var boardAfterAIMove = CloneBoardState(board);
            int flipCountScoreAI = CountFlippablePieces(move.x, move.y, aiTag, boardAfterAIMove);
            SimulateMove(boardAfterAIMove, move.x, move.y, aiTag);

            List<Vector2Int> playerMoves = GetValidMoves(boardAfterAIMove, playerTag);
            if (playerMoves.Count == 0)
            {
                int score = EvaluateBoard(boardAfterAIMove, aiTag, table) + flipCountScoreAI * flipWeight;
                if (score > maxScore)
                {
                    maxScore = score;
                    bestMove = move;
                }
                continue;
            }
            int worstScore = int.MaxValue;
            foreach (var playerMove in playerMoves)
            {
                string[,] boardAfterPlayer = CloneBoardState(boardAfterAIMove);
                int flipCountScorePlayer = CountFlippablePieces(playerMove.x, playerMove.y, playerTag, boardAfterPlayer);
                SimulateMove(boardAfterPlayer, playerMove.x, playerMove.y, playerTag);

                int score = EvaluateBoard(boardAfterPlayer, aiTag, table) + (flipCountScoreAI - flipCountScorePlayer) * flipWeight;
                Debug.Log($"Move: {move}, Player Move: {playerMove}, Score: {score}");
                if (score < worstScore)
                {
                    worstScore = score;
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
    private int EvaluateBoard(string[,] board, string aiTag, int[,] table)
    {
        int score = 0;
        string playerTag = aiTag == "White" ? "Black" : "White";

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                string piece = board[x, y];
                if (piece == null) continue;
                if (piece == aiTag)
                {
                    score += table[y, x];
                }
                else if (piece == playerTag)
                {
                    score -= table[y, x];
                }
            }
        }
        return score;
    }
}
