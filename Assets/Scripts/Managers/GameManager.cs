﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float fadeDuration;
    private CameraFade fadePanel;
    private Image fadeImage;
    private string _currentLevel;
    private bool _gameEnded;

	void Start ()
    {
        _currentLevel = "";
        DontDestroyOnLoad(this.gameObject); 
        EventManager.Listen("level_complete", LevelComplete);
        EventManager.Listen("die_player", DiePlayer);
        EventManager.Listen("in_tampon", RestartScene);
        EventManager.Listen("play_menu_button", Play);
        EventManager.Listen("back_to_menu", BackToMenu);
        EventManager.Listen("restart_current", RestartLevel);

        SceneManager.LoadScene("Menu");

        if(PlayerPrefs.GetInt("game_ended") == 1)
        {
            _gameEnded = true;
        }
        else
        {
            _gameEnded = false;
        }
    }

    void OnDisable()
    {
        EventManager.Remove("level_complete", LevelComplete);
        EventManager.Remove("die_player", DiePlayer);
        EventManager.Remove("in_tampon", RestartScene);
        EventManager.Remove("play_menu_button", Play);
        EventManager.Remove("back_to_menu", BackToMenu);
        EventManager.Remove("restart_current", RestartLevel);
    }

    private void DiePlayer(object[] args)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutOnQuit(Color.black, "TamponRestart", 0.5f));
    }

    private void RestartLevel(object[] args)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutOnQuit(Color.black, "TamponRestart", fadeDuration));
    }

    private void RestartScene(object[] args)
    {
        Debug.Log("restart");   
        StopAllCoroutines();
        StartCoroutine(FadeOutOnQuit(Color.black, _currentLevel, fadeDuration));
    }

    private void Play(object[] args)
    {
        _currentLevel = (string)args[0];
        StopAllCoroutines();
        StartCoroutine(FadeOutOnQuit(Color.white, (string)args[0], fadeDuration));
    }

    private void BackToMenu(object[] args)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutOnQuit(Color.black, "Menu", fadeDuration));
    }

    private void LevelComplete(object[] args)
    {
        _currentLevel = "level_" + (int)args[0];
        if ((int)args[0] == 5)
        {
            PlayerPrefs.SetInt("game_ended", 1);
            _gameEnded = true;
            PlayerPrefs.Save();
        }
        string key = "" + (int)args[0];
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        string toFade = "level_" + ((int)args[0] + 1);
        StartCoroutine(FadeOutOnQuit(Color.black, toFade, fadeDuration));
    }

    private IEnumerator FadeOutOnQuit(Color fadeColor, string sceneOnComplete, float duration)
    {
        if (fadePanel == null)
        {
            fadePanel = FindObjectOfType<CameraFade>();
            if(fadePanel == null)
            {
                Debug.LogError("No FadePanel in Scene");
                SceneManager.LoadScene(sceneOnComplete);
                yield return new WaitForEndOfFrame();
                if (_gameEnded)
                {
                    EventManager.Send("game_ended");
                }
                StartCoroutine(FadeInOnStart(fadeColor, duration));
                yield break;
            }
        }
        float timer = 0;
        while (timer < duration)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, timer / duration);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        SceneManager.LoadScene(sceneOnComplete);
        yield return new WaitForEndOfFrame();
        if(_gameEnded)
        {
            EventManager.Send("game_ended");
        }
        StartCoroutine(FadeInOnStart(fadeColor, duration));
    }

    private IEnumerator FadeInOnStart(Color fadeColor, float duration)
    {
        if(fadePanel == null)
        {
            fadePanel = FindObjectOfType<CameraFade>();
            if (fadePanel == null)
            {
                Debug.LogError("No FadePanel in Scene");
                yield break;
            }
            fadeImage = fadePanel.gameObject.GetComponent<Image>();
        }
        float timer = 0;
        while (timer < fadeDuration)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1 - timer / fadeDuration);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
    