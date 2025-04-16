using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class OthelloBoard : MonoBehaviour
{
    public static OthelloBoard Instance;
    public const int gridSize = 8;
    private static readonly int[,] directions = {
        { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 },
        { 1, 1 }, { -1, -1 }, { 1, -1 }, { -1, 1 }
    };
    private GameObject[,] boardState = new GameObject[gridSize, gridSize];

    private void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

    // コマを配置し、ひっくり返せる駒があれば反転処理
    public async UniTask PlacePiece(int x, int y, GameObject piece, string tag)
    {
        OthelloManager.Waiting = true;

        boardState[x, y] = piece;
        List<GameObject> piecesToFlip = CheckAndFlipPieces(x, y, tag);
        if (piecesToFlip.Count > 0)
        {
            await FlipPieces(piecesToFlip);
        }
        OthelloManager.Waiting = false;
    }
    // 指定座標が空かどうか
    public bool IsCellEmpty(int x, int y) => boardState[x, y] == null;

    // 指定座標のコマを取得
    public GameObject GetPiece(int x, int y) => boardState[x, y];
    public void ClearBoardState()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                boardState[x, y] = null;
            }
        }
    }

    // ひっくり返す処理の開始
    // 同期で一括取得（変更なし）
    private List<GameObject> CheckAndFlipPieces(int x, int y, string currentTag)
    {
        List<GameObject> piecesToFlip = new List<GameObject>();

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int dx = directions[i, 0];
            int dy = directions[i, 1];
            piecesToFlip.AddRange(GetFlippablePieces(x, y, dx, dy, currentTag));
        }

        return piecesToFlip;
    }
    // 実際にひっくり返すコルーチン
    private async UniTask FlipPieces(List<GameObject> piecesToFlip)
    {
        float interval = 0.0f;

        // 並列実行
        var flipTasks = new List<UniTask>();
        foreach (GameObject piece in piecesToFlip)
        {
            //await piece.GetComponent<OthelloPiece>().Flip();
            flipTasks.Add(piece.GetComponent<OthelloPiece>().Flip());
        }
        //await UniTask.Delay(System.TimeSpan.FromSeconds(interval));
        await UniTask.WhenAll(flipTasks);
    }

    // ひっくり返せるコマをリストアップする
    private List<GameObject> GetFlippablePieces(int x, int y, int dx, int dy, string currentTag)
    {
        List<GameObject> flippablePieces = new List<GameObject>();
        int checkX = x + dx;
        int checkY = y + dy;
        bool foundOpponent = false;

        while (IsValidPosition(checkX, checkY))
        {
            GameObject checkPiece = boardState[checkX, checkY];

            if (checkPiece == null)
            {
                // 空のマスなら途中で終了（ひっくり返せない）
                return new List<GameObject>();
            }

            if (checkPiece.tag != currentTag)
            {
                // 相手のコマがあれば一旦リストに追加
                flippablePieces.Add(checkPiece);
                foundOpponent = true;
            }
            else
            {
                // 自分のコマにたどり着いた場合
                if (foundOpponent)
                {
                    // 挟めている場合、これまで追加した駒を返す
                    return flippablePieces;
                }
                else
                {
                    // 挟めていない場合、無効
                    return new List<GameObject>();
                }
            }

            // 次のマスをチェック
            checkX += dx;
            checkY += dy;
        }

        // 盤面の外まで到達した場合も無効
        return new List<GameObject>();
    }

    // 盤面の範囲内かどうかチェック
    private bool IsValidPosition(int x, int y) => x >= 0 && x < gridSize && y >= 0 && y < gridSize;
}
