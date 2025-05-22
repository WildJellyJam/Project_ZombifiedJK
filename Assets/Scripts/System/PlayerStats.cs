[System.Serializable]
public class PlayerStats
{
    public static float sanity; // 理智值
    public static float socialEnergy; // 社交能量
    public static float popularity; // 校園熱門度
    public static float anxiety; // 焦慮指數

    public static void Interact()
    {
        socialEnergy -= 5f; // 每次互動扣除社交能量
        if (socialEnergy < 0) socialEnergy = 0;
    }

    public static void UpdateAnxiety(float delta)
    {
        anxiety += delta;
        if (anxiety > 120f) TriggerBadEnding(); // 焦慮>120觸發壞結局
        if (anxiety < 0) anxiety = 0;
    }

    private static void TriggerBadEnding()
    {
        // 觸發壞結局
    }
}