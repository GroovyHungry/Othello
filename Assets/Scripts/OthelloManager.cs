using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OthelloManager : MonoBehaviour
{
    public GameObject flipMarker; // 反転中に表示するマーク

    public static OthelloManager Instance;
    public static bool Waiting = false;
    public static bool initializing = false;

    private bool isWhiteTurn = false;
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;
    public Sprite Highlighting;
    private OthelloBoard board;

    private const int gridSize = 8; // 盤面サイズ (ハイライト等に使用)

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        board = GetComponent<OthelloBoard>(); // 同じGameObjectにアタッチされたOthelloBoard参照
        StartCoroutine(InitializeBoard());
    }

    public void ShowFlipMarker(bool show)
    {
        if (flipMarker != null)
        {
            flipMarker.SetActive(show);
        }
    }

    // 初期配置
    private IEnumerator InitializeBoard()
    {
        initializing = true;
        float waitTime = 0.1f;

        yield return PlaceInitialPiece(3, 4, blackPiecePrefab, waitTime);
        yield return PlaceInitialPiece(3, 3, whitePiecePrefab, waitTime);
        yield return PlaceInitialPiece(4, 3, blackPiecePrefab, waitTime);
        yield return PlaceInitialPiece(4, 4, whitePiecePrefab, waitTime);

        initializing = false;
    }

    private IEnumerator PlaceInitialPiece(int x, int y, GameObject prefab, float waitTime)
    {
        GameObject piece = Instantiate(prefab, new Vector3(x - 3.5f, y - 3.5f, 0), Quaternion.identity);
        board.PlacePiece(x, y, piece, piece.tag);
        yield return new WaitForSeconds(waitTime);
    }

    // ターン情報
    public bool IsWhiteTurn() => isWhiteTurn;

    public OthelloBoard GetBoard() => board; // boardを取得する公開メソッド

    public void EndTurn() => isWhiteTurn = !isWhiteTurn;

    // 合法手の判定
    public bool IsValidMove(int x, int y, string currentTag)
    {
        if (!board.IsCellEmpty(x, y)) return false;

        int[,] directions = {
            { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 },
            { 1, 1 }, { -1, -1 }, { 1, -1 }, { -1, 1 }
        };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int dx = directions[i, 0];
            int dy = directions[i, 1];

            if (CanFlipDirection(x, y, dx, dy, currentTag))
            {
                return true;
            }
        }
        return false;
    }

    // 指定方向で挟めるかチェック
    private bool CanFlipDirection(int x, int y, int dx, int dy, string currentTag)
    {
        int checkX = x + dx;
        int checkY = y + dy;
        bool foundOpponent = false;

        while (IsValidPosition(checkX, checkY))
        {
            GameObject checkPiece = board.GetPiece(checkX, checkY);
            if (checkPiece == null) return false;

            if (checkPiece.tag != currentTag)
            {
                foundOpponent = true;
            }
            else
            {
                return foundOpponent;
            }
            checkX += dx;
            checkY += dy;
        }
        return false;
    }

    // 盤面の範囲チェック
    private bool IsValidPosition(int x, int y) => x >= 0 && x < gridSize && y >= 0 && y < gridSize;

    // 合法手のハイライト表示
    public void HighlightValidMoves()
    {
        List<OthelloCell> validCells = new List<OthelloCell>();
        List<OthelloCell> invalidCells = new List<OthelloCell>();

        foreach (OthelloCell cell in FindObjectsByType<OthelloCell>(FindObjectsSortMode.None))
        {
            if (IsValidMove(cell.x, cell.y, isWhiteTurn ? "White" : "Black"))
            {
                validCells.Add(cell);
            }
            else
            {
                invalidCells.Add(cell);
            }
        }

        // 合法手 → 不透明
        foreach (OthelloCell cell in validCells)
        {
            SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1.0f);
        }

        // 非合法手 → 透明
        foreach (OthelloCell cell in invalidCells)
        {
            SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.0f);
        }
    }

    // ターンごとの状況管理（駒数カウント）
    private void Update()
    {
        int whiteCount = 0;
        int blackCount = 0;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject piece = board.GetPiece(x, y);
                if (piece != null)
                {
                    if (piece.tag == "White") whiteCount++;
                    else if (piece.tag == "Black") blackCount++;
                }
            }
        }

        if (!initializing && !Waiting)
        {
            HighlightValidMoves(); // ターンごとにハイライト更新
        }

        // Debug.Log($"White: {whiteCount}, Black: {blackCount}, Turn: {(isWhiteTurn ? "White" : "Black")}");
    }
}
