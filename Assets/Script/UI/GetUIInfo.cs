using UnityEngine;
using TMPro;

public class GetUIInfo : MonoBehaviour
{
    public TextMeshProUGUI gameClearScoreText; // 내 UI에 있는 텍스트 (복사할 대상)

    // 외부에서 GameObject를 받아서 텍스트를 복사하는 함수
    public void CopyScoreTextFrom(GameObject sourceUI)
    {
        if (sourceUI == null)
        {
            Debug.LogWarning("sourceUI가 null입니다.");
            return;
        }

        // 자식 중 "Score"라는 이름의 오브젝트 찾기
        Transform scoreTransform = sourceUI.transform.Find("Score");
        if (scoreTransform == null)
        {
            Debug.LogWarning("Score 오브젝트를 sourceUI 안에서 찾을 수 없습니다.");
            return;
        }

        // TextMeshProUGUI 컴포넌트 가져오기
        TextMeshProUGUI sourceScoreText = scoreTransform.GetComponent<TextMeshProUGUI>();
        if (sourceScoreText == null)
        {
            Debug.LogWarning("Score 오브젝트에 TextMeshProUGUI 컴포넌트가 없습니다.");
            return;
        }

        // 내 UI에 텍스트 복사
        gameClearScoreText.text = sourceScoreText.text;
    }
}
