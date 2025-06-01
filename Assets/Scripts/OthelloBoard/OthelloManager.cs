using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using AK.Wwise;

/// <summary>
/// オセロゲームを管理するクラス
/// </summary>
public class OthelloManager : MonoBehaviour
{
    /// <summary>
    /// グローバルアクセス用インスタンス
    /// </summary>
    public static OthelloManager Instance;

    /// <summary>
    /// 盤面サイズ
    /// </summary>
    private const int gridSize = 8;

    /// <summary>
    /// Inspector 割り当て用GameObject, UI, イベント
    /// </summary>
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;
    public GameObject youWhite;
    public GameObject youBlack;
    public GameObject cpuWhite;
    public GameObject cpuBlack;
    public GameObject player1;
    public GameObject player2;
    public GameObject skipMessageWhite;
    public GameObject skipMessageBlack;
    public GameObject blackStockPrefab;
    public GameObject whiteStockPrefab;
    public Transform blackStockParent;
    public Transform whiteStockParent;
    public List<GameObject> blackStocks = new List<GameObject>();
    public List<GameObject> whiteStocks = new List<GameObject>();
    public Button settingButtonInGame;
    public Button exitButton;
    public Sprite whiteHintSprite;
    public Sprite blackHintSprite;

    // public Image whiteDigit1;
    // public Image whiteDigit2;
    // public Image blackDigit1;
    // public Image blackDigit2;
    // public Sprite[] numSprites;

    /// <summary>
    /// Wwiseイベント
    /// </summary>
    [SerializeField] private AK.Wwise.Event OnClick;
    [SerializeField] private AK.Wwise.Event Place;
    [SerializeField] private AK.Wwise.Event Stock;
    [SerializeField] private AK.Wwise.Event Skip;

    /// <summary>
    /// 内部状態（ターン管理やプレイ状況管理）
    /// </summary>
    public static bool Waiting = false;
    public static bool initializing = false;
    public static bool isWhiteTurn = false;
    public static bool isAIPlaying = false;
    public static bool isAIOpponent = true;
    public bool isWhiteFirst = false;
    public bool isAIWhite = true;
    private OthelloBoard othelloBoard;
    private int gameoverCounter;
    private int blackPlacedCount = 0;
    private int whitePlacedCount = 0;

    /// <summary>
    /// インスタンスを設定
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Application.targetFrameRate = 60;
        othelloBoard = GetComponent<OthelloBoard>();

        settingButtonInGame.onClick.AddListener(OnSettingButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    /// <summary>
    /// リスナー解除
    /// </summary>
    private void OnDestroy()
    {
        exitButton.onClick.RemoveListener(OnExitButtonClicked);
        settingButtonInGame.onClick.RemoveListener(OnSettingButtonClicked);
    }

    /// <summary>
    /// ゲーム開始
    /// </summary>
    private async UniTaskVoid Start()
    {
        await StartGame();
    }

    /// <summary>
    /// 毎フレームスコア更新
    /// 現在は未使用
    /// </summary>
    private void Update()
    {
        // if (!initializing && !Waiting)
        // {
        //     UpdateScoreUI();
        // }
    }

    /// <summary>
    /// ゲーム開始処理
    /// </summary>
    public async UniTask StartGame()
    {
        initializing = true;

        if (isAIOpponent)
        {
            ///難易度選択
            await DifficultySelect.Instance.StartDifficultySelect();
            ///コイントスを行う
            await CoinTossManager.Instance.StartCoinTossVsCPU();
            ShowYouAndCPUUI();
        }
        else
        {
            ///PvPであれば難易度選択を飛ばす
            await CoinTossManager.Instance.StartCoinTossPvP();
            ShowP1AndP2();
        }

        isWhiteFirst = isWhiteTurn;
        ///駒のストックを生成
        await GenerateStockPieces();
        ///盤面の初期設定
        await InitializeBoard();
        ///有効手ハイライト
        HighlightValidMoves();

        initializing = false;
        bool isAITurn = (isWhiteTurn && isAIWhite) || (!isWhiteTurn && !isAIWhite);
        if (isAIOpponent && isAITurn)
        {
            await OthelloAI.Instance.PlayAITurn();
        }
    }

    /// <summary>
    /// メインメニューへ戻る処理
    /// </summary>
    public async UniTask ExitToMainMenu()
    {
        await SceneTransition.Instance.Transition("MainMenu");
        othelloBoard.ClearBoardState();
        ClearHighlightedCells();
    }

    /// <summary>
    /// Exitボタン押下時処理
    /// </summary>
    private void OnExitButtonClicked()
    {
        Waiting = true;
        DoubleCheck.Instance.OpenDoubleCheckPanel();
        OnClick.Post(exitButton.gameObject);
    }

    /// <summary>
    /// Settingボタン押下時処理
    /// </summary>
    private void OnSettingButtonClicked()
    {
        Waiting = true;
        SettingManager.Instance.OpenSettingPanel();
        settingButtonInGame.interactable = false;
        OnClick.Post(settingButtonInGame.gameObject);
    }

    // /// <summary>
    // /// スコアUI更新
    // /// 未使用状態
    // /// </summary>
    // public void UpdateScoreUI()
    // {
    //     UpdateScore(othelloBoard.CountPieces(true), whiteDigit1, whiteDigit2);
    //     UpdateScore(othelloBoard.CountPieces(false), blackDigit1, blackDigit2);
    // }

    // /// <summary>
    // /// スコア更新補助
    // /// 現在未使用
    // /// </summary>
    // private void UpdateScore(int score, Image digit1, Image digit2)
    // {
    //     int tens = score / 10;
    //     int ones = score % 10;
    //     digit1.sprite = numSprites[tens];
    //     digit2.sprite = numSprites[ones];
    // }

    /// <summary>
    /// プレイヤーとCPUを表示
    /// </summary>
    private void ShowYouAndCPUUI()
    {
        if (isAIWhite)
        {
            cpuWhite.SetActive(true);
            youBlack.SetActive(true);
        }
        else
        {
            cpuBlack.SetActive(true);
            youWhite.SetActive(true);
        }
    }

    /// <summary>
    /// P1/P2の表示
    /// </summary>
    private void ShowP1AndP2()
    {
        player1.SetActive(true);
        player2.SetActive(true);
    }

    /// <summary>
    /// コマ配置処理
    /// </summary>
    /// <param name="x">配置する列インデックス</param>
    /// <param name="y">配置する行インデックス</param>
    /// <param name="tag">駒の色タグ ("White" または "Black")</param>
    /// <param name="position">配置位置のワールド座標</param>
    public async UniTask PlacePiece(int x, int y, string tag, Vector3 position)
    {
        Waiting = true;
        ClearHighlightedCells();
        await ConsumeStock(tag);
        GameObject prefab = (tag == "White") ? whitePiecePrefab : blackPiecePrefab;
        GameObject piece = Instantiate(prefab, position, Quaternion.identity);
        piece.GetComponent<OthelloPiece>().InitState(x, y);
        piece.tag = tag;
        Place.Post(piece);
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.15f));
        await othelloBoard.ApplyMove(x, y, piece, tag);
        Waiting = false;
        await EndTurn();
    }

    /// <summary>
    /// ストック駒生成
    /// </summary>
    private async UniTask GenerateStockPieces()
    {
        Waiting = true;
        int columns = 14, rows = 4, total = columns * rows;
        float spacingX = 2f / 16f, spacingY = 14f / 16f;
        Vector3 blackStart = blackStockParent.position;
        Vector3 whiteStart = whiteStockParent.position;
        for (int i = 0; i < total; i++)
        {
            int x = i % columns, y = i / columns;
            Vector3 blackPos = blackStart + new Vector3(x * spacingX, -y * spacingY, 0);
            Vector3 whitePos = whiteStart + new Vector3(x * spacingX, -y * spacingY, 0);
            GameObject black = Instantiate(blackStockPrefab, blackPos, Quaternion.identity, blackStockParent);
            blackStocks.Add(black);
            GameObject white = Instantiate(whiteStockPrefab, whitePos, Quaternion.identity, whiteStockParent);
            whiteStocks.Add(white);
            Stock.Post(white);
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.0001f));
        }
        Waiting = false;
    }

    /// <summary>
    /// 初期コマ配置
    /// </summary>
    private async UniTask InitializeBoard()
    {
        float waitTime = 0.1f;
        await PlaceInitialPiece(3, 4, blackPiecePrefab);
        await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime));
        await PlaceInitialPiece(3, 3, whitePiecePrefab);
        await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime));
        await PlaceInitialPiece(4, 3, blackPiecePrefab);
        await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime));
        await PlaceInitialPiece(4, 4, whitePiecePrefab);
    }

    /// <summary>
    /// 初期コマ配置補助
    /// </summary>
    /// <param name="x">列インデックス</param>
    /// <param name="y">行インデックス</param>
    /// <param name="prefab">配置する駒のプレハブ</param>
    private async UniTask PlaceInitialPiece(int x, int y, GameObject prefab)
    {
        GameObject piece = Instantiate(prefab, new Vector3(x - 3.5f, y - 3.5f, 0), Quaternion.identity);
        piece.GetComponent<OthelloPiece>().InitState(x, y);
        Place.Post(piece);
        await othelloBoard.ApplyMove(x, y, piece, piece.tag);
    }

    /// <summary>
    /// ターン終了処理
    /// </summary>
    public async UniTask EndTurn()
    {
        isWhiteTurn = !isWhiteTurn;
        HighlightValidMoves();
        bool isAITurn = (isWhiteTurn && isAIWhite) || (!isWhiteTurn && !isAIWhite);
        if (isAIOpponent && isAITurn) await OthelloAI.Instance.PlayAITurn();
    }

    /// <summary>
    /// 駒ストック消費
    /// </summary>
    /// <param name="tag">消費する駒の色タグ</param>
    public async UniTask ConsumeStock(string tag)
    {
        if (tag == "Black" && blackPlacedCount < blackStocks.Count)
        {
            var stock = blackStocks[blackPlacedCount++];
            stock.GetComponent<Animator>().SetTrigger("consume");
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.2f));
            stock.SetActive(false);
        }
        else if (tag == "White" && whitePlacedCount < whiteStocks.Count)
        {
            var stock = whiteStocks[whitePlacedCount++];
            stock.GetComponent<Animator>().SetTrigger("consume");
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.2f));
            stock.SetActive(false);
        }
    }

    /// <summary>
    /// Skipメッセージ表示
    /// </summary>
    /// <param name="isWhite">現在のターンが白かどうか</param>
    private async UniTask ShowSkipMessage(bool isWhite)
    {
        Waiting = true;
        if (isWhite)
        {
            skipMessageBlack.SetActive(true);
            Skip.Post(skipMessageBlack.gameObject);
            await UniTask.Delay(System.TimeSpan.FromSeconds(1.5f));
            skipMessageBlack.SetActive(false);
        }
        else
        {
            skipMessageWhite.SetActive(true);
            Skip.Post(skipMessageWhite.gameObject);
            await UniTask.Delay(System.TimeSpan.FromSeconds(1.5f));
            skipMessageWhite.SetActive(false);
        }
        Waiting = false;
    }

    /// <summary>
    /// 合法手取得
    /// </summary>
    /// <param name="validCells">合法手のセルリスト出力パラメータ</param>
    public void GetValidCells(out List<OthelloCell> validCells)
    {
        validCells = new List<OthelloCell>();
        foreach (OthelloCell cell in FindObjectsByType<OthelloCell>(FindObjectsSortMode.None))
        {
            if (othelloBoard.IsValidMove(cell.x, cell.y, isWhiteTurn ? "White" : "Black"))
                validCells.Add(cell);
        }
    }

    /// <summary>
    /// ハイライト表示
    /// </summary>
    public async void HighlightValidMoves()
    {
        GetValidCells(out List<OthelloCell> validCells);
        bool isAITurn = (isWhiteTurn && isAIWhite) || (!isWhiteTurn && !isAIWhite);
        if (isWhiteFirst == isWhiteTurn) AudioManager.Instance.ChangeBGM_1();
        AudioManager.Instance.ChangeBGM_2();
        if ((!isAIOpponent || !isAITurn) && DifficultySelect.difficulty != "secret")
        {
            foreach (OthelloCell cell in validCells)
            {
                var sr = cell.GetComponent<SpriteRenderer>();
                sr.sprite = isWhiteTurn ? whiteHintSprite : blackHintSprite;
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1.0f);
            }
        }
        await CheckSkipOrGameOver();
    }

    /// <summary>
    /// ハイライトクリア
    /// </summary>
    public void ClearHighlightedCells()
    {
        foreach (var cell in FindObjectsByType<OthelloCell>(FindObjectsSortMode.None))
        {
            var sr = cell.GetComponent<SpriteRenderer>();
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.0f);
        }
    }

    /// <summary>
    /// ゲームオーバー判定
    /// </summary>
    private async UniTask CheckSkipOrGameOver()
    {
        GetValidCells(out List<OthelloCell> validCells);
        if (validCells.Count == 0)
        {
            gameoverCounter++;
            if (gameoverCounter == 1)
            {
                isWhiteTurn = !isWhiteTurn;
                bool nextHasMove = false;
                for (int x = 0; x < gridSize; x++)
                    for (int y = 0; y < gridSize; y++)
                        if (othelloBoard.IsValidMove(x, y, isWhiteTurn ? "White" : "Black"))
                            nextHasMove = true;
                if (nextHasMove)
                {
                    await ShowSkipMessage(isWhiteTurn);
                    HighlightValidMoves();
                }
                else gameoverCounter++;
            }
            if (gameoverCounter == 2)
                await ResultManager.Instance.ShowResult();
        }
        else gameoverCounter = 0;
    }
}
