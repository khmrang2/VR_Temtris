using UnityEngine;
using TMPro;

public class GetScore : MonoBehaviour
{
    [Header("UI Text 연결")]
    [Tooltip("최종 점수를 표시할 텍스트")]
    public TextMeshProUGUI scoreText;

    [Tooltip("레벨을 표시할 텍스트")]
    public TextMeshProUGUI levelText;

    [Header("레벨 설정")]
    [Tooltip("이 화면에 표시할 레벨 값 (수동 입력)")]
    public int levelAmount;


    void OnEnable()
    {
        if (scoreText == null)
        {
            Debug.LogWarning("scoreText 연결 안 됨");
            return;
        }
        if (levelText == null)
        {
            Debug.LogWarning("scoreText 연결 안 됨");
            return;
        }

        if (ScoreManager.Instance == null)
        {
            Debug.LogWarning("ScoreManager 인스턴스 없음");
            return;
        }

        int finalScore = ScoreManager.Instance.GetScore();
        scoreText.text = $"Score {finalScore}";
        levelText.text = $"Level {levelAmount}";
    }
}
