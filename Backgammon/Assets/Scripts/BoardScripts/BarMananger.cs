using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Этот класс фокусируется на управлении шашками на панели и имеет дело с добавлением/удалением/разрешением перемещения шашек
/// </summary>
public class BarMananger : MonoBehaviour
{
    private const int MAXCHECKERS = 15;

    // Сохраняет позиции, куда добавлять шашки
    [SerializeField] Vector3 whiteCheckerPos = Vector3.zero;
    [SerializeField] Vector3 blackCheckerPos = Vector3.zero;

    // Создает смещение между каждой добавленной на панель шашкой
    [SerializeField] float checkerYOffset = 0.5f;

    private Checker[] whiteCheckers = new Checker[MAXCHECKERS];
    private Checker[] blackCheckers = new Checker[MAXCHECKERS];

    private bool whiteOnBar = false;
    private bool blackOnBar = false;

    // Действует как стек, где он указывает на самую высокую шашку в стеке шашечной панели, так что одна из них будет перемещена первой.
    private int currWhitePos = 0;
    private int currBlackPos = 0;

    // Добавляет шашку в бар 
    public void addCheckerOnBar(Checker checker) {
        if(checker.isCheckerWhite()) {
            whiteCheckers[currWhitePos] = checker;

            float adjustedYPos = whiteCheckerPos.y + (checkerYOffset * currWhitePos);
            checker.transform.position = new Vector3(whiteCheckerPos.x, adjustedYPos, whiteCheckerPos.z);

            currWhitePos++;
            whiteOnBar = true;
        }
        else {
            blackCheckers[currBlackPos] = checker;

            float adjustedYPos = blackCheckerPos.y + (checkerYOffset * currBlackPos);
            checker.transform.position = new Vector3(blackCheckerPos.x, adjustedYPos, blackCheckerPos.z);

            currBlackPos++;
            blackOnBar = true;
        }
    }

    // Удаляет и возвращает самую верхнюю шашку из бара, учитывая, какую шашку цвета удалить
    public Checker removeCheckerOnBar(bool isWhiteChecker) {
        if(isWhiteChecker) {
            if(whiteOnBar) {
                currWhitePos--;
                Checker removedChecker = whiteCheckers[currWhitePos];
                whiteCheckers[currWhitePos] = null;

                if(currWhitePos == 0) {
                    whiteOnBar = false;
                }

                return removedChecker;
            }
            else {
                Debug.LogError("Cannot remove checker from empty bar");
                return null;
            }
        }
        else {
            if(blackOnBar) {
                currBlackPos--;
                Checker removedChecker = blackCheckers[currBlackPos];
                blackCheckers[currBlackPos] = null;

                if (currBlackPos == 0) {
                    blackOnBar = false;
                }

                return removedChecker;
            }
            else {
                Debug.LogError("Cannot remove checker from empty bar");
                return null;
            }
        }
    }

    // Применяет шейдер, чтобы указать, что его можно выбрать.
    public void highlightChecker(bool whiteToHighlight) {
        if(whiteToHighlight) {
            whiteCheckers[currWhitePos - 1].changeHighlightChecker(true);
        }
        else {
            blackCheckers[currBlackPos - 1].changeHighlightChecker(true);
        }
    }
}
