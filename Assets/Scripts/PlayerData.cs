using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerData
{
    [DllImport("__Internal")]
    private static extern void UpdateLeaderboardScore(int value);
    [DllImport("__Internal")]
    private static extern void ShowFullscreenAdv();
    
    private int _currentLevel;

    public PlayerData()
    {
        TryLoad();
    }

    public void UnlockNewLevel()
    {
        _currentLevel ++;
        PlayerPrefs.SetInt("Level", _currentLevel);
        Save();

#if UNITY_WEBGL && !UNITY_EDITOR
        GameObject yandexSDK = GameObject.FindGameObjectWithTag("ySDK");
        if (yandexSDK != null)
        {
            UpdateLeaderboardScore(_currentLevel);
            ShowFullscreenAdv();
        }
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
