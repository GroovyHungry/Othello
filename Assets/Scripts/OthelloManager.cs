using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.Linq;
using AK.Wwise;

public class OthelloManager : MonoBehaviour
{
    public GameObject flipMarker;
    public static OthelloManager Instance;
    public static bool Waiting = false;
    public static bool initializing = false;
    private bool previousWaiting = false;
    public bool isWhiteTurn = false;
    public bool isWhiteFirst = false;
    public static bool isAIPlaying = false;
    public static bool isAIOpponent = true;
    public bool isAIWhite = true;
    public GameObject youWhite;
    public GameObject youBlack;
    public GameObject cpuWhite;
    public GameObject cpuBlack;
    public GameObject player1;
    public GameObject player2;
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
    private OthelloBoard othelloBoard;
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
    public Button settingButtonInGame;
    public Button exitButton;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        Application.targetFrameRate = 60;
        othelloBoard = GetComponent<OthelloBoard>();

        settingButtonInGame.onClick.AddListener(OnSettingButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }
    private void OnDestroy()
    {
        exitButton.onClick.RemoveListener(OnExitButtonClicked);
        settingButtonInGame.onClick.RemoveListener(OnSettingButtonClicked);
    }
    private async UniTaskVoid Start()
    {
        await StartGame();
    }
    public async UniTask StartGame()
    {
        if (isAIOpponent)
        {
            await DifficultySelectManager.Instance.StartDifficultySelect();
            await CoinTossManager.Instance.StartCoinTossVsCPU();
            // ShowYouAndCPUUI();
        }
        else
        {
            await CoinTossManager.Instance.StartCoinTossPvP();
            ShowP1AndP2();

        }
        isWhiteFirst = isWhiteTurn;

        GenerateStockPieces();
        await InitializeBoard();
        HighlightValidMoves();
        bool isAITurn = (isWhiteTurn && isAIWhite) || (!isWhiteTurn && !isAIWhite);
        if (isAIOpponent && isAITurn)
        {
            await OthelloAI.Instance.PlayAITurn();
        }
    }
    public async UniTask ExitToMainMenu()
    {
        othelloBoard.ClearBoardState();
        await SceneTransition.Instance.Transition("MainMenu");
    }
    private void OnExitButtonClicked()
    {
        Waiting = true;
        DoubleCheckManager.Instance.OpenDoubleCheckPanel();
        AkSoundEngine.PostEvent("OnClick", exitButton.gameObject);
    }
    private void OnSettingButtonClicked()
    {
        Waiting = true;
        SettingManager.Instance.OpenSettingPanel();
        settingButtonInGame.interactable = false;
        AkSoundEngine.PostEvent("OnClick", settingButtonInGame.gameObject);
    }
    public void UpdateScoreUI()
	{
		UpdateScore(CountPieces(true), whiteDigit1, whiteDigit2);
		UpdateScore(CountPieces(false), blackDigit1, blackDigit2);
	}

	void UpdateScore(int score, Image digit1, Image digit2)
	{
		int tens = score / 10;
		int ones = score % 10;

		digit1.sprite = numSprites[tens];
		digit2.sprite = numSprites[ones];
	}
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
    private void ShowP1AndP2()
    {
        player1.SetActive(true);
        player2.SetActive(true);
    }
    public async UniTask PlacePiece(int x, int y, string tag, Vector3 position)
    {
        Waiting = true;
        ConsumeStock(tag);
        GameObject prefab = (tag == "White") ? whitePiecePrefab : blackPiecePrefab;
        GameObject piece = Instantiate(prefab, position, Quaternion.identity);
        piece.GetComponent<OthelloPiece>().InitState(x, y);
        piece.tag = tag;
        AkSoundEngine.PostEvent("PlacePiece", piece);
        ClearHighlightedCells();
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.15f));
        await othelloBoard.ApplyMove(x, y, piece, tag);
        Waiting = false;
        await EndTurn();
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
        float waitTime = 0.05f;

        await PlaceInitialPiece(3, 4, blackPiecePrefab);
        await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime));
        await PlaceInitialPiece(3, 3, whitePiecePrefab);
        await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime));
        await PlaceInitialPiece(4, 3, blackPiecePrefab);
        await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime));
        await PlaceInitialPiece(4, 4, whitePiecePrefab);
        await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime));

        initializing = false;
    }

    private async UniTask PlaceInitialPiece(int x, int y, GameObject prefab)
    {
        GameObject piece = Instantiate(prefab, new Vector3(x - 3.5f, y - 3.5f, 0), Quaternion.identity);
        piece.GetComponent<OthelloPiece>().InitState(x, y);
        AkSoundEngine.PostEvent("PlacePiece", piece);
        await othelloBoard.ApplyMove(x, y, piece, piece.tag);
    }

    // ターン情報
    public bool IsWhiteTurn() => isWhiteTurn;
    public bool IsAIWhite() => isAIWhite;
    public bool IsPlayerWhite() => !isAIWhite;

    public async UniTask EndTurn()
    {
        isWhiteTurn = !isWhiteTurn;

        HighlightValidMoves();

        bool isAITurn = (isWhiteTurn && isAIWhite) || (!isWhiteTurn && !isAIWhite);
        if (isAIOpponent && isAITurn)
        {
            await OthelloAI.Instance.PlayAITurn();
        }
    }
    public void ConsumeStock(string tag)
    {
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

    // 指定方向で挟めるかチェック
    // private bool CanFlipDirection(int x, int y, int dx, int dy, string currentTag)
    // {
    //     int checkX = x + dx;
    //     int checkY = y + dy;
    //     bool foundOpponent = false;

    //     while (IsValidPosition(checkX, checkY))
    //     {
    //         GameObject checkPiece = othelloBoard.GetState(checkX, checkY);
    //         if (checkPiece == null) return false;

    //         if (checkPiece.tag != currentTag)
    //         {
    //             foundOpponent = true;
    //         }
    //         else
    //         {
    //             return foundOpponent;
    //         }
    //         checkX += dx;
    //         checkY += dy;
    //     }
    //     return false;
    // }


    private async UniTask ShowSkipMessage(bool isWhite)
    {
        Waiting = true;
        if (isWhite)
        {
            skipMessageBlack.SetActive(true);
            AkSoundEngine.PostEvent("Skip", skipMessageBlack.gameObject);
            await UniTask.Delay(System.TimeSpan.FromSeconds(1.5));
            skipMessageBlack.SetActive(false);
        }
        else
        {
            skipMessageWhite.SetActive(true);
            AkSoundEngine.PostEvent("Skip", skipMessageWhite.gameObject);
            await UniTask.Delay(System.TimeSpan.FromSeconds(1.5));
            skipMessageWhite.SetActive(false);
        }
        Waiting = false;
    }
    public void GetValidCells(out List<OthelloCell> validCells)
    {
        validCells = new List<OthelloCell>();

        foreach (OthelloCell cell in FindObjectsByType<OthelloCell>(FindObjectsSortMode.None))
        {
            if (othelloBoard.IsValidMove(cell.x, cell.y, isWhiteTurn ? "White" : "Black"))
            {
                validCells.Add(cell);
            }
        }
    }
    public async void HighlightValidMoves()
    {
        GetValidCells(out List<OthelloCell> validCells);

        bool isAITurn = (isWhiteTurn && isAIWhite) || (!isWhiteTurn && !isAIWhite);

        if (isWhiteFirst == isWhiteTurn)
        {
            BGMController.Instance.ChangeBGM_1();
        }
        BGMController.Instance.ChangeBGM_2();

        // ClearHighlightedCells();

        if (!isAIOpponent || (isAIOpponent && !isAITurn))
        {
            // ハイライト表示
            foreach (OthelloCell cell in validCells)
            {
                SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                sr.sprite = isWhiteTurn ? whiteHintSprite : blackHintSprite;
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1.0f);
            }
        }
        await CheckSkipOrGameOver();
    }
    public void ClearHighlightedCells()
    {
        var allCells = FindObjectsByType<OthelloCell>(FindObjectsSortMode.None);
        foreach (var cell in allCells)
        {
            SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.0f);
        }
    }
    private async UniTask CheckSkipOrGameOver()
    {
        GetValidCells(out List<OthelloCell> validCells);

        if (validCells.Count == 0)
        {
            gameoverCounter += 1;

            if (gameoverCounter == 1)
            {
                isWhiteTurn = !isWhiteTurn;
                bool nextHasMove = false;

                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        if (othelloBoard.IsValidMove(x, y, isWhiteTurn ? "White" : "Black"))
                        {
                            nextHasMove = true;
                        }
                    }
                }

                if (nextHasMove)
                {
                    await ShowSkipMessage(isWhiteTurn);
                    // await UniTask.DelayFrame(1);
				    HighlightValidMoves();
                }
                else
                {
                    gameoverCounter += 1;
                }
            }
            if (gameoverCounter == 2)
            {
                await ResultManager.Instance.ShowResult();
            }
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
                string piece = othelloBoard.GetState(x, y);
                if (piece != null)
                {
                    if (piece == "White") whiteCount++;
                    else if (piece == "Black") blackCount++;
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
    private void Update()
    {
        if (!initializing && !Waiting)
        {
            UpdateScoreUI();
            // Debug.Log("black"+CountPieces(false));
            // Debug.Log("white"+CountPieces(true));
        }
        previousWaiting = Waiting;
    }
}
