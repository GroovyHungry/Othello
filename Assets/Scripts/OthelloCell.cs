using UnityEngine;
using Cysharp.Threading.Tasks;
using AK.Wwise;

public class OthelloCell : MonoBehaviour
{
    public SpriteRenderer hoverFrame;
    public int x, y;

    private bool isHovering = false;

    private void Update()
    {
        if (OthelloManager.initializing || OthelloManager.Waiting || OthelloManager.isAIPlaying)
        {
            hoverFrame.enabled = false;
            isHovering = false;
            return;
        }

        if (IsMouseOver())
        {
            string currentTag = OthelloManager.Instance.IsWhiteTurn() ? "White" : "Black";
            if (OthelloBoard.Instance.IsCellEmpty(x, y) && OthelloBoard.Instance.IsValidMove(x, y, currentTag))
            {
                if (!isHovering)
                {
                    hoverFrame.enabled = true;
                    AkSoundEngine.PostEvent("OnSelect", gameObject);
                    isHovering = true;
                }
            }
            else
            {
                hoverFrame.enabled = false;
                isHovering = false;
            }
        }
        else
        {
            hoverFrame.enabled = false;
            isHovering = false;
        }
    }

    private bool IsMouseOver()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return false;

        return col.OverlapPoint(mousePos2D);
    }

    private void OnMouseDown()
    {
        if (OthelloManager.initializing) return;
        if (OthelloManager.Waiting) return;
        if (OthelloManager.isAIPlaying) return;

        string currentTag = OthelloManager.Instance.IsWhiteTurn() ? "White" : "Black";

        if (OthelloBoard.Instance.IsCellEmpty(x, y) && OthelloBoard.Instance.IsValidMove(x, y, currentTag))
        {
            Vector3 pos = transform.position;
            _ = OthelloManager.Instance.PlacePiece(x, y, currentTag, pos);
        }
    }
}
