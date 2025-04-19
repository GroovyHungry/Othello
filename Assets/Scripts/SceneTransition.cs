using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;
    public GameObject EffectImage;
    public Animator EffectAnimator; // ← FadeImageに付いたAnimatorをアタッチ

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでも残す
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public async UniTask Transition(string nextSceneName)
    {
        EffectImage.SetActive(true);
        EffectAnimator.SetTrigger("FadeOut");
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f)); // FadeOutの時間に合わせて調整

        await SceneManager.LoadSceneAsync(nextSceneName);

        EffectAnimator.SetTrigger("FadeIn");
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f));
        EffectImage.SetActive(false);
    }
}
