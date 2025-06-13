using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

public class DebugManager : MonoBehaviour
{
    public GameObject grippssser;
    public GameObject bbbbox;


    [Header("Spawn Settings")]
    public Transform spawnPoint;        // 첫 시작 스폰 포인트. 
    public float spawnInterval = 1.5f;  // 박스 오브젝트 스폰 간격 . <- (StageManager.cs)스테이지 매니져에서 받아옴.

    [Header("Box Settings")]
    public int blockVariantCount = 7;   // 랜덤 스폰할 블록 개수. 
    public int randomSeed = 410;        // 박스 오브젝트 생성 시드 <- (StageManager.cs)스테이지 매니져에서 받아옴.

    [Header("Item Settings")]
    [Tooltip("아이템에 대한 프리팹 설정은 Prefabs/Box 에서 설정해주셔야합니다.")]
    public float itemProbability = 0.1f; // 아이템이 나올 확률. 기본 10% 

    private List<int> itemBag = new List<int>();
    private int bagIndex = 0;
    private System.Random rng;

    private void Awake()
    {
        // 랜덤 시드 설정
        rng = new System.Random(randomSeed);
        CreateNewBag();
    }

    private void Start()
    {
        SpawnGrippersWithInterval();
    }

    private void SpawnGrippersWithInterval()
    {
        for (int i = 0; i < 1; i++)
        {
            // 오브젝트 풀링으로 체크해서 가져오기. 
            GameObject gripper = grippssser;
            GameObject box = bbbbox;

            // OPM 에러 체크.
            if (gripper == null || box == null)
            {
                Debug.LogWarning("Gripper 또는 Box 생성 실패");
                continue;
            }

            // 셔플추가요.
            if (bagIndex >= itemBag.Count)
                CreateNewBag();

            int chosenIndex;
            double temp = rng.NextDouble();
            // 아이템 확률
            if (temp > itemProbability)
            {
                Debug.Log($"{temp} vs {itemProbability} : 블록 생성됨 90%");
                chosenIndex = itemBag[bagIndex++];
            }
            else
            {
                Debug.Log($"{temp} vs {itemProbability} : 아이템 생성됨 10%");
                chosenIndex = rng.Next(7, 9);
            }


            // 이거 코드 리팩토링해서 OnEnable하고 OnDisable에 넣어야겠다.
            box.GetComponent<BoxOpen>()?.ResetBox(chosenIndex);

            var gripperComponent = gripper.GetComponent<Gripper>();
            gripperComponent?.ResetGripper();
            gripperComponent?.Grab(box);

            var follower = gripper.GetComponent<PathFollower>();
        }
    }

    /// <summary>
    /// 7-Bag 리스트 생성 및 셔플
    /// </summary>
    private int shuffleCount = 0;
    private void CreateNewBag()
    {
        itemBag = Enumerable.Range(0, blockVariantCount).ToList();

        // 하나의 시드에서 고정된 셔플을 강제하기 위함.
        var localRng = new System.Random(randomSeed + shuffleCount);

        // 간단한 셔플 알고리즘. 
        for (int i = itemBag.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (itemBag[i], itemBag[j]) = (itemBag[j], itemBag[i]);
        }
        // 모두 소모했으니(혹은 콜링했을때만) bagIndex를 다시 0 으로 설정.
        bagIndex = 0;
    }
}
