using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Данный класс позволяет управлять генерацией игральных костей
/// Когда приходит время бросить кость, то мы вызываем функцию throwDice, генерируем два рандомных значения в диапазоне и 
/// вызываем скрипт Dice.cs, который имитирует бросание кости, так как сила приложенная к кости всегда будет давать нужный результат
/// </summary>
public class DiceManager : MonoBehaviour
{
    // Максимальные значения, которые можно получить бросив кость
    const int minDiceValue = 1;
    const int maxDiceValue = 6;

    // Определяем начальные точки для игральных костей во время броска и в статичном режиме
    [SerializeField] Vector3 positionOfDiceThrowing = new Vector3(2,2.5f,-11.5f);
    [SerializeField] Vector3 starterPositionOfDice = new Vector3(-0.88f,1.52f,0.58f);

    // Ссылки на экземпляры объекта игральной кости
    [SerializeField] Dice firstDice = null;
    [SerializeField] Dice secondDice = null;

    // Задает базовое положение для костей в статичном состоянии 
    [SerializeField] float dice2ZOffset = 2f;
    [SerializeField] float resetRotationY = 90f;

    // Данный метод делает кликабельным игральные кости, меняя состояние шейдера, тем самым дав понять пользователю о возможности разыграть игральную кость
    public void startToThrowDices() {
        resetTransformations(starterPositionOfDice);
        this.transform.Rotate(0f, resetRotationY, 0f);
        makeDiceClickable();
    }


    // После вызова данного метода идет генерация рандомных значений и вызов метода для отображения эффекта подбрасывания
    public int[] throwDice(bool allowDoubles) {
        resetTransformations(positionOfDiceThrowing); // Перезадаем скорость броска, чтобы гарантировать, корректность анимации броска игральной кости
        firstDice.changeHighlightDice(false); // Кости не могут двигаться во время броска, поэтому отключаем их кликабельность сменой шейдера
        secondDice.changeHighlightDice(false);

        int diceValue1 = Random.Range(minDiceValue, maxDiceValue + 1);
        int diceValue2 = Random.Range(minDiceValue, maxDiceValue + 1);

        if(!allowDoubles) {
            while(diceValue1 == diceValue2) {
                diceValue1 = Random.Range(minDiceValue, maxDiceValue + 1);
                diceValue2 = Random.Range(minDiceValue, maxDiceValue + 1);
            }
        }

        firstDice.rollDice(diceValue1); // Запускаем метод для имитации рандомной анимации бросания игральной кости
        secondDice.rollDice(diceValue2);

        int[] rolledValues = { diceValue1, diceValue2 }; // Возвращаем данные значения экземпляру класс, чтобы дать понять, какие возможные ходы пользователь может сделать
        return rolledValues;
    }

    // Перезадаем кость в соответствии с identity, чтобы избежать каких-то странных позиций
    private void resetTransformations(Vector3 newPos) {
        this.transform.position = newPos;
        this.transform.rotation = Quaternion.identity;
        firstDice.transform.localPosition = new Vector3(0f, 0f, 0f);
        secondDice.transform.localPosition = new Vector3(0f, 0f, dice2ZOffset);
        firstDice.transform.rotation = Quaternion.identity;
        secondDice.transform.rotation = Quaternion.identity;
    }

   
    // Запускаем шейдеры, для того чтобы дать понять игроку, что они кликабельны для разыгрывания 
    private void makeDiceClickable() {
        firstDice.changeHighlightDice(true);
        secondDice.changeHighlightDice(true);
    }
}
