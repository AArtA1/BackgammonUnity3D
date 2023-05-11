using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Данный класс контролирует большую часть событий, происходящих на игровом поле
/// </summary>
public class BoardManagerV2 : MonoBehaviour {
    private const int MAINSCREENBUILDINDEX = 0;
    
    private const int MAXPOINTES = 24;
    private const int MAXCHECKERS = 15;

    private const string WHITEWON = "White wins!";
    private const string BLACKWON = "Black wins!";


    [SerializeField] PointeManager[] pointes = new PointeManager[MAXPOINTES];
    [SerializeField] Checker[] whiteCheckers = new Checker[MAXCHECKERS];
    [SerializeField] Checker[] blackCheckers = new Checker[MAXCHECKERS];
    [SerializeField] DiceManager dice = null;
    [SerializeField] BarMananger bar = null;
    [SerializeField] BaringOffManager baringOff = null;
    [SerializeField] MonteCarloAI AI = null;

    [SerializeField] Button winInformation;
    [SerializeField] Text winInformationText;
    [SerializeField] Button AIMoveInformation;
    [SerializeField] Text AIMoveInformationText;

    [SerializeField] float waitForDiceRollTime = 2f;
    [SerializeField] float waitFinishMove = 2f;

    [SerializeField] float selectedPieceYOffset = 2f;

    public static bool againstAI = false;

    private enum turnPhase { StartGame, RollDice, WhiteTurn, BlackTurn };
    private enum gamePhase { Regular, BaringOff };

    private turnPhase currGamePhase;
    private gamePhase whiteCheckerPhase;
    private gamePhase blackCheckerPhase;

    private int movesLeftForTurn = -1;

    private bool usedRoll1 = false;
    private bool usedRoll2 = false;
    private bool canUseBothMoves = true;

    private int[] rolledValues = new int[2];
   

    private Vector3 selectedCheckerOrigPos = new Vector3(0f, 0f, 0f);
    private Checker selectedChecker = null;
    private int selectedCheckerOrigBoardPos = -1;

    private int[] board = new int[MAXPOINTES + 4];

    private List<KeyValuePair<int, int>> legalMoves = new List<KeyValuePair<int, int>>();


    private void Start() {
        dice.startToThrowDices(); 
        arrangeBoard(); 
        assignPointePos(); 

        
        if(againstAI) {
            AIMoveInformation.gameObject.SetActive(true);
        }

       
        currGamePhase = turnPhase.StartGame;
        whiteCheckerPhase = gamePhase.Regular;
        blackCheckerPhase = gamePhase.Regular;

        
        StartCoroutine(determinePlayerOrder());
    }

    IEnumerator determinePlayerOrder() {
        rolledValues = dice.throwDice(false); 

        yield return new WaitForSeconds(waitForDiceRollTime); 

        
        if (rolledValues[0] > rolledValues[1]) {
            currGamePhase = turnPhase.WhiteTurn;

            movesLeftForTurn = 2;
            usedRoll1 = false;
            usedRoll2 = false;
            getLegalMoves(rolledValues[0], rolledValues[1]); 
        }
        else {
            currGamePhase = turnPhase.BlackTurn;
            if(againstAI) {
                StartCoroutine(doAIMove());
                currGamePhase = turnPhase.WhiteTurn;             }
            else {
                movesLeftForTurn = 2;
                usedRoll1 = false;
                usedRoll2 = false;
                getLegalMoves(rolledValues[0], rolledValues[1]);
            }
        }   
    }

    private void getLegalMoves(int roll1, int roll2) {
        List<KeyValuePair<int, int>> movesRoll1 = new List<KeyValuePair<int, int>>();
        List<KeyValuePair<int, int>> movesRoll2 = new List<KeyValuePair<int, int>>();
        legalMoves.Clear();

        if(currGamePhase == turnPhase.WhiteTurn) {
            if(whiteCheckerPhase == gamePhase.BaringOff) {
                if(!usedRoll1) {
                    movesRoll1 = getBarringOffMoves(roll1, roll2, true);
                }
                if(!usedRoll2) {
                    movesRoll2 = getBarringOffMoves(roll2, roll1, true);
                }
            }
            else {
                if(!usedRoll1) {
                    movesRoll1 = getRegularMoves(roll1, roll2, true);
                }
                if(!usedRoll2) {
                    movesRoll2 = getRegularMoves(roll2, roll1, true);
                }
                
                if(!usedRoll1 && !usedRoll2) {
                    int numRoll1Moves = movesRoll1.Count;
                    int numRoll2Moves = movesRoll2.Count;

                    if (numRoll1Moves == 0 && numRoll2Moves > 1) {
                        movesRoll2 = refineRegularMoves(movesRoll2, roll1, true);
                    }
                    else if (numRoll2Moves == 0 && numRoll1Moves > 1) {
                        movesRoll1 = refineRegularMoves(movesRoll1, roll2, true);
                    }
                }
            }
        }
        else {
            if(blackCheckerPhase == gamePhase.BaringOff) {
                if(!usedRoll1) {
                    movesRoll1 = getBarringOffMoves(roll1, roll2, false);
                }
                if(!usedRoll2) {
                    movesRoll2 = getBarringOffMoves(roll2, roll1, false);
                }
            }
            else {
                if(!usedRoll1) {
                    movesRoll1 = getRegularMoves(roll1, roll2, false);
                }
                if(!usedRoll2) {
                    movesRoll2 = getRegularMoves(roll2, roll1, false);
                }
        
                if(!usedRoll1 && !usedRoll2) {
                    int numRoll1Moves = movesRoll1.Count;
                    int numRoll2Moves = movesRoll2.Count;

                    if (numRoll1Moves == 0 && numRoll2Moves > 1) {
                        movesRoll2 = refineRegularMoves(movesRoll2, roll1, false);
                    }
                    else if (numRoll2Moves == 0 && numRoll1Moves > 1) {
                        movesRoll1 = refineRegularMoves(movesRoll1, roll2, false);
                    }
                }
            }
        }

        foreach(var move in movesRoll1) {
            legalMoves.Add(move);
        }

        foreach(var move in movesRoll2) {
            legalMoves.Add(move);
        }

        if(legalMoves.Count == 0) {
            dice.startToThrowDices();

            if (currGamePhase == turnPhase.WhiteTurn) {
                currGamePhase = turnPhase.BlackTurn;
                if(againstAI) {
                    rolledValues = dice.throwDice(true);
                    StartCoroutine(doAIMove());
                    currGamePhase = turnPhase.WhiteTurn;
                }
            }
            else {
                currGamePhase = turnPhase.WhiteTurn;
            }
        }
        else {
            highlightCheckersMoveable();
        }
    }

    private void highlightCheckersMoveable() {
        foreach (var move in legalMoves) {
            if (move.Key >= MAXPOINTES) {
                if (currGamePhase == turnPhase.WhiteTurn) {
                    bar.highlightChecker(true);
                }
                else {
                    bar.highlightChecker(false);
                }
            }
            else {
                pointes[move.Key].highlightLastChecker(true);
            }
        }
    }

    private List<KeyValuePair<int, int>> getRegularMoves(int roll, int nonUsedRoll, bool isWhite) {
        List<KeyValuePair<int, int>> moves = new List<KeyValuePair<int, int>>();

        if(isWhite) {
            if (board[24] > 0) {
                if (board[MAXPOINTES - roll] > -2) {
                    if (board[24] == 1 && nonUsedRoll > roll && board[MAXPOINTES - nonUsedRoll] > -2 && !anyValidMoves(roll, nonUsedRoll, 24, true, true)) {
                    }
                    else {
                        moves.Add(new KeyValuePair<int, int>(24, MAXPOINTES - roll));
                    }
                }
            }
            else {
                for (int i = MAXPOINTES; i >= roll; i--) {
                    if (board[i] > 0 && board[i - roll] > -2) {
                        if (nonUsedRoll > roll && (i - nonUsedRoll) >= 0 && board[i - nonUsedRoll] > -2 && !anyValidMoves(roll, nonUsedRoll, i, false, true)) {
                        }
                        else {
                            moves.Add(new KeyValuePair<int, int>(i, i - roll));
                        }
                    }
                }
            }
        }
        else {
            if (board[25] < 0) {
                if (board[roll - 1] < 2) {
                    if (board[25] == -1 && nonUsedRoll > roll && board[nonUsedRoll - 1] < 2 && !anyValidMoves(roll, nonUsedRoll, 25, true, false)) {
                    }
                    else {
                        moves.Add(new KeyValuePair<int, int>(25, roll - 1));
                    }
                }
            }
            else {
                for (int i = (MAXPOINTES - roll - 1); i >= 0; i--) {
                    if (board[i] < 0 && board[i + roll] < 2) {
                        if (nonUsedRoll > roll && (i + nonUsedRoll) < MAXPOINTES && board[i + nonUsedRoll] < 2 && !anyValidMoves(roll, nonUsedRoll, i, false, false)) {
                        }
                        else {
                            moves.Add(new KeyValuePair<int, int>(i, i + roll));
                        }
                    }
                }
            }
        }

        return moves;
    }

    private List<KeyValuePair<int, int>> refineRegularMoves(List<KeyValuePair<int, int>> legalMoves, int unusedRoll, bool isWhite) {

        List<KeyValuePair<int, int>> refinedMoves = new List<KeyValuePair<int, int>>();

        if(isWhite) {
            foreach (var move in legalMoves) {
                int pointeToConsider = move.Value - unusedRoll;
                if (pointeToConsider >= 0 && board[move.Value - unusedRoll] > -2) {
                    refinedMoves.Add(move);
                }
            }
        }
        else {
            foreach (var move in legalMoves) {
                int pointeToConsider = move.Value + unusedRoll;
                if (pointeToConsider < MAXPOINTES && board[move.Value + unusedRoll] < 2) {
                    refinedMoves.Add(move);
                }
            }
        }

        if (refinedMoves.Count > 0) {
            return refinedMoves;
        }

        return legalMoves;
    }

    private bool anyValidMoves(int smallerRoll, int largerRoll, int origPos, bool barCase, bool isWhite) {
        bool foundLegalMove = false;

        if(isWhite) {
            board[origPos]--;

            if (barCase) {
                board[MAXPOINTES - smallerRoll]++;
            }
            else {
                board[origPos - smallerRoll]++;
            }

            if(whiteCanBearOff()) {
                foundLegalMove = true;
            }
            else {
                for (int i = MAXPOINTES; i >= largerRoll; i--) {
                    if (board[i] > 0 && board[i - largerRoll] > -2) {
                        foundLegalMove = true;
                        break;
                    }
                }
            }

            board[origPos]++;
            if (barCase) {
                board[MAXPOINTES - smallerRoll]--;
            }
            else {
                board[origPos - smallerRoll]--;
            }
        }
        else {
            board[origPos]++;

            if (barCase) {
                board[smallerRoll - 1]--;
            }
            else {
                board[origPos + smallerRoll]--;
            }

            if(blackCanBearOff()) {
                foundLegalMove = true;
            }
            else {
                for (int i = 0; i < MAXPOINTES - largerRoll; i++) {
                    if (board[i] < 0 && board[i + largerRoll] < 2) {
                        foundLegalMove = true;
                        break;
                    }
                }
            }

            board[origPos]--;
            if (barCase) {
                board[smallerRoll - 1]++;
            }
            else {
                board[origPos + smallerRoll]++;
            }
        }

        return foundLegalMove;
    }

    private List<KeyValuePair<int, int>> getBarringOffMoves(int roll, int nonUsedRoll, bool isWhite) {
        List<KeyValuePair<int, int>> moves = new List<KeyValuePair<int, int>>();

        if(isWhite) {
            if (board[roll - 1] > 0) {
                moves.Add(new KeyValuePair<int, int>(roll - 1, 26));
            }
            else {
                bool foundValidChecker = false;
                for (int i = 5; i > (roll - 1); i--) {
                    if (board[i] > 0 && board[i - roll] > -2) {

                        if (nonUsedRoll > 0 && (i - nonUsedRoll) == -1) {
                        }
                        else {
                            moves.Add(new KeyValuePair<int, int>(i, i - roll));
                            foundValidChecker = true;
                        }
                    }
                }

                if (!foundValidChecker) {
                    for (int i = roll - 2; i >= 0; i--) {
                        if (board[i] > 0) {

                            if (nonUsedRoll > 0 && (i - nonUsedRoll) == -1) {
                            }
                            else {
                                moves.Add(new KeyValuePair<int, int>(i, 26));
                                break;
                            }
                        }
                    }
                }
            }
        }
        else {
            if (board[MAXPOINTES - roll] < 0) {
                moves.Add(new KeyValuePair<int, int>(MAXPOINTES - roll, 27));
            }
            else {
                bool foundValidChecker = false;
                for (int i = 18; i < (MAXPOINTES - roll); i++) {
                    if (board[i] < 0 && board[i + roll] < 2) {

                        if (nonUsedRoll > 0 && (i + nonUsedRoll) == MAXPOINTES) {
                        }
                        else {
                            moves.Add(new KeyValuePair<int, int>(i, i + roll));
                            foundValidChecker = true;
                        }
                    }
                }

                if (!foundValidChecker) {
                    for (int i = MAXPOINTES - roll + 1; i < MAXPOINTES; i++) {
                        if (board[i] < 0) {

                            if (nonUsedRoll > 0 && (i + nonUsedRoll) == MAXPOINTES) {
                            }
                            else {
                                moves.Add(new KeyValuePair<int, int>(i, 27));
                                break;
                            }
                        }
                    }
                }
            }
        }

        return moves;
    }

    private void removeCheckerHighlights() {
        for(int i = 0; i < MAXCHECKERS; i++) {
            whiteCheckers[i].changeHighlightChecker(false);
            blackCheckers[i].changeHighlightChecker(false);
        }
    }

    private void arrangeBoard() {
        pointes[0].addChecker(blackCheckers[0]);
        pointes[0].addChecker(blackCheckers[1]);
        board[0] = -2;
        blackCheckers[0].setPos(0);
        blackCheckers[1].setPos(0);

        pointes[5].addChecker(whiteCheckers[0]);
        pointes[5].addChecker(whiteCheckers[1]);
        pointes[5].addChecker(whiteCheckers[2]);
        pointes[5].addChecker(whiteCheckers[3]);
        pointes[5].addChecker(whiteCheckers[4]);
        board[5] = 5;
        whiteCheckers[0].setPos(5);
        whiteCheckers[1].setPos(5);
        whiteCheckers[2].setPos(5);
        whiteCheckers[3].setPos(5);
        whiteCheckers[4].setPos(5);

        pointes[7].addChecker(whiteCheckers[5]);
        pointes[7].addChecker(whiteCheckers[6]);
        pointes[7].addChecker(whiteCheckers[7]);
        board[7] = 3;
        whiteCheckers[5].setPos(7);
        whiteCheckers[6].setPos(7);
        whiteCheckers[7].setPos(7);

        pointes[11].addChecker(blackCheckers[2]);
        pointes[11].addChecker(blackCheckers[3]);
        pointes[11].addChecker(blackCheckers[4]);
        pointes[11].addChecker(blackCheckers[5]);
        pointes[11].addChecker(blackCheckers[6]);
        board[11] = -5;
        blackCheckers[2].setPos(11);
        blackCheckers[3].setPos(11);
        blackCheckers[4].setPos(11);
        blackCheckers[5].setPos(11);
        blackCheckers[6].setPos(11);

        pointes[12].addChecker(whiteCheckers[8]);
        pointes[12].addChecker(whiteCheckers[9]);
        pointes[12].addChecker(whiteCheckers[10]);
        pointes[12].addChecker(whiteCheckers[11]);
        pointes[12].addChecker(whiteCheckers[12]);
        board[12] = 5;
        whiteCheckers[8].setPos(12);
        whiteCheckers[9].setPos(12);
        whiteCheckers[10].setPos(12);
        whiteCheckers[11].setPos(12);
        whiteCheckers[12].setPos(12);

        pointes[16].addChecker(blackCheckers[7]);
        pointes[16].addChecker(blackCheckers[8]);
        pointes[16].addChecker(blackCheckers[9]);
        board[16] = -3;
        blackCheckers[7].setPos(16);
        blackCheckers[8].setPos(16);
        blackCheckers[9].setPos(16);

        pointes[18].addChecker(blackCheckers[10]);
        pointes[18].addChecker(blackCheckers[11]);
        pointes[18].addChecker(blackCheckers[12]);
        pointes[18].addChecker(blackCheckers[13]);
        pointes[18].addChecker(blackCheckers[14]);
        board[18] = -5;
        blackCheckers[10].setPos(18);
        blackCheckers[11].setPos(18);
        blackCheckers[12].setPos(18);
        blackCheckers[13].setPos(18);
        blackCheckers[14].setPos(18);

        pointes[23].addChecker(whiteCheckers[13]);
        pointes[23].addChecker(whiteCheckers[14]);
        board[23] = 2;
        whiteCheckers[13].setPos(23);
        whiteCheckers[14].setPos(23);
    }

    private void makeNearBarringOffBoard() {
        pointes[1].addChecker(whiteCheckers[0]);
        board[1] = 1;
        whiteCheckers[0].setPos(1);

        pointes[2].addChecker(whiteCheckers[1]);
        pointes[2].addChecker(whiteCheckers[2]);
        board[2] = 2;
        whiteCheckers[1].setPos(2);
        whiteCheckers[2].setPos(2);

        pointes[3].addChecker(blackCheckers[0]);
        pointes[3].addChecker(blackCheckers[1]);
        board[3] = -2;
        blackCheckers[0].setPos(3);
        blackCheckers[1].setPos(3);

        pointes[4].addChecker(whiteCheckers[3]);
        pointes[4].addChecker(whiteCheckers[4]);
        pointes[4].addChecker(whiteCheckers[5]);
        pointes[4].addChecker(whiteCheckers[6]);
        pointes[4].addChecker(whiteCheckers[7]);
        board[4] = 5;
        whiteCheckers[3].setPos(4);
        whiteCheckers[4].setPos(4);
        whiteCheckers[5].setPos(4);
        whiteCheckers[6].setPos(4);
        whiteCheckers[7].setPos(4);

        pointes[5].addChecker(whiteCheckers[8]);
        pointes[5].addChecker(whiteCheckers[9]);
        pointes[5].addChecker(whiteCheckers[10]);
        pointes[5].addChecker(whiteCheckers[11]);
        pointes[5].addChecker(whiteCheckers[12]);
        pointes[5].addChecker(whiteCheckers[13]);
        board[5] = 6;
        whiteCheckers[8].setPos(5);
        whiteCheckers[9].setPos(5);
        whiteCheckers[10].setPos(5);
        whiteCheckers[11].setPos(5);
        whiteCheckers[12].setPos(5);
        whiteCheckers[13].setPos(5);

        pointes[7].addChecker(whiteCheckers[14]);
        board[7] = 1;
        whiteCheckers[14].setPos(7);

        pointes[18].addChecker(blackCheckers[2]);
        pointes[18].addChecker(blackCheckers[3]);
        pointes[18].addChecker(blackCheckers[4]);
        pointes[18].addChecker(blackCheckers[5]);
        pointes[18].addChecker(blackCheckers[6]);
        board[18] = -5;
        blackCheckers[2].setPos(18);
        blackCheckers[3].setPos(18);
        blackCheckers[4].setPos(18);
        blackCheckers[5].setPos(18);
        blackCheckers[6].setPos(18);

        pointes[19].addChecker(blackCheckers[7]);
        pointes[19].addChecker(blackCheckers[8]);
        board[19] = -2;
        blackCheckers[7].setPos(19);
        blackCheckers[8].setPos(19);

        pointes[20].addChecker(blackCheckers[9]);
        board[20] = -1;
        blackCheckers[9].setPos(20);

        pointes[21].addChecker(blackCheckers[10]);
        board[21] = -1;
        blackCheckers[10].setPos(21);

        pointes[22].addChecker(blackCheckers[11]);
        pointes[22].addChecker(blackCheckers[12]);
        pointes[22].addChecker(blackCheckers[13]);
        board[22] = -3;
        blackCheckers[11].setPos(22);
        blackCheckers[12].setPos(22);
        blackCheckers[13].setPos(22);

        pointes[23].addChecker(blackCheckers[14]);
        board[23] = -1;
        blackCheckers[14].setPos(23);
    }

    private void assignPointePos() {
        for(int i = 0; i < pointes.Length; i++) {
            pointes[i].setPointePos(i);
        }
    }

    private void Update() {
        if (Input.GetMouseButtonUp(0) && selectedChecker) {
            releaseSelectedChecker();
        }

        if (selectedChecker) {
            updateSelectedCheckerPos();
        }

        if (Input.GetMouseButtonDown(0)) {
            handleClickAction();
        }

        if (Input.GetMouseButtonDown(1) && selectedChecker) {
            resetSelectedChecker();
        }
    }

    private void releaseSelectedChecker() {
        RaycastHit[] hits;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        hits = Physics.RaycastAll(ray);
        bool clickedPointe = false;
        int newPointePos = 0;

        for (int i = 0; i < hits.Length; i++) {
            PointeManager pointeSelected = hits[i].transform.GetComponent<PointeManager>();
            BaringOffManager clickedBaringOff = hits[i].transform.GetComponent<BaringOffManager>();

            if (pointeSelected && pointeSelected.isClickable()) {
                clickedPointe = true;

                newPointePos = pointeSelected.getPos();
                if (currGamePhase == turnPhase.WhiteTurn) {
                    if (board[newPointePos] == -1) {
                        blackCheckerPhase = gamePhase.Regular;
                        Checker removedChecker = pointes[newPointePos].removeChecker();
                        removedChecker.setPos(25);
                        bar.addCheckerOnBar(removedChecker);
                        board[newPointePos] = 0;
                        board[25]--;
                    }

                    if (board[24] > 0) {
                        Checker removedChecker = bar.removeCheckerOnBar(true);
                        removedChecker.setPos(newPointePos);
                        pointes[newPointePos].addChecker(removedChecker);
                        selectedCheckerOrigBoardPos = 24;
                        board[24]--;
                    }
                    else {
                        Checker removedChecker = pointes[selectedCheckerOrigBoardPos].removeChecker();
                        removedChecker.setPos(newPointePos);
                        pointes[newPointePos].addChecker(removedChecker);
                        board[selectedCheckerOrigBoardPos]--;
                    }

                    board[newPointePos]++;
                }
                else if (currGamePhase == turnPhase.BlackTurn) {
                    if (board[newPointePos] == 1) {
                        whiteCheckerPhase = gamePhase.Regular;
                        Checker removedChecker = pointes[newPointePos].removeChecker();
                        removedChecker.setPos(24);
                        bar.addCheckerOnBar(removedChecker);
                        board[newPointePos] = 0;
                        board[24]++;
                    }

                    if (board[25] < 0) {
                        Checker removedChecker = bar.removeCheckerOnBar(false);
                        removedChecker.setPos(newPointePos);
                        pointes[newPointePos].addChecker(removedChecker);
                        selectedCheckerOrigBoardPos = -1;
                        board[25]++;
                    }
                    else {
                        Checker removedChecker = pointes[selectedCheckerOrigBoardPos].removeChecker();
                        removedChecker.setPos(newPointePos);
                        pointes[newPointePos].addChecker(removedChecker);
                        board[selectedCheckerOrigBoardPos]++;
                    }

                    board[newPointePos]--;
                }
            }
            else if (clickedBaringOff && clickedBaringOff.isClickable()) {
                clickedPointe = true;
                Checker removedChecker = pointes[selectedCheckerOrigBoardPos].removeChecker();
                baringOff.addCheckerBaringOff(removedChecker);

                if (currGamePhase == turnPhase.WhiteTurn) {
                    removedChecker.setPos(26);
                    board[selectedCheckerOrigBoardPos]--;
                    board[26]++;
                    newPointePos = -1;
                }
                else {
                    removedChecker.setPos(27);
                    board[selectedCheckerOrigBoardPos]++;
                    board[27]--;
                    newPointePos = 24;
                }

                baringOff.changeHighlightBaringOff(false);
            }
        }

        if (clickedPointe) {
            removeCheckerHighlights();
            movesLeftForTurn--;

            if (currGamePhase == turnPhase.WhiteTurn) {
                if (board[26] == MAXCHECKERS) {
                    StartCoroutine(finishGame(true));
                }

                if (whiteCanBearOff()) {
                    whiteCheckerPhase = gamePhase.BaringOff;
                }
            }
            else {
                if (-board[27] == MAXCHECKERS) {
                    StartCoroutine(finishGame(false));
                }

                if (blackCanBearOff()) {
                    blackCheckerPhase = gamePhase.BaringOff;
                }
            }

            if (movesLeftForTurn == 0) {
                //Switch turns
                dice.startToThrowDices();

                if (currGamePhase == turnPhase.WhiteTurn) {
                    currGamePhase = turnPhase.BlackTurn;
                    if (againstAI) {
                        rolledValues = dice.throwDice(true);
                        StartCoroutine(doAIMove());
                        currGamePhase = turnPhase.WhiteTurn;
                    }
                }
                else {
                    currGamePhase = turnPhase.WhiteTurn;
                }
            }
            else if (rolledValues[0] == rolledValues[1]) {
                usedRoll1 = true;
                getLegalMoves(-1, rolledValues[1]);
            }
            else if (Mathf.Abs(newPointePos - selectedCheckerOrigBoardPos) == rolledValues[0]) {
                usedRoll1 = true;
                getLegalMoves(-1, rolledValues[1]);
            }
            else {
                usedRoll2 = true;
                getLegalMoves(rolledValues[0], -1);
            }
        }
        else {
            selectedChecker.transform.position = selectedCheckerOrigPos;
        }

        selectedChecker = null;
        selectedCheckerOrigPos = Vector3.zero;
        selectedCheckerOrigBoardPos = -1;

        resetPointes();
    }

    private void updateSelectedCheckerPos() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit)) {
            selectedChecker.transform.position = new Vector3(hit.point.x, selectedPieceYOffset, hit.point.z);
        }
    }

    private void handleClickAction() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit)) {
            Checker clickedChecker = hit.transform.GetComponent<Checker>();
            Dice clickedDice = hit.transform.GetComponent<Dice>();

            if (clickedChecker && clickedChecker.isClickable()) {
                selectedChecker = clickedChecker;
                selectedCheckerOrigPos = selectedChecker.transform.position;

                selectedCheckerOrigBoardPos = selectedChecker.getPos();

                highlightPointes(selectedCheckerOrigBoardPos);
            }
            else if (clickedDice && clickedDice.isClickable()) {
                StartCoroutine(rollDice());
            }
            else {
            }
        }
    }

    private void resetSelectedChecker() {
        selectedChecker.transform.position = selectedCheckerOrigPos;
        selectedChecker = null;
        selectedCheckerOrigPos = Vector3.zero;
        selectedCheckerOrigBoardPos = -1;
        resetPointes();
    }

    private void resetPointes() {
        baringOff.changeHighlightBaringOff(false);
        for (int i = 0; i < pointes.Length; i++) {
            pointes[i].changeHighlightPointe(false);
        }
    }

    private void highlightPointes(int boardPos) {
        foreach(var move in legalMoves) {
            if(move.Key == boardPos) {
                if(move.Value > MAXPOINTES) {
                    baringOff.changeHighlightBaringOff(true);
                }
                else {
                    pointes[move.Value].changeHighlightPointe(true);
                }
            }
        }
    }

    private bool whiteCanBearOff() {
        int numWhiteInHome = 0;

        for (int i = 0; i < 6; i++) {
            if (board[i] > 0) {
                numWhiteInHome += board[i];
            }
        }
        numWhiteInHome += board[26];

        if (numWhiteInHome == MAXCHECKERS) {
            return true;
        }

        return false;
    }

    private bool blackCanBearOff() {
        int numBlackInHome = 0;

        for (int i = 18; i < MAXPOINTES; i++) {
            if (board[i] < 0) {
                numBlackInHome += board[i];
            }
        }

        numBlackInHome += board[27];

        if (-numBlackInHome == MAXCHECKERS) {
            return true;
        }

        return false;
    }

    IEnumerator rollDice() {
        rolledValues = dice.throwDice(true);

        yield return new WaitForSeconds(waitForDiceRollTime);

        if(currGamePhase == turnPhase.BlackTurn && againstAI) {
            StartCoroutine(doAIMove());
            currGamePhase = turnPhase.WhiteTurn;
        }
        else {
            movesLeftForTurn = 2;
            usedRoll1 = false;
            usedRoll2 = false;

            if (rolledValues[0] == rolledValues[1]) {
                movesLeftForTurn = 4;
            }

            getLegalMoves(rolledValues[0], rolledValues[1]);
        } 
    }

    IEnumerator doAIMove() {

        yield return new WaitForSeconds(waitFinishMove);

        int roll1 = rolledValues[0];
        int roll2 = rolledValues[1];
        Dictionary<KeyValuePair<int, int>, float> winPercentages = new Dictionary<KeyValuePair<int, int>, float>();

        if(roll1 == roll2) {
            for(int i = 0; i < 4; i++) {
                KeyValuePair<int, int> move = AI.getPlay(board, roll1, roll2, winPercentages);

                if(move.Key == -1 && move.Value == -1) {
                    break;
                }

                updateBoard(move);

                yield return new WaitForSeconds(waitFinishMove);
            }
        }
        else {
            KeyValuePair<int, int> move = AI.getPlay(board, roll1, roll2, winPercentages);

            if (move.Key != -1 && move.Value != -1) {
                updateBoard(move);
                yield return new WaitForSeconds(waitFinishMove);

                if (Mathf.Abs(move.Key - move.Value) == roll1) {
                    move = AI.getPlay(board, roll2, -1, winPercentages);

                    if (move.Key != -1 && move.Value != -1) {
                        updateBoard(move);
                    }
                }
                else {
                    move = AI.getPlay(board, roll1, -1, winPercentages);

                    if (move.Key != -1 && move.Value != -1) {
                        updateBoard(move);
                    }
                }
            }
        }

        if(-board[27] == MAXCHECKERS) {
            StartCoroutine(finishGame(false));
        }

        if(blackCanBearOff()) {
            blackCheckerPhase = gamePhase.BaringOff;
        }

        dice.startToThrowDices();
    }

    private void updateBoard(KeyValuePair<int, int> move) {
        if (board[move.Value] == 1) {
            whiteCheckerPhase = gamePhase.Regular;
            Checker removedChecker = pointes[move.Value].removeChecker();
            removedChecker.setPos(24);
            bar.addCheckerOnBar(removedChecker);
            board[move.Value] = 0;
            board[24]++;
        }

        if (board[25] < 0) {
            Checker removedChecker = bar.removeCheckerOnBar(false);
            removedChecker.setPos(move.Value);
            pointes[move.Value].addChecker(removedChecker);
            board[25]++;
        }
        else if(move.Value > MAXPOINTES) {
            Checker removedChecker = pointes[move.Key].removeChecker();
            baringOff.addCheckerBaringOff(removedChecker);
            removedChecker.setPos(27);
            board[move.Key]++;
            board[27]--;
        }
        else {
            Checker removedChecker = pointes[move.Key].removeChecker();
            removedChecker.setPos(move.Value);
            pointes[move.Value].addChecker(removedChecker);
            board[move.Key]++;
        }

        board[move.Value]--;
    }

    IEnumerator finishGame(bool whiteHasWon) {
        AIMoveInformation.gameObject.SetActive(false);
        winInformation.gameObject.SetActive(true);
        if (whiteHasWon) {
            winInformationText.text = WHITEWON;
        }
        else {
            winInformationText.text = BLACKWON;
        }

        yield return new WaitForSeconds(waitFinishMove);

        SceneManager.LoadScene(MAINSCREENBUILDINDEX); //Go back to title screen
    }
}