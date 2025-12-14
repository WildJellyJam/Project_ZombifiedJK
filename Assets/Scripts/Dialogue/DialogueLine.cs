using UnityEngine;
using UnityEngine.Events;

// 誰在講話
public enum Speaker
{
    None,
    CharacterA,
    CharacterB
}

// 頭像情緒
public enum PortraitMood
{
    Default,
    Happy,
    Sad,
    Angry,
    Surprised,
    CustomSprite
}

[System.Serializable]
public class DialogueLine
{
    [TextArea]
    public string text;

    [Header("說話者")]
    public Speaker speaker = Speaker.CharacterA;

    [Header("頭像情緒")]
    public PortraitMood mood = PortraitMood.Default;
    public Sprite customPortraitSprite;

    [Header("角色動作（可選）")]
    public Transform moveTarget;          // 🔹 這行一定要有
    public string animatorTriggerName;    // 🔹 這行一定要有

    [Header("事件（可選）")]
    public UnityEvent onLineStart;
}
