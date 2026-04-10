using UnityEngine;

public class EndingFlowButtonBridge : MonoBehaviour
{
    // BadEndingScene 的 continue 用這個
    public void GoToEndingText()
    {
        EndingFlow.Instance.GoToEndingText();
    }

    // 需要指定結局（可做 debug 用）
    public EndingSequence badEnding;
    public EndingSequence normalEnding;
    public EndingSequence goodEnding;

    public void PlayBad()
    {
        EndingFlow.Instance.StartBadEndingFlow(badEnding);
    }

    public void PlayNormal()
    {
        EndingFlow.Instance.StartEndingText(normalEnding);
    }

    public void PlayGood()
    {
        EndingFlow.Instance.StartEndingText(goodEnding);
    }
}