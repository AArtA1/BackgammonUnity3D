using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Это класс который хранит значения состояних для игральных шашек
/// </summary>
public class Checker : MonoBehaviour
{
    // Данное поле определяет цвет шашки
    [SerializeField] bool isWhite = false;

    // Применяем несколько разных шейдеров, чтобы пользователь мог определить фишку, которую возможно разыграть дальше
    private const string WHITEHIGHLIGHTSHADERNAME = "Shader Graphs/WhiteCheckerHighlight";
    private const string BLACKHIGHLIGHTSHADERNAME = "Shader Graphs/BlackCheckerHighlight";
    private const string DEFAULTSHADERNAME = "Lightweight Render Pipeline/Lit";
    private Renderer renderer = null;
    private bool clickable = false;

    // Это поле хранит в каком из Pointer находится шашка
    private int currPointePos = -1;

    private void Awake() {
        renderer = GetComponent<Renderer>();
    }

    public bool isCheckerWhite() {
        return isWhite;
    }

    // Меняем шейдер в зависимости от его кликабельности 
    public void changeHighlightChecker(bool toHighlight) {
        if(toHighlight) {

            clickable = true;
            if (isWhite) {
                renderer.material.shader = Shader.Find(WHITEHIGHLIGHTSHADERNAME);
            }
            else {
                renderer.material.shader = Shader.Find(BLACKHIGHLIGHTSHADERNAME);
            }
        }
        else {
            renderer.material.shader = Shader.Find(DEFAULTSHADERNAME);
            clickable = false;
        }
    }

    public bool isClickable() {
        return clickable;
    }

    // Задаем новый Pointer, на который перемещана шашка
    public void setPos(int newPointePos) {
        currPointePos = newPointePos;
    }

    // Получаем значение приватного поля позиции Pointer шашки 
    public int getPos() {
        if(currPointePos == -1) {
            Debug.Log("Error: Should not access checker without assigned pointe position");
        }

        return currPointePos;
    }
}
