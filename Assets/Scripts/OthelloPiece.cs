using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using AK.Wwise;

public class OthelloPiece : MonoBehaviour
{
    public int state_X;
    public int state_Y;
    private Animator animator;
    public Sprite whiteSprite;
    public Sprite blackSprite;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        // float flipDuration = GetAnimationClipLength("FlipWhitePiece");
    }
    public void InitState(int x, int y)
    {
        state_X = x;
        state_Y = y;
    }
    public async UniTask Place() // async化
    {
        if (gameObject.tag == "White")
        {
            // animator.SetTrigger("PlaceWhiteTrigger");
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f));
        }
        else
        {
            // animator.SetTrigger("PlaceBlackTrigger");
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f));
        }
    }

    public async UniTask Flip()
    {
        // float flipDuration = GetAnimationClipLength("FlipWhitePiece");
        if (gameObject.tag == "White")
        {
            animator.SetTrigger("FlipWhiteToBlackTrigger");
            gameObject.tag = "Black";
            spriteRenderer.sprite = blackSprite;
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.2f));
            AkSoundEngine.PostEvent("PlacePiece", gameObject);

        }
        else
        {
            animator.SetTrigger("FlipBlackToWhiteTrigger");
            gameObject.tag = "White";
            spriteRenderer.sprite = whiteSprite;
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.2f));
            AkSoundEngine.PostEvent("PlacePiece", gameObject);
        }
    }
    public async UniTask Remove(float delay)
    {
        if (gameObject.tag == "White")
        {
            animator.SetTrigger("RemoveWhitePiece");
        }
        else
        {
            animator.SetTrigger("RemoveBlackPiece");
        }
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
    }
}