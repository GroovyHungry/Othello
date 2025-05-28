using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using AK.Wwise;

/// <summary>
/// 8x8のオセロ盤面管理を行うクラス
/// </summary>
public class OthelloBoard : MonoBehaviour
{
    /// <summary>
    /// グローバルアクセス用インスタンス
    /// </summary>
    public static OthelloBoard Instance;

    /// <summary>
    /// 盤面のサイズ（8x8）
    /// </summary>
    public const int gridSize = 8;

    /// <summary>
    /// 裏返し判定用の8方向オフセットベクトル
    /// </summary>
    private static readonly (int dx, int dy)[] directions = {
        (1, 0), (-1, 0), (0, 1), (0, -1),
        (1, 1), (-1, -1), (1, -1), (-1, 1)
    };

    /// <summary>
    /// 盤面上の駒所有者を格納する配列 ("White"/"Black"/null)
    /// </summary>
    private string[,] boardState = new string[gridSize, gridSize];

    /// <summary>
    /// 盤面上の駒GameObjectを格納する配列
    /// </summary>
    private GameObject[,] pieceObjects = new GameObject[gridSize, gridSize];

    /// <summary>
    /// Wwiseイベント
    /// </summary>
    [SerializeField] private AK.Wwise.Event flipPieceEvent;
    [SerializeField] private AK.Wwise.Event placePieceEvent;

    /// <summary>
    /// インスタンスを設定
    /// </summary>
    private void Awake()
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

    /// <summary>
    /// 駒を配置し，影響を受ける駒を非同期で裏返す
    /// </summary>
    /// <param name="x">列インデックス (0-7)</param>
    /// <param name="y">行インデックス (0-7)</param>
    /// <param name="piece">配置する駒のGameObject</param>
    /// <param name="tag">配置プレイヤー色タグ ("White"/"Black")</param>
    public async UniTask ApplyMove(int x, int y, GameObject piece, string tag)
    {
        pieceObjects[x, y] = piece;
        boardState[x, y] = tag;
        await CheckAndFlipPieces(x, y, tag);
    }

    /// <summary>
    /// 指定セルが空かどうかを返す
    /// </summary>
    /// <param name="x">列インデックス</param>
    /// <param name="y">行インデックス</param>
    /// <returns>空ならtrue</returns>
    public bool IsCellEmpty(int x, int y) => boardState[x, y] == null;

    /// <summary>
    /// 指定座標が盤面内かを判定する
    /// </summary>
    /// <param name="x">列インデックス</param>
    /// <param name="y">行インデックス</param>
    /// <returns>有効ならtrue</returns>
    public bool IsValidPosition(int x, int y) => x >= 0 && x < gridSize && y >= 0 && y < gridSize;

    /// <summary>
    /// セルの所有者タグを取得する
    /// </summary>
    /// <param name="x">列インデックス</param>
    /// <param name="y">行インデックス</param>
    /// <returns>"White","Black"またはnull</returns>
    public string GetState(int x, int y) => boardState[x, y];

    /// <summary>
    /// セルに配置された駒オブジェクトを取得する
    /// </summary>
    /// <param name="x">列インデックス</param>
    /// <param name="y">行インデックス</param>
    /// <returns>駒のGameObject</returns>
    public GameObject GetPiece(int x, int y) => pieceObjects[x, y];

    /// <summary>
    /// 現在の盤面状態をディープコピーして返す
    /// 外部からの盤面状況の操作を避けるため
    /// </summary>
    /// <returns>2D配列のコピー</returns>
    public string[,] GetBoardState()
    {
        var copy = new string[gridSize, gridSize];
        for (int i = 0; i < gridSize; i++)
            for (int j = 0; j < gridSize; j++)
                copy[i, j] = boardState[i, j];
        return copy;
    }

    /// <summary>
    /// 盤面と駒オブジェクトをクリアし初期化する
    /// </summary>
    public void ClearBoardState()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                boardState[i, j] = null;
                if (pieceObjects[i, j] != null)
                {
                    Destroy(pieceObjects[i, j]);
                    pieceObjects[i, j] = null;
                }
            }
        }
    }

    /// <summary>
    /// 指定位置に駒を置いたときの合法手判定
    /// </summary>
    /// <param name="x">列インデックス</param>
    /// <param name="y">行インデックス</param>
    /// <param name="currentTag">現在のプレイヤータグ</param>
    /// <param name="board">チェック用盤面配列 (nullで内部盤面を使用)</param>
    /// <returns>合法手ならtrue</returns>
    public bool IsValidMove(int x, int y, string currentTag, string[,] board = null)
    {
        board ??= boardState;
        if (board[x, y] != null) return false;
        foreach (var (dx, dy) in directions)
        {
            int cx = x + dx, cy = y + dy;
            bool foundOpponent = false;
            while (IsValidPosition(cx, cy))
            {
                var state = board[cx, cy];
                if (state == null) break;
                if (state != currentTag) foundOpponent = true;
                else if (foundOpponent) return true;
                else break;
                cx += dx; cy += dy;
            }
        }
        return false;
    }

    /// <summary>
    /// 駒配置後に裏返すべき駒を検出・反映する非同期処理
    /// </summary>
    /// <param name="x">配置列インデックス</param>
    /// <param name="y">配置行インデックス</param>
    /// <param name="currentTag">配置プレイヤータグ</param>
    private async UniTask CheckAndFlipPieces(int x, int y, string currentTag)
    {
        var byDir = new List<List<Vector2Int>>();
        foreach (var (dx, dy) in directions)
        {
            var list = GetFlippablePieces(x, y, dx, dy, currentTag);
            if (list.Count > 0) byDir.Add(list);
        }
        if (byDir.Count > 0) await FlipPieces(byDir, currentTag);
    }

    /// <summary>
    /// 裏返す駒リストをレイヤーごとにアニメーション付きで反映
    /// </summary>
    /// <param name="byDir">方向ごとのフリップ位置リスト</param>
    /// <param name="currentTag">配置プレイヤータグ</param>
    private async UniTask FlipPieces(List<List<Vector2Int>> byDir, string currentTag)
    {
        int maxLayer = 0;
        foreach (var list in byDir) if (list.Count > maxLayer) maxLayer = list.Count;
        float startDelay = 0.1f, acceleration = 0.05f;
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
        await UniTask.Delay(TimeSpan.FromSeconds(0.3));
    }

    /// <summary>
    /// 1方向に沿って裏返せる駒位置を取得する
    /// </summary>
    /// <param name="x">開始列インデックス</param>
    /// <param name="y">開始行インデックス</param>
    /// <param name="dx">検出方向ベクトルX</param>
    /// <param name="dy">検出方向ベクトルY</param>
    /// <param name="currentTag">配置プレイヤータグ</param>
    /// <returns>裏返せる駒の座標リスト</returns>
    private List<Vector2Int> GetFlippablePieces(int x, int y, int dx, int dy, string currentTag)
    {
        var result = new List<Vector2Int>();
        int cx = x + dx, cy = y + dy;
        bool foundOpponent = false;
        while (IsValidPosition(cx, cy))
        {
            var state = boardState[cx, cy];
            if (state == null) return new List<Vector2Int>();
            if (state != currentTag)
            {
                result.Add(new Vector2Int(cx, cy));
                foundOpponent = true;
            }
            else return foundOpponent ? result : new List<Vector2Int>();
            cx += dx; cy += dy;
        }
        return new List<Vector2Int>();
    }

    /// <summary>
    /// 指定の色の駒数をカウントする
    /// </summary>
    /// <param name="isWhite">白か否か</param>
    /// <returns>指定色の駒数</returns>
    public int CountPieces(bool isWhite)
    {
        int white = 0, black = 0;
        for (int i = 0; i < gridSize; i++)
            for (int j = 0; j < gridSize; j++)
            {
                var state = boardState[i, j];
                if (state == "White") white++;
                else if (state == "Black") black++;
            }
        return isWhite ? white : black;
    }
}
