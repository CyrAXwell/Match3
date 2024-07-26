using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Match3UI : MonoBehaviour
{
    [SerializeField] private Match3 match3;
    [SerializeField] private TMP_Text level;
    [SerializeField] private TMP_Text score;
    [SerializeField] private TMP_Text targetScore;
    [SerializeField] private TMP_Text moves;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private CanvasGroup transition;
    [SerializeField] private GameObject glassIcon;
    [SerializeField] private AudioManager audioManager;

    private void Awake()
    {
        glassIcon.SetActive(false);
        transition.gameObject.SetActive(true);
        transition.DOFade(0, 0.5f).SetUpdate(UpdateType.Late, false).OnComplete(()=> transition.gameObject.SetActive(false));

        match3.OnScoreChange += OnScoreChange;
        match3.OnSetLevel += OnSetLevel;
        match3.OnMove += OnMove;
        match3.OnGameOver += OnGameOver;
    }

    private void OnSetLevel()
    {
        if (match3.GetTarget() == TargetType.glass)
            glassIcon.SetActive(true);

        UpdateText();
    }

    private void OnScoreChange()
    {
        UpdateText();
    }

    private void OnMove()
    {
        UpdateText();
    }

    private void OnGameOver(bool isWin)
    {
        if (isWin)
        {
            winPanel.SetActive(true);
            audioManager.PlaySFX(audioManager.WinSound);
        }
        else
        {
            losePanel.SetActive(true);
            audioManager.PlaySFX(audioManager.LoseSound);
        }
        UpdateText();
    }

    public void OnCloseGameOverPanel()
    {
        audioManager.PlaySFX(audioManager.ButtonSound);

        winPanel.SetActive(false);
        losePanel.SetActive(false);
        
        transition.gameObject.SetActive(true);
        transition.alpha = 0;
        transition.DOFade(1, 0.5f).SetUpdate(UpdateType.Late, false).OnComplete(()=> SceneManager.LoadScene(0));
    }

    private void UpdateText()
    {
        level.text = "Уровень " + (match3.GetLevel() + 1).ToString();
        score.text = match3.GetScore().ToString();
        targetScore.text = match3.GetTargetScore().ToString();
        moves.text = match3.GetMoves().ToString();
    }

}
