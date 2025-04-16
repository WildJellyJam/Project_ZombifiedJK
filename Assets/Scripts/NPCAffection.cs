using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCAffection
{
    public Dictionary<string, float> affection = new Dictionary<string, float>();

    public void UpdateAffection(string npcName, float delta)
    {
        if (!affection.ContainsKey(npcName)) affection[npcName] = 0f;
        affection[npcName] += delta;
        if (affection[npcName] < 0) affection[npcName] = 0;
    }
}