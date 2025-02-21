using UnityEngine;

public class OthelloPiece : MonoBehaviour
{
    public Sprite whiteSprite;
    public Sprite blackSprite;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Flip()
    {
        if (gameObject.tag == "White")
        {
            gameObject.tag = "Black";
            spriteRenderer.sprite = blackSprite;
        }
        else
        {
            gameObject.tag = "White";
            spriteRenderer.sprite = whiteSprite;
        }
        Debug.Log($"After Flip: {gameObject.name} position: {transform.position}");
    }
    
}
