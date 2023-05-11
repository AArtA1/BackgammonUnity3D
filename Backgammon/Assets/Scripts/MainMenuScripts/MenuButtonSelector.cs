using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Данный класс используется для управления действиями при нажатии одной из основных кнопок.
/// </summary>
public class MenuButtonSelector : MonoBehaviour
{
    private const int gameSceneIndex = 1; // Сейчас только одна сцена поддерживается, но возможно также и расширить

    public void onLocalPlayButtonSelected() {
        BoardManagerV2.againstAI = false; // Даем понять BoardManager, что будет играть человек
        SceneManager.LoadScene(gameSceneIndex); // Загружаем саму сцену
    }

    public void onAIButtonSelected() {
        BoardManagerV2.againstAI = true; // Даем понять BoardManager, что будет играть искусственный интеллект 
        SceneManager.LoadScene(gameSceneIndex); // Загружаем саму сцену
    }

    public void onQuitButtonSelected() {
        Application.Quit(); // Завершаем работу приложения и выходим из нее
    }

}
