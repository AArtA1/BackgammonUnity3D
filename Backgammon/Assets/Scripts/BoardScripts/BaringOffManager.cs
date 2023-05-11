using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Этот класс занимается перемещением шашек на бар и с бара
public class BaringOffManager : MonoBehaviour
{
    private const string HIGHLIGHTSHADERNAME = "Shader Graphs/PointeHighlight";
    private const string DEFAULTSHADERNAME = "Lightweight Render Pipeline/Lit";

    [SerializeField] Vector3 whiteCheckerPos = Vector3.zero;
    [SerializeField] Vector3 blackCheckerPos = Vector3.zero;

    [SerializeField] float checkerYOffset = 0.5f;

    [SerializeField] Renderer renderer = null;

    private int currWhitePos = 0;
    private int currBlackPos = 0;

    private bool clickable = false;

    // Добавляем шашку на бар 
    public void addCheckerBaringOff(Checker checker) {
        if (checker.isCheckerWhite()) {
            float adjustedYPos = whiteCheckerPos.y + (checkerYOffset * currWhitePos);
            checker.transform.position = new Vector3(whiteCheckerPos.x, adjustedYPos, whiteCheckerPos.z);

            currWhitePos++;
        }
        else {
            float adjustedYPos = blackCheckerPos.y + (checkerYOffset * currBlackPos);
            checker.transform.position = new Vector3(blackCheckerPos.x, adjustedYPos, blackCheckerPos.z);

            currBlackPos++;
        }
    }
    

    // Меняем шейдер шашки
    public void changeHighlightBaringOff(bool toHighlight) {
        if (toHighlight) {
            renderer.material.shader = Shader.Find(HIGHLIGHTSHADERNAME);
            clickable = true;
        }
        else {
            renderer.material.shader = Shader.Find(DEFAULTSHADERNAME);
            clickable = false;
        }
    }

    public bool isClickable() {
        return clickable;
    }
}
