using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///Этот класс имеет дело с отдельными Pointer, где он имеет возможность добавлять/удалять шашки и указывать, что их можно переместить
/// </summary>
public class PointeManager : MonoBehaviour {
    private const int MAXCHECKERROW = 6; // Максимальное количество шашек на одной доске
    private const int MAXCHECKERS = 15; // Максимальное количество шашек одного игрока

    // Хранит шейдер изменения
    private const string HIGHLIGHTSHADERNAME = "Shader Graphs/PointeHighlight";
    private const string DEFAULTSHADERNAME = "Lightweight Render Pipeline/Lit";
    private Renderer renderer = null;
    private bool clickable = false;

    private Vector3 initialCheckerPos = Vector3.zero;

    private int currCheckerPos = 0; // Указывает следующее свободное место для игры
    private int pointePos = -1; // Ссылается на целочисленное представление Pointer
    private Checker[] checkers = new Checker[MAXCHECKERS];

    // Вспомогательные поля для иницилизации и ровных границ
    [SerializeField] float initialcheckerXOffset = 0.5f;
    [SerializeField] float initialcheckerYOffset = 0.09f;
    [SerializeField] float initialcheckerZOffset = -0.5f;

    // Определяет расстояние между шашками
    [SerializeField] float xChangeBetweenCheckers = 1f;
    [SerializeField] float yChangeAboveCheckers = 0.21f;

    [SerializeField] float defaultYScale = 1f;
    [SerializeField] float expandedYScale = 1.4f;

    // В начале игры определяет место, куда нужно поставить шашки
    private void Awake() {
        float pointePosX = this.transform.position.x;
        
        float initialCheckerPosX = 0f;

        if (pointePosX > 0) {
            initialCheckerPosX = pointePosX - initialcheckerXOffset;
        }
        else {
            initialCheckerPosX = pointePosX + initialcheckerXOffset;
        }

        float initialCheckerPosY = this.transform.position.y + initialcheckerYOffset;

        float initialCheckerPosZ = this.transform.position.z + initialcheckerZOffset;

        initialCheckerPos = new Vector3(initialCheckerPosX, initialCheckerPosY, initialCheckerPosZ);

        renderer = GetComponent<Renderer>();
    }

    // Добавляет заданную шашку к pointer, вычисляя правильную позицию для ее перемещения
    public void addChecker(Checker checker) {
        checkers[currCheckerPos] = checker;

        float xOffset = xChangeBetweenCheckers * (currCheckerPos % MAXCHECKERROW);
        float newCheckerX = 0f;

        if(initialCheckerPos.x > 0) {
            newCheckerX = initialCheckerPos.x - xOffset;
        }
        else {
            newCheckerX = initialCheckerPos.x + xOffset;
        }

        float yOffset = 0f;

        if(currCheckerPos >= (2 * MAXCHECKERROW)) {
            yOffset = yChangeAboveCheckers * 2;
        }
        else if(currCheckerPos >= MAXCHECKERROW) {
            yOffset = yChangeAboveCheckers;
        }

        float newCheckerY = initialCheckerPos.y + yOffset;

        checker.transform.position = new Vector3(newCheckerX, newCheckerY, initialCheckerPos.z);

        currCheckerPos++;
    }

    // Возвращает последнюю шашку, содержащуюся в Pointer, и избавляется от нее
    public Checker removeChecker() {
        currCheckerPos--;
        Checker removedChecker = checkers[currCheckerPos];
        checkers[currCheckerPos] = null;

        return removedChecker;
    }

    // Меняет шейдер, чтобы дать понять пользователю о возможности взаимодействия c Pointer
    public void changeHighlightPointe(bool toHighlight) {
        if (toHighlight) {
            renderer.material.shader = Shader.Find(HIGHLIGHTSHADERNAME);
            this.transform.localScale = new Vector3(this.transform.localScale.x, expandedYScale, this.transform.localScale.z);
            clickable = true;
        }
        else {
            renderer.material.shader = Shader.Find(DEFAULTSHADERNAME);
            this.transform.localScale = new Vector3(this.transform.localScale.x, defaultYScale, this.transform.localScale.z);
            clickable = false;
        }
    }

    // Возвращает, можно ли в данный момент переместить Pointer
    public bool isClickable() {
        return clickable;
    }

    public void highlightLastChecker(bool toHighlight) {
        checkers[currCheckerPos - 1].changeHighlightChecker(true);
    }

    // Setter для Pointer
    public void setPointePos(int pos) {
        pointePos = pos;
    }

    // Getter для Pointer
    public int getPos() {
        if (pointePos == -1) {
            Debug.Log("Error: Should not access pointe without assigned pointe position");
        }

        return pointePos;
    }
}
