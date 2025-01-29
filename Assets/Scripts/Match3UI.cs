using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Match3UI : MonoBehaviour
{
    [SerializeField] private Match3 _match3;
    [SerializeField] private TMP_Text _level;
    [SerializeField] private TMP_Text _score;
    [SerializeField] private TMP_Text _targetScore;
    [SerializeField] private TMP_Text _moves;
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private GameObject _losePanel;
    [SerializeField] private CanvasGroup _transition;
    [SerializeField] private GameObject _glassIcon;
    [SerializeField] private AudioManager _audioManager;

    private void Awake()
    {
        _glassIcon.SetActive(false);
        _transition.gameObject.SetActive(true);
        _transition.DOFade(0, 0.5f).SetUpdate(UpdateType.Late, false).OnComplete(()=> _transition.gameObject.SetActive(false));

        _match3.OnScoreChange += OnScoreChange;
        _match3.OnSetLevel += OnSetLevel;
        _match3.OnMove += OnMove;
        _match3.OnGameOver += OnGameOver;
    }

    private void OnSetLevel()
    {
        if (_match3.GetTarget() == TargetType.glass)
            _glassIcon.SetActive(true);

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
            _winPanel.SetActive(true);
            _audioManager.PlaySFX(_audioManager.WinSound);
        }
        else
        {
            _losePanel.SetActive(true);
            _audioManager.PlaySFX(_audioManager.LoseSound);
        }
        UpdateText();
    }

    public void OnCloseGameOverPanel()
    {
        _audioManager.PlaySFX(_audioManager.ButtonSound);

        _winPanel.SetActive(false);
        _losePanel.SetActive(false);
        
        _transition.gameObject.SetActive(true);
        _transition.alpha = 0;
        _transition.DOFade(1, 0.5f).SetUpdate(UpdateType.Late, false).OnComplete(()=> SceneManager.LoadScene(0));
    }

    private void UpdateText()
    {
        _level.text = "Уровень " + (_match3.GetLevel() + 1).ToString();
        _score.text = _match3.GetScore().ToString();
        _targetScore.text = _match3.GetTargetScore().ToString();
        _moves.text = _match3.GetMoves().ToString();
    }
}
