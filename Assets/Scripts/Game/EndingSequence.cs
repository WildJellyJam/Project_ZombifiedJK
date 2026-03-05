using System;
using System.Collections.Generic;
using UnityEngine;

public enum EndingTheme
{
    Bad,
    Normal,
    Good
}

[CreateAssetMenu(menuName = "Game/Ending Sequence", fileName = "EndingSequence")]
public class EndingSequence : ScriptableObject
{
    [Header("ID")]
    public string endingId = "Ending_A";

    [Header("Theme (切換不同UI畫面)")]
    public EndingTheme theme = EndingTheme.Normal;

    [Header("Start Image (保證每個結局至少換一次圖)")]
    [Tooltip("一進結局時會先把 TopImage 換成這張圖，即使第一句沒有勾 changeImage。")]
    public Sprite startImage;

    [Header("Lines")]
    public List<EndingLine> lines = new List<EndingLine>();

    [Header("After Finish")]
    [Tooltip("播完結局要回去的場景名稱，例如 MainMenu")]
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

    [Header("Auto Next (optional)")]
    [Tooltip("0 = 不自動跳下一句")]
    public float autoNextAfterSeconds = 0f;

    [Header("SFX (optional)")]
    public bool playSfx;
    public AudioClip sfxClip;
    [Range(0f, 1f)] public float sfxVolume = 1f;
}