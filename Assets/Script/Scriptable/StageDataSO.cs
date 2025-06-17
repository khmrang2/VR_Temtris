using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/StageData")]
public class StageDataSO : ScriptableObject
{
    [Header("스테이지 설정")]
    public float blockSpawnInterval = 1.0f;   // 블록 생성 간격 (초 단위)
    public int randomSeed = 12345;            // 랜덤 시드 (블록 배치 또는 패턴 고정용)
    public float timeLimit = 60f;             // 제한 시간 (초 단위)
    public int pointGoal = 100;               // 목표 점수.
}
