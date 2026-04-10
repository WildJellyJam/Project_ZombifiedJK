using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Game/Ending Sequence", fileName = "EndingSequence")]
public class EndingSequence : ScriptableObject
{
    public string endingId = "Ending_A";

    [Header("Start Image (建議必填：保證至少換一次圖)")]
    public Sprite startImage;

    [FormerlySerializedAs("lines")]
    public List<EndingLine> steps = new List<EndingLine>();

    // （可選）如果你其它地方還用到 lines 這名字，給它一個相容讀取用的 alias
    public List<EndingLine> lines => steps;

    [Header("After Finish")]
    public string backToSceneName = "MainMenu";
}

[Serializable]
public class EndingLine
{
    public string speaker;
    [TextArea(2, 5)]
    public string text;

    [Header("Image swap at this line (optional)")]
    public bool changeImage;
    public Sprite newSprite;

    [Header("SFX at this line (optional)")]
    public bool playSfx;
    public AudioClip sfxClip;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Auto Next (optional)")]
    public float autoNextAfterSeconds = 0f;
}