using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerData
{
    [DllImport("__Internal")]
    private static extern void UpdateLeaderboardScore(int value);
    [DllImport("__Internal")]
    private static extern void ShowFullscreenAdv();
    
    //public event Action LevelChanged;
    private int _currentLevel;

    public PlayerData()
    {
        //ClearData();
        TryLoad();
    }

    public void UnlockNewLevel()
    {
        _currentLevel ++;
        PlayerPrefs.SetInt("Level", _currentLevel);
        Save();

#if UNITY_WEBGL && !UNITY_EDITOR
        UpdateLeaderboardScore(_currentLevel);

        ShowFullscreenAdv();
#endif
            
    }

    public int GetCurrentLevel()
    {
        return _currentLevel;
    }

    public void TryLoad()
    {
        _currentLevel = PlayerPrefs.GetInt("Level", 0);
    }

    public void Save()
    {
        if (_currentLevel >= PlayerPrefs.GetInt("Level", 0))
            PlayerPrefs.Save();
    }

    public void ClearData()
    {
        PlayerPrefs.DeleteAll();
    }
}
