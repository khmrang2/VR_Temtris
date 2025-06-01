using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("SceneLoaderSO 에셋")]
    [SerializeField] private SceneLoaderSO sceneLoader;   // Inspector: SceneLoader.asset 연결

    [Header("Block Box 생성 간격")]
    [SerializeField] private ObjectSpawnManager spawnManager;

    [Header("제한 시간 표시용 텍스트")]
    [SerializeField] private TMP_Text timerText;

    [Header("점수 표시용 텍스트")]
    [SerializeField] private TMP_Text scoreText;

    [Header("승리 / 패배 패널")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    // 현재 점수
    private int currentScore = 0;

    // 목표 점수
    private int goalScore = 10;

    // 남은 시간 (초)
    private float remainingTime = 300f;

    // 게임 종료 여부 플래그
    private bool isGameEnded = false;

    private void Start()
    {
        // 1. 스테이지 초기화
        InitializeStage();

        // 2. ScoreManager 이벤트 구독
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
            ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
        }
        else
        {
            Debug.LogWarning("[GameManager] ScoreManager 인스턴스를 찾을 수 없습니다.");
        }

        // 3. 타이머 코루틴 시작
        StartCoroutine(TimerRoutine());
    }

    private void OnDestroy()
    {
        // 이벤트 언구독
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
    }

    /// <summary>
    /// 스테이지 데이터 읽어와서 초기화하는 메서드
    /// </summary>
    private void InitializeStage()
    {
        if (sceneLoader == null)
        {
            Debug.LogError("[GameManager] SceneLoaderSO가 할당되지 않았습니다.");
            return;
        }

        // 점수 초기화
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
            currentScore = ScoreManager.Instance.GetScore();
        }

        // 스테이지 데이터 불러와 적용
        LoadStageData();
    }

    /// <summary>
    /// SceneLoaderSO로부터 StageDataSO를 읽어서 적용합니다.
    /// </summary>
    private void LoadStageData()
    {
        StageDataSO data = sceneLoader.stageData;
        if (data == null)
        {
            Debug.LogError("[GameManager] currentStageData가 null입니다. 메뉴에서 스테이지를 선택했는지 확인하세요.");
            return;
        }

        // 랜덤 시드 고정
        Random.InitState(data.randomSeed);

        // 블록 생성 간격 세팅
        if (spawnManager != null)
        {
            spawnManager.spawnInterval = data.blockSpawnInterval;
        }
        else
        {
            Debug.LogWarning("[GameManager] spawnManager가 할당되지 않았습니다.");
        }

        // 제한 시간 세팅 및 UI 업데이트
        remainingTime = data.timeLimit;
        if (timerText != null)
        {
            timerText.text = $"{Mathf.CeilToInt(remainingTime)}";
        }

        // 목표 점수 세팅
        goalScore = data.pointGoal;

        Debug.Log($"[GameManager] 스테이지 초기화 완료: {data.name} " +
                  $"(간격={data.blockSpawnInterval}, 시드={data.randomSeed}, 제한시간={data.timeLimit}, 목표점수={data.pointGoal})");
    }

    /// <summary>
    /// ScoreManager.OnScoreChanged 이벤트 핸들러
    /// 점수가 변경될 때 UI를 업데이트하고 게임 종료 여부를 체크합니다.
    /// </summary>
    /// <param name="newScore">변경된 총 점수</param>
    private void HandleScoreChanged(int newScore)
    {
        currentScore = newScore;
        if (scoreText != null)
        {
            scoreText.text = $"Score: {newScore}";
        }

        CheckGameEnd();
    }

    /// <summary>
    /// 타이머를 감소시키는 코루틴
    /// </summary>
    private IEnumerator TimerRoutine()
    {
        while (remainingTime > 0f && !isGameEnded)
        {
            remainingTime -= Time.deltaTime;
            if (timerText != null)
            {
                timerText.text = $"{Mathf.CeilToInt(remainingTime)}";
            }
            yield return null;
        }

        if (!isGameEnded)
        {
            remainingTime = 0f;
            CheckGameEnd();
        }
    }

    /// <summary>
    /// 게임 종료 조건을 검사합니다.
    /// 1. 승리: currentScore >= goalScore
    /// 2. 패배: remainingTime <= 0
    /// </summary>
    private void CheckGameEnd()
    {
        if (isGameEnded) return;

        // 1) 승리 조건 확인
        if (currentScore >= goalScore)
        {
            isGameEnded = true;
            Debug.Log("게임 승리!");
            if (winPanel != null)
                winPanel.SetActive(true);
            return;
        }

        // 2) 패배 조건 확인
        if (remainingTime <= 0f)
        {
            isGameEnded = true;
            Debug.Log("게임 패배...");
            if (losePanel != null)
                losePanel.SetActive(true);
        }
    }
}
