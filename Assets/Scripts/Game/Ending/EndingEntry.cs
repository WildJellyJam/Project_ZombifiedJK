using UnityEngine;

public class EndingEntry : MonoBehaviour
{
    public EndingSequencePlayer playerInScene;

    private void Start()
    {
        if (EndingFlow.Instance == null || EndingFlow.Instance.pendingSequence == null)
        {
            Debug.LogError("[EndingEntry] No pending ending sequence. Did you forget to set EndingFlow.pendingSequence before loading EndingTextScene?");
            return;
        }

        playerInScene.sequence = EndingFlow.Instance.pendingSequence;
        playerInScene.gameObject.SetActive(true);

        // 可選：播完回主畫面後你也可以在主畫面清掉
        // EndingFlow.Instance.ClearPending();
    }
}