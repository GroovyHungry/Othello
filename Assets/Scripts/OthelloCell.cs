using UnityEngine;
using Cysharp.Threading.Tasks;

public class OthelloCell : MonoBehaviour
{
    public int x, y;

    private void OnMouseDown()
    {
        if (OthelloManager.initializing) return;
        if (OthelloManager.Waiting) return;
        if (OthelloManager.isAIPlaying) return;

        string currentTag = OthelloManager.Instance.IsWhiteTurn() ? "White" : "Black";

        if (OthelloManager.Instance.GetBoard().IsCellEmpty(x, y) && OthelloManager.Instance.IsValidMove(x, y, currentTag))
        {
            Vector3 pos = transform.position;
            _ = OthelloManager.Instance.PlacePieces(x, y, currentTag, pos);
        }
    }
}
