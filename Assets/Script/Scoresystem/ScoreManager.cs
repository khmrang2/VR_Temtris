using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int score = 0;

    /// <summary>
    /// ���� ������ ȣ��Ǿ� ������ �߰���
    /// </summary>
    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log($"[ScoreManager] +{amount}��! ���� ����: {score}");
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