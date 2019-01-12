using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public int level = 1;
    public int maxLevel = 200;
    public int curExperience = 0;
    public int baseXP = 50;
    public int minExpGain = 7;
    public int maxExpGain = 10;
    public int[] expList;

    private float displayCurXP;
    private int expToNextLevel;

    void Start()
    {
        expList = new int[maxLevel + 1];
        expList[1] = baseXP;
        for (int i = 2; i < expList.Length; i++)
        {
            expList[i] = expList[i - 1] + Mathf.RoundToInt(Random.Range(minExpGain, maxExpGain) * i);
        }
    }

    void Update()
    {
        expToNextLevel = expList[level];
        level = Mathf.Clamp(level, 1, maxLevel);

        if (displayCurXP >= expToNextLevel - 0.1f)
        {
            int overkill = curExperience - expToNextLevel;
            LevelUp(overkill);
        }

        displayCurXP = Mathf.Clamp(Mathf.Lerp(displayCurXP, curExperience, Time.deltaTime * 2.15f), 0f, expToNextLevel);
    }

    public void GetXP(int amount)
    {
        curExperience += amount;
    }

    public void LevelUp(int leftover = 0)
    {
        level++;
        curExperience = leftover;
        displayCurXP = 0f;
    }

    public void SetLevel(int amount)
    {
        level = amount;
    }
}