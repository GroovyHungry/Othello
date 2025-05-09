using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Linq;
using System;

public class OthelloBoard : MonoBehaviour
{
    public static OthelloBoard Instance;
    public const int gridSize = 8;

    private static readonly (int dx, int dy)[] directions = {
        ( 1, 0), (-1, 0), ( 0, 1), ( 0,-1),
        ( 1, 1), (-1,-1), ( 1,-1), (-1, 1),
    };

    private string[,] boardState = new string[gridSize, gridSize];
    private GameObject[,] pieceObjects = new GameObject[gridSize, gridSize];

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public async UniTask ApplyMove(int x, int y, GameObject piece, string tag)
    {
        pieceObjects[x, y] = piece;
        boardState[x, y] = tag;
        await CheckAndFlipPieces(x, y, tag);
    }

    public bool IsCellEmpty(int x, int y) => boardState[x, y] == null;
    public bool IsValidPosition(int x, int y) => x >= 0 && x < gridSize && y >= 0 && y < gridSize;
    public string GetState(int x, int y) => boardState[x, y];
    public GameObject GetPiece(int x, int y) => pieceObjects[x, y];

    public string[,] GetBoardState()
    {
        var copy = new string[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                copy[x, y] = boardState[x, y];
            }
        }
        return copy;
    }

    public void ClearBoardState()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                boardState[x, y] = null;
                if (pieceObjects[x, y] != null)
                {
                    Destroy(pieceObjects[x, y]);
                    pieceObjects[x, y] = null;
                }
            }
        }
    }

    public bool IsValidMove(int x, int y, string currentTag, string[,] board = null)
    {
        board ??= boardState;
        if (board[x, y] != null) return false;

        foreach (var (dx, dy) in directions)
        {
            int checkX = x + dx;
            int checkY = y + dy;
            bool foundOpponent = false;
            while (IsValidPosition(checkX, checkY))
            {
                string piece = board[checkX, checkY];
                if (piece == null) break;
                if (piece != currentTag)
                {
                    foundOpponent = true;
                }
                else
                {
                    if (foundOpponent) return true;
                    break;
                }
                checkX += dx;
                checkY += dy;
            }
        }
        return false;
    }

    private async UniTask CheckAndFlipPieces(int x, int y, string currentTag)
    {
        List<List<Vector2Int>> byDir = new List<List<Vector2Int>>();

        foreach (var (dx, dy) in directions)
        {
            var list = GetFlippablePieces(x, y, dx, dy, currentTag);
            if (list.Count > 0) byDir.Add(list);
        }
        if (byDir.Count == 0) return;

        await FlipPieces(byDir, currentTag);
    }

    private async UniTask FlipPieces(List<List<Vector2Int>> byDir, string currentTag)
    {
        int maxLayer = byDir.Max(list => list.Count);
        float startDelay = 0.1f;
        float acceleration = 0.05f;

        AkSoundEngine.PostEvent("FlipPiece", gameObject);
        for (int layer = 0; layer < maxLayer; layer++)
        {
            foreach (var list in byDir)
            {
                if (layer < list.Count)
                {
                    var pos = list[layer];
                    boardState[pos.x, pos.y] = currentTag;
                    pieceObjects[pos.x, pos.y].GetComponent<OthelloPiece>().Flip().Forget();
                }
            }

            float delay = Math.Max(0.01f, startDelay - acceleration * layer);
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
        }
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
    }

    private List<Vector2Int> GetFlippablePieces(int x, int y, int dx, int dy, string currentTag)
    {
        List<Vector2Int> flippablePieces = new List<Vector2Int>();
        int checkX = x + dx;
        int checkY = y + dy;
        bool foundOpponent = false;

        while (IsValidPosition(checkX, checkY))
        {
            string checkPiece = boardState[checkX, checkY];

            if (checkPiece == null)
            {
                return new List<Vector2Int>();
            }

            if (checkPiece != currentTag)
            {
                flippablePieces.Add(new Vector2Int(checkX, checkY));
                foundOpponent = true;
            }
            else
            {
                return foundOpponent ? flippablePieces : new List<Vector2Int>();
            }
            checkX += dx;
            checkY += dy;
        }
        return new List<Vector2Int>();
    }
}
