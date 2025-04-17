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
    public List<AffectionEntry> affection = new List<AffectionEntry>();

    public void UpdateAffection(string npcName, float delta)
    {
        AffectionEntry entry = affection.Find(e => e.npcName == npcName);
        if (entry == null)
        {
            entry = new AffectionEntry { npcName = npcName, value = 0f };
            affection.Add(entry);
        }
        entry.value += delta;
        if (entry.value < 0) entry.value = 0;
    }

    public float GetAffection(string npcName)
    {
        AffectionEntry entry = affection.Find(e => e.npcName == npcName);
        return entry != null ? entry.value : 0f;
    }
}