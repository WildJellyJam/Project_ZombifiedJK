using System.Collections.Generic; 
using UnityEngine;

[System.Serializable]
public class AffectionEntry
{
    public string npcName;
    public float value;
}

[System.Serializable]
public class NPCAffection
{
    public Dictionary<string, float> affection = new Dictionary<string, float>();

    public void UpdateAffection(string npcName, float delta)
    {
        if (!affection.ContainsKey(npcName)) // 使用 ContainsKey 替代 Find
        {
            affection[npcName] = 0f;
        }
        affection[npcName] += delta;
        if (affection[npcName] < 0) affection[npcName] = 0;
    }
}