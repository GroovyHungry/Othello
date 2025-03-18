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
    public async UniTask Place() // async化
    {
        if (gameObject.tag == "White")
        {
            animator.SetTrigger("PlaceWhiteTrigger");
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f));
        }
        else
        {
            animator.SetTrigger("PlaceBlackTrigger");
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f));
        }
    }

    public async UniTask Flip()
    {
        float flipDuration = GetAnimationClipLength("FlipWhitePiece");
        Debug.Log(flipDuration.GetType());
        if (gameObject.tag == "White")
        {
            animator.SetTrigger("FlipWhiteToBlackTrigger"); // 白 → 黒
            gameObject.tag = "Black";
            spriteRenderer.sprite = blackSprite;
            await UniTask.Delay(System.TimeSpan.FromSeconds(flipDuration));
            animator.ResetTrigger("FlipWhiteToBlackTrigger"); // 🔥 ここでリセット
        }
        else
        {
            animator.SetTrigger("FlipBlackToWhiteTrigger"); // 黒 → 白
            gameObject.tag = "White";
            spriteRenderer.sprite = whiteSprite;
            await UniTask.Delay(System.TimeSpan.FromSeconds(flipDuration));
            animator.ResetTrigger("FlipBlackToWhiteTrigger"); // 🔥 ここでリセット
        }
    }
}