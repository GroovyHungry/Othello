using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Linq;
using System;

public class OthelloBoard : MonoBehaviour
{
    public static OthelloBoard Instance;
    public const int gridSize = 8;
    private static readonly (int dx,int dy)[] directions = {
    ( 1, 0),(-1, 0),( 0, 1),( 0,-1),
    ( 1, 1),(-1,-1),( 1,-1),(-1, 1),
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
        AkSoundEngine.PostEvent("PlacePiece", piece);
        await UniTask.Delay(TimeSpan.FromSeconds(0.15f));
        await CheckAndFlipPieces(x, y, tag);
        OthelloManager.Waiting = false;
    }
    // 指定座標が空かどうか
    public bool IsCellEmpty(int x, int y) => boardState[x, y] == null;
    public bool IsValidPosition(int x, int y) => x >= 0 && x < gridSize && y >= 0 && y < gridSize;

    // 指定座標のコマを取得
    public GameObject GetPiece(int x, int y) => boardState[x, y];
    public GameObject GetBoardState() => boardState;
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
    public bool IsValidMove(GameObject[,] board, int x, int y, string currentTag)
    {
        // if (!othelloBoard.IsCellEmpty(x, y)) return false;
        if (board[x, y] != null) return false;

        foreach (var(dx, dy) in directions)
        {
            int checkX = x + dx;
            int checkY = y + dy;
            bool foundOpponent = false;
            while (IsValidPosition(checkX, checkY))
            {
                GameObject piece = board[checkX, checkY];
                if (piece == null) break;
                if (piece.tag != currentTag)
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
        }
        return false;
    }
    private async UniTask CheckAndFlipPieces(int x, int y, string currentTag)
    {
        List<List<GameObject>> byDir = new List<List<GameObject>>();

        foreach (var (dx,dy) in directions)
        {
            var list = GetFlippablePieces(x, y, dx, dy, currentTag);
            if (list.Count > 0) byDir.Add(list);
        }
        if (byDir.Count == 0)return;

        await FlipPieces(byDir);
    }
    private async UniTask FlipPieces(List<List<GameObject>> byDir)
    {
        int maxLayer = byDir.Max(list => list.Count);
        float startDelay = 0.1f;
        float acceleration = 0.05f;

        AkSoundEngine.PostEvent("FlipPiece", gameObject);
        for (int layer = 0; layer < maxLayer; layer++)
        {
            var tasks = new List<UniTask>();
            foreach (var list in byDir)
            {
                if (layer < list.Count)
                    list[layer].GetComponent<OthelloPiece>().Flip().Forget();
            }

            float delay = Math.Max(0.01f, startDelay - acceleration * layer);
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
        }
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
    }

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
}
