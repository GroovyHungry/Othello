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
        float speed = 1.0f;
        await PlayFadeOut(speed);

        BGMController.Instance.TransitionBGM(nextSceneName);
        await SceneManager.LoadSceneAsync(nextSceneName);

        await PlayFadeIn(speed);
    }
    public async UniTask PlayFadeOut(float speed)
    {
        EffectImage.SetActive(true);
        EffectAnimator.speed = speed;
        EffectAnimator.SetTrigger("FadeOut");
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f/speed));
    }
    public async UniTask PlayFadeIn(float speed)
    {
        EffectAnimator.speed = speed;
        EffectAnimator.SetTrigger("FadeIn");
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f/speed));
        EffectImage.SetActive(false);
    }
}
