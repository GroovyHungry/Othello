using UnityEngine;

public class OthelloPiece : MonoBehaviour
{
    private Animator animator;
    public Sprite whiteSprite;
    public Sprite blackSprite;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Place()
    {
        if (gameObject.tag == "White")
        {
            animator.SetTrigger("PlaceWhiteTrigger"); // 白のコマを置くアニメーション
        }
        else
        {
            animator.SetTrigger("PlaceBlackTrigger"); // 黒のコマを置くアニメーション
        }
    }

    public void Flip()
    {
        if (gameObject.tag == "White")
        {
            animator.SetTrigger("FlipWhiteToBlackTrigger"); // 白 → 黒
            Invoke("ChangeToBlack", 0.5f);
        }
        else
        {
            animator.SetTrigger("FlipBlackToWhiteTrigger"); // 黒 → 白
            Invoke("ChangeToWhite", 0.5f);
        }
    }

    private void ChangeToBlack()
    {
        gameObject.tag = "Black";
        spriteRenderer.sprite = blackSprite;
        animator.ResetTrigger("FlipWhiteToBlackTrigger"); // 🔥 ここでリセット
    }

    private void ChangeToWhite()
    {
        gameObject.tag = "White";
        spriteRenderer.sprite = whiteSprite;
        animator.ResetTrigger("FlipBlackToWhiteTrigger"); // 🔥 ここでリセット
    }
}