using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int score = 0;

    /// <summary>
    /// 절단 시점에 호출되어 점수를 추가함
    /// </summary>
    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log($"[ScoreManager] +{amount}점! 현재 점수: {score}");
    }

    public int GetScore()
    {
        return score;
    }

    public void ResetScore()
    {
        score = 0;
    }
}