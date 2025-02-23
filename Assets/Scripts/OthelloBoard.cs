using UnityEngine;

public class OthelloBoard : MonoBehaviour
{
    public static OthelloBoard Instance;

    private const int gridSize = 8;
    private GameObject[,] boardState = new GameObject[gridSize, gridSize];

    private bool isWhiteTurn = false;

    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // void Start()
    // {
    //     InitializeBoard(); // 🔥 初期配置
    // }

    // private void InitializeBoard()
    // {
    //     int centerX = gridSize / 2;
    //     int centerY = gridSize / 2;

    //     PlacePiece(centerX - 1, centerY - 1, Instantiate(blackPiecePrefab, transform.position, Quaternion.identity)); // 左上 黒
    //     PlacePiece(centerX, centerY, Instantiate(blackPiecePrefab, transform.position, Quaternion.identity)); // 右下 黒
    //     PlacePiece(centerX - 1, centerY, Instantiate(whitePiecePrefab, transform.position, Quaternion.identity)); // 左下 白
    //     PlacePiece(centerX, centerY - 1, Instantiate(whitePiecePrefab, transform.position, Quaternion.identity)); // 右上 白
    // }

    public void PlacePiece(int x, int y, GameObject piece)
    {
        boardState[x, y] = piece;
        piece.GetComponent<OthelloPiece>().Place();
        CheckAndFlipPieces(x, y, piece.tag);

        isWhiteTurn = !isWhiteTurn;
        Debug.Log($"Turn: {(isWhiteTurn ? "White" : "Black")}");
    }

    public bool IsCellEmpty(int x, int y)
    {
        return boardState[x, y] == null;
    }

    // 🔥 追加：指定した座標の駒を取得
    public GameObject GetPiece(int x, int y)
    {
        return boardState[x, y];
    }

    private void CheckAndFlipPieces(int x, int y, string currentTag)
    {
        int[,] directions = {
            { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 }, // 水平方向 & 垂直方向
            { 1, 1 }, { -1, -1 }, { 1, -1 }, { -1, 1 }  // 斜め方向
        };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int dx = directions[i, 0];
            int dy = directions[i, 1];
            FlipInDirection(x, y, dx, dy, currentTag);
        }
    }

    private void FlipInDirection(int x, int y, int dx, int dy, string currentTag)
    {
        int checkX = x + dx;
        int checkY = y + dy;
        bool foundOpponent = false;
        GameObject[] toFlip = new GameObject[gridSize];

        int flipCount = 0;
        while (IsValidPosition(checkX, checkY))
        {
            GameObject checkPiece = boardState[checkX, checkY];

            if (checkPiece == null) return; // 空のマスなら終了

            if (checkPiece.tag != currentTag)
            {
                // 相手の駒を見つけた → 挟める可能性あり
                toFlip[flipCount++] = checkPiece;
                foundOpponent = true;
            }
            else if (foundOpponent)
            {
                // 自分の駒に戻ったので、挟んだ駒を裏返す
                for (int i = 0; i < flipCount; i++)
                {
                    toFlip[i].GetComponent<OthelloPiece>().Flip();
                }
                return;
            }
            else return; // 挟めていなければ何もしない

            checkX += dx;
            checkY += dy;
        }
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridSize && y >= 0 && y < gridSize;
    }

    private void Update()
    {
        int whiteCount = 0;
        int blackCount = 0;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (boardState[x, y] != null)
                {
                    if (boardState[x, y].tag == "White")
                    {
                        whiteCount++;
                    }
                    else if (boardState[x, y].tag == "Black")
                    {
                        blackCount++;
                    }
                }
            }
        }

        Debug.Log($"White: {whiteCount}, Black: {blackCount}");
    }

    public bool IsWhiteTurn()
    {
        return isWhiteTurn;
    }
}
