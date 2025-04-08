using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class OthelloManager : MonoBehaviour
{
    public GameObject flipMarker; // 反転中に表示するマーク

    public static OthelloManager Instance;
    public static bool Waiting = false;
    public static bool initializing = false;
    private bool previousWaiting = false;

    private bool isWhiteTurn = false;
    public GameObject skipMessageWhite;
    public GameObject skipMessageBlack;
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;
    public Image whiteDigit1;
    public Image whiteDigit2;
    public Image blackDigit1;
    public Image blackDigit2;
    public Sprite[] numSprites;
    public Sprite whiteHintSprite; // 半透明の白スプライトをInspectorで設定
    public Sprite blackHintSprite; // 半透明の黒スプライトをInspectorで設
    private OthelloBoard board;

    private const int gridSize = 8; // 盤面サイズ (ハイライト等に使用)
    public GameObject gameover;
    private int gameoverCounter;
    private int blackPlacedCount = 0;
    private int whitePlacedCount = 0;

    public GameObject blackStockPrefab;
    public GameObject whiteStockPrefab;
    public Transform blackStockParent;
    public Transform whiteStockParent;

    public List<GameObject> blackStocks = new List<GameObject>();
    public List<GameObject> whiteStocks = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        Application.targetFrameRate = 60; // フレームレートを60FPSに固定
        board = GetComponent<OthelloBoard>();
    }

    private async UniTaskVoid Start()
    {
        GenerateStockPieces();
        await InitializeBoard();
    }

    public void UpdateScoreUI()
	{
		UpdateScore(CountPieces(true), whiteDigit1, whiteDigit2);
		UpdateScore(CountPieces(false), blackDigit1, blackDigit2);
        Debug.Log(CountPieces(true));
        Debug.Log(CountPieces(false));
	}

	void UpdateScore(int score, Image digit1, Image digit2)
	{
		int tens = score / 10;
		int ones = score % 10;

		digit1.sprite = numSprites[tens];
		digit2.sprite = numSprites[ones];
	}

    public void ShowFlipMarker(bool show)
    {
        if (flipMarker != null)
        {
            flipMarker.SetActive(show);
        }
    }

    // 初期配置
    private void GenerateStockPieces()
    {
        int columns = 14; // 1行の個数
        int rows = 4;     // 行数
        int total = columns * rows; // = 56個

        int spacingPxX = 2;
        int spacingPxY = 12 + 2; // 14pxのスプライト + 2pxの間隔
        float spacingX = spacingPxX / 16f; // PPU=16 → Unity単位に変換
        float spacingY = spacingPxY / 16f;

        Vector3 blackStartPos = blackStockParent.position;
        Vector3 whiteStartPos = whiteStockParent.position;

        for (int i = 0; i < total; i++)
        {
            int x = i % columns;
            int y = i / columns;

            // 黒コマの配置座標
            Vector3 blackPos = blackStartPos + new Vector3(x * spacingX, -y * spacingY, 0);
            GameObject black = Instantiate(blackStockPrefab, blackPos, Quaternion.identity, blackStockParent);
            blackStocks.Add(black);

            // 白コマの配置座標
            Vector3 whitePos = whiteStartPos + new Vector3(x * spacingX, -y * spacingY, 0);
            GameObject white = Instantiate(whiteStockPrefab, whitePos, Quaternion.identity, whiteStockParent);
            whiteStocks.Add(white);
        }
    }


    private async UniTask InitializeBoard()
    {
        initializing = true;
        float waitTime = 0.1f;

        await PlaceInitialPiece(3, 4, blackPiecePrefab, waitTime);
        await PlaceInitialPiece(3, 3, whitePiecePrefab, waitTime);
        await PlaceInitialPiece(4, 3, whitePiecePrefab, waitTime);
        await PlaceInitialPiece(4, 4, whitePiecePrefab, waitTime);

        initializing = false;
    }

    private async UniTask PlaceInitialPiece(int x, int y, GameObject prefab, float waitTime)
    {
        GameObject piece = Instantiate(prefab, new Vector3(x - 3.5f, y - 3.5f, 0), Quaternion.identity);
        await board.PlacePiece(x, y, piece, piece.tag);
        await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime));
    }

    // ターン情報
    public bool IsWhiteTurn() => isWhiteTurn;

    public OthelloBoard GetBoard() => board; // boardを取得する公開メソッド

    public void EndTurn() => isWhiteTurn = !isWhiteTurn;

    // 合法手の判定

    public void ConsumeStock(string tag)
    {
        int columns = 14;

        if (tag == "Black" && blackPlacedCount < blackStocks.Count)
        {
            GameObject stock = blackStocks[blackPlacedCount];
            stock.SetActive(false);
            blackPlacedCount++;
        }
        else if (tag == "White" && whitePlacedCount < whiteStocks.Count)
        {
            GameObject stock = whiteStocks[whitePlacedCount];
            stock.SetActive(false);
            whitePlacedCount++;
        }
    }
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

    private async UniTask ShowSkipMessage(bool isWhite)
    {
        if (skipMessageBlack != null && skipMessageWhite != null)
        {
            if (isWhite)
            {
                skipMessageBlack.SetActive(true);  // 表示
                await UniTask.Delay(System.TimeSpan.FromSeconds(2.0)); // 1.5秒待つ
                skipMessageBlack.SetActive(false); // 非表示
            }
            else
            {
                skipMessageWhite.SetActive(true);  // 表示
                await UniTask.Delay(System.TimeSpan.FromSeconds(2.0)); // 1.5秒待つ
                skipMessageWhite.SetActive(false); // 非表示
            }
        }
    }
    public async UniTask HighlightValidMoves()
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

        // ✅ 合法手セル：ハイライト表示（半透明スプライト）
        foreach (OthelloCell cell in validCells)
        {
            SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
            sr.sprite = isWhiteTurn ? whiteHintSprite : blackHintSprite;

            // 色を半透明に設定
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, isWhiteTurn ? 0.3f : 0.5f);
        }

        // ✅ 非合法手セル：透明にする
        foreach (OthelloCell cell in invalidCells)
        {
            SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.0f);
        }

        if (validCells.Count == 0)
        {
            gameoverCounter += 1;

            if (gameoverCounter == 1)
            {
                // 🧠 次のプレイヤーにも合法手がないなら、Skipも出さずに終了する
                isWhiteTurn = !isWhiteTurn;
                bool nextHasMove = false;

                foreach (OthelloCell cell in FindObjectsByType<OthelloCell>(FindObjectsSortMode.None))
                {
                    if (IsValidMove(cell.x, cell.y, isWhiteTurn ? "White" : "Black"))
                    {
                        nextHasMove = true;
                        break;
                    }
                }

                if (nextHasMove)
                {
                    // ✅ 次のプレイヤーが打てる → SKIP表示する
                    await ShowSkipMessage(isWhiteTurn);
                }
                else
                {
                    // ❌ 次も打てない → 2連続スキップになるのでSKIPは出さず即終了
                    gameoverCounter += 1;
                }
            }

            // ゲームオーバー処理は Update() などで拾う（gameoverCounter == 2）
        }
        else
        {
            gameoverCounter = 0;
        }
    }

    public int CountPieces(bool isWhite)
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
        if (isWhite)
        {
            return whiteCount;
        }
        else
        {
            return blackCount;
        }
    }

    // 合法手のハイライト表示
    // public void HighlightValidMoves()
    // {
    //     List<OthelloCell> validCells = new List<OthelloCell>();
    //     List<OthelloCell> invalidCells = new List<OthelloCell>();

    //     foreach (OthelloCell cell in FindObjectsByType<OthelloCell>(FindObjectsSortMode.None))
    //     {
    //         if (IsValidMove(cell.x, cell.y, isWhiteTurn ? "White" : "Black"))
    //         {
    //             validCells.Add(cell);
    //         }
    //         else
    //         {
    //             invalidCells.Add(cell);
    //         }
    //     }

    //     // 合法手 → 不透明
    //     foreach (OthelloCell cell in validCells)
    //     {
    //         SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
    //         sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1.0f);
    //     }

    //     // 非合法手 → 透明
    //     foreach (OthelloCell cell in invalidCells)
    //     {
    //         SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
    //         sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.0f);
    //     }
    // }

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
            _ = HighlightValidMoves(); // ターンごとにハイライト更新
            UpdateScoreUI();
        }
        previousWaiting = Waiting;

        if (gameoverCounter == 2)
        {
            Debug.Log("Gameover");
            gameover.SetActive(true);
        }

        // Debug.Log($"White: {whiteCount}, Black: {blackCount}, Turn: {(isWhiteTurn ? "White" : "Black")}");
    }
}
