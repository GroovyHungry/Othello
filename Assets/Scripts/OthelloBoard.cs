using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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

    void Start()
    {
        StartCoroutine(InitializeBoard()); // 🔥 コルーチンで実行
    }

    private IEnumerator InitializeBoard()
    {
        float waitTime = 0.1f;

        PlacePiece(3, 4, Instantiate(blackPiecePrefab, new Vector3(-0.5f, 0.5f, 0), Quaternion.identity)); // 左上 黒
        yield return new WaitForSeconds(waitTime);

        PlacePiece(4, 3, Instantiate(blackPiecePrefab, new Vector3(0.5f, -0.5f, 0), Quaternion.identity)); // 右下 黒
        yield return new WaitForSeconds(waitTime);

        PlacePiece(3, 3, Instantiate(whitePiecePrefab, new Vector3(-0.5f, -0.5f, 0), Quaternion.identity)); // 左下 白
        yield return new WaitForSeconds(waitTime);

        PlacePiece(4, 4, Instantiate(whitePiecePrefab, new Vector3(0.5f, 0.5f, 0), Quaternion.identity)); // 右上 白
    }

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

        List<GameObject> piecesToFlip = new List<GameObject>(); // 🔥 ひっくり返す駒のリスト

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int dx = directions[i, 0];
            int dy = directions[i, 1];
            piecesToFlip.AddRange(GetFlippablePieces(x, y, dx, dy, currentTag)); // 🔥 ひっくり返せる駒をリストに追加
        }

        // 🔥 すべての駒をまとめてひっくり返す
        foreach (GameObject piece in piecesToFlip)
        {
            piece.GetComponent<OthelloPiece>().Flip();
        }
    }

    // 🔥 ひっくり返せる駒を取得する関数
    private List<GameObject> GetFlippablePieces(int x, int y, int dx, int dy, string currentTag)
    {
        List<GameObject> flippablePieces = new List<GameObject>();
        int checkX = x + dx;
        int checkY = y + dy;
        bool foundOpponent = false;

        while (IsValidPosition(checkX, checkY))
        {
            GameObject checkPiece = boardState[checkX, checkY];

            if (checkPiece == null) return new List<GameObject>(); // 🔥 空のマスなら終了（無効）

            if (checkPiece.tag != currentTag)
            {
                flippablePieces.Add(checkPiece); // 🔥 相手の駒をリストに追加
                foundOpponent = true;
            }
            else
            {
                if (foundOpponent) return flippablePieces; // 🔥 挟めている場合、リストを返す
                return new List<GameObject>(); // 🔥 挟めなかったら無効
            }

            checkX += dx;
            checkY += dy;
        }

        return new List<GameObject>(); // 🔥 どの条件にも当てはまらなかった場合
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