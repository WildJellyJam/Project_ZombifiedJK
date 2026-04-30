using UnityEngine;

public class EventAnimatorIntroController : MonoBehaviour
{
    [Header("Animator")]
    public Animator imageAnimator;

    [Tooltip("Animator 裡面圖片進場動畫的 State 名稱")]
    public string introStateName = "EventIntro";

    [Header("選項")]
    public GameObject optionsRoot;

    [Header("設定")]
    public bool canClickToSkip = true;

    private bool introPlaying = false;
    private bool introFinished = false;

    private void OnEnable()
    {
        StartIntro();
    }

    private void Update()
    {
        if (!introPlaying || introFinished) return;

        if (canClickToSkip && Input.GetMouseButtonDown(0))
        {
            SkipIntro();
            return;
        }

        CheckAnimationFinished();
    }

    public void StartIntro()
    {
        introFinished = false;
        introPlaying = true;

        if (optionsRoot != null)
        {
            optionsRoot.SetActive(false);
        }

        if (imageAnimator == null)
        {
            Debug.LogError("[EventAnimatorIntroController] imageAnimator 沒有指定。", this);
            FinishIntro();
            return;
        }

        imageAnimator.gameObject.SetActive(true);
        imageAnimator.speed = 1f;

        imageAnimator.Play(introStateName, 0, 0f);
    }

    private void CheckAnimationFinished()
    {
        if (imageAnimator == null) return;

        AnimatorStateInfo stateInfo = imageAnimator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName(introStateName) && stateInfo.normalizedTime >= 1f)
        {
            FinishIntro();
        }
    }

    public void SkipIntro()
    {
        if (introFinished) return;

        if (imageAnimator != null)
        {
            imageAnimator.Play(introStateName, 0, 1f);
            imageAnimator.Update(0f);
        }

        FinishIntro();
    }

    public void FinishIntro()
    {
        if (introFinished) return;

        introFinished = true;
        introPlaying = false;

        if (imageAnimator != null)
        {
            imageAnimator.speed = 0f;
        }

        if (optionsRoot != null)
        {
            optionsRoot.SetActive(true);
        }
    }
}