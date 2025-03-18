using UnityEngine;
using Cysharp.Threading.Tasks;

public class OthelloCell : MonoBehaviour
{
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    private bool isOccupied = false;

    public int x, y;

    private void OnMouseDown()
    {
        if (OthelloManager.initializing) return;
        if (OthelloManager.Waiting) return;

        // 今のターンの色
        string currentTag = OthelloManager.Instance.IsWhiteTurn() ? "White" : "Black";

        // 合法手チェック
        if (!isOccupied && OthelloManager.Instance.IsValidMove(x, y, currentTag))
        {
            // コマの生成
            GameObject piecePrefab = (currentTag == "White") ? whitePiecePrefab : blackPiecePrefab;
            GameObject piece = Instantiate(piecePrefab, transform.position, Quaternion.identity);
            piece.tag = currentTag; // タグ設定（重要）

            // 盤面への配置
            OthelloManager.Instance.GetBoard().PlacePiece(x, y, piece, currentTag);

            // ターン終了（交代）
            OthelloManager.Instance.EndTurn();

            // 占有済み設定
            isOccupied = true;
        }
    }
}
