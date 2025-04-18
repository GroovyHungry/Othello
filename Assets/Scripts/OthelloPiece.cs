using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

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
        // float flipDuration = GetAnimationClipLength("FlipWhitePiece");
    }
    private float GetAnimationClipLength(string animationName)
    {
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        foreach(var clip in ac.animationClips)
        {
            if(clip.name == animationName)
            {
                return clip.length;
            }
        }
        return 0.5f;
    }
    // public async UniTask Place() // async化
    // {
    //     if (gameObject.tag == "White")
    //     {
    //         // animator.SetTrigger("PlaceWhiteTrigger");
    //         await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f));
    //     }
    //     else
    //     {
    //         // animator.SetTrigger("PlaceBlackTrigger");
    //         await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f));
    //     }
    // }

    public async UniTask Flip()
    {
        // float flipDuration = GetAnimationClipLength("FlipWhitePiece");
        if (gameObject.tag == "White")
        {
            animator.SetTrigger("FlipWhiteToBlackTrigger"); // 白 → 黒
            gameObject.tag = "Black";
            spriteRenderer.sprite = blackSprite;
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f));
            // animator.ResetTrigger("FlipWhiteToBlackTrigger"); // 🔥 ここでリセット
        }
        else
        {
            animator.SetTrigger("FlipBlackToWhiteTrigger"); // 黒 → 白
            gameObject.tag = "White";
            spriteRenderer.sprite = whiteSprite;
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f));
            // animator.ResetTrigger("FlipBlackToWhiteTrigger"); // 🔥 ここでリセット
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