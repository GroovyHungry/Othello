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
    private static readonly int[,] hardDifficultyTable = new int[8,8]
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
    const int weightFlip = 10;
    const int weightPos = 1;

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
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.4f));
        GameObject[,] board = OthelloBoard.Instance.GetBoardState();
        string aiTag = OthelloManager.Instance.IsAIWhite() ? "White" :"Black";


        List<Vector2Int> validMoves = GetValidMoves(board, aiTag);
        Debug.Log("Valid Moves: " + validMoves.Count);
        if (validMoves.Count == 0)
        {
            OthelloManager.isAIPlaying = false;
            return;
        }

        Vector2Int aiMove = Vector2Int.zero;
        if (difficulty == "easy")
        {
            aiMove = EasyAI(validMoves);
        }
        else if (difficulty == "normal")
        {
            aiMove = NormalAI(validMoves, aiTag);
        }
        else if (difficulty == "hard")
        {
            aiMove = HardAI(validMoves);
        }
        else if (difficulty == "secret")
        {
            aiMove = SecretAI(validMoves);
        }

        Vector3 pos = new Vector3(aiMove.x - 3.5f, aiMove.y - 3.5f, 0);
        await OthelloManager.Instance.PlacePieces(aiMove.x, aiMove.y, aiTag, pos);

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

        foreach (var move in validMoves)
        {
            int flipCountScore = CountFlippablePieces(move.x, move.y, aiTag);
            int positionScore = normalDifficultyTable[move.y, move.x];
            int totalScore = flipCountScore * weightFlip + positionScore * weightPos;

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
        GameObject[,] board = OthelloBoard.Instance.GetBoardState();

        foreach (var move in validMoves)
        {
            var boardAfteraiMove = CloneBoardState(board);
            SimulateMove(boardAfteraiMove, move.x, move.y, aiTag);

            List<Vector2Int> playerMoves = GetValidMoves(boardAfteraiMove, playerTag);

            int worstScore = int.MaxValue;
            foreach (var playerMove in playerMoves)
            {
                GameObject[,] boardAfterPlayer = CloneBoardState(boardAfteraiMove);
                SimulateMove(boardAfterPlayer, playerMove.x, playerMove.y, playerTag);

                int score = EvaluateBoard(boardAfterPlayer, aiTag);
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
        return bestMove;
    }
    private Vector2Int SecretAI(List<Vector2Int> validMoves)
    {
        return validMoves[Random.Range(0, validMoves.Count)];
    }
    public int CountFlippablePieces(int x, int y, string currentTag)
    {
        int count = 0;

        foreach (var (dx, dy) in directions)
        {
            int checkX = x + dx;
            int checkY = y + dy;
            int tempCount = 0;

            while (OthelloBoard.Instance.IsValidPosition(checkX, checkY))
            {
                GameObject piece = OthelloBoard.Instance.GetPiece(checkX, checkY);
                if (piece == null)
                {
                    break;
                }
                else if (piece.tag != currentTag)
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
    private GameObject[,] CloneBoardState(GameObject[,] original)
    {
        var clone = new string[8, 8];

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                clone[x, y] = original[x, y] != null ? original[x, y].tag : null;
            }
        }
        return clone;
    }
    private void SimulateMove(GameObject[,] board, int x, int y, string tag)
    {
        board[x, y] = tag;

        foreach (var (dx, dy) in directions)
        {
            int checkX = x + dx;
            int checkY = y + dy;
            var toFlip = new List<int, int>();

            while (OthelloBoard.Instance.IsValidPosition(checkX, checkY))
            {
                string current = board[checkX, checkY];

                if (current == null)
                {
                    toFlip.Clear();
                    break;
                }
                else if (current != tag)
                {
                    toFlip.Add((checkX, checkY));
                }
                else
                {
                    foreach (var (cx, cy) in toFlip)
                    {
                        board[cx, cy] = tag;
                    }
                    break;
                }

                checkX += dx;
                checkY += dy;
            }
        }
    }
    private List<Vector2Int> GetValidMoves(GameObject[,] board, string tag)
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
    private int EvaluateBoard(GameObject[,] board, string aiTag)
    {
        int score = 0;
        string playerTag = aiTag == "White" ? "Black" : "White";

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = board[x, y];
                if (piece == null) continue;
                if (piece.tag == aiTag)
                {
                    score += hardDifficultyTable[x, y];
                }
                else if (piece.tag == playerTag)
                {
                    score -= hardDifficultyTable[x, y];
                }
            }
        }
        return score;
    }
}
