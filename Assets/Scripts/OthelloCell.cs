using UnityEngine;

public class OthelloCell : MonoBehaviour
{
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab; // 🔥 変数名を統一（大文字を修正）

    private bool isOccupied = false; // 🔥 初期配置時に影響しないようにする

    public int x, y;

    private void OnMouseDown()
    {
        // Debug.Log($"clicked Cell num is {x}, {y}");
        if (OthelloBoard.initializing) return;
        if (OthelloBoard.Waiting) return;

        if (!isOccupied && OthelloBoard.Instance.IsValidMove(x, y, OthelloBoard.Instance.IsWhiteTurn() ? "White" : "Black"))
        {
            GameObject piece = Instantiate(
                OthelloBoard.Instance.IsWhiteTurn() ? whitePiecePrefab : blackPiecePrefab,
                transform.position,
                Quaternion.identity
            );

            isOccupied = true;
            OthelloBoard.Instance.PlacePiece(x, y, piece);
        }
    }
}
