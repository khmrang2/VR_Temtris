using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int currentScore = 0;

    // 점수가 변경될 때마다 호출할 이벤트
    // Update보단 점수가 변경될때마다 호출.. 최적화 하셔야지..
    public event Action<int> OnScoreChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 점수를 더합니다. (예: 적 처치 시 호출)
    /// </summary>
    public void AddScore(int amount)
    {
        currentScore += amount;
        Debug.Log($"[ScoreManager] +{amount}점! 현재 점수: {currentScore}");
        // 구독자들에게 알림
        OnScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// 현재 점수를 반환합니다.
    /// </summary>
    public int GetScore()
    {
        return currentScore;
    }

    /// <summary>
    /// 점수를 초기화할 때 사용
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }
}