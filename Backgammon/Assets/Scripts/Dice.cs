using UnityEngine;

/// <summary>
/// Данный класс определяет поведение игральной кости
/// </summary>
public class Dice : MonoBehaviour
{
    // Используем два разных шейдера, чтобы дать понять пользователю, когда элемент становится кликабельным
    private const string HIGHLIGHTSHADERNAME = "Shader Graphs/DiceHighlight";
    private const string DEFAULTSHADERNAME = "Lightweight Render Pipeline/Lit";
    private bool clickable = false;

    // Значения, которые можно получить с помощью игральной кости (1-6) 
    private int minDiceRollNum = 0;
    private int maxDiceRollNum = 6;

    // Сила, с которой бросаем игральную кость
    [SerializeField] Vector3 forceAmount = new Vector3(-300f, 0f, 0f);
    [SerializeField] Rigidbody rb = null;
    [SerializeField] Renderer renderer = null;

    // После тестирования на протяжении некоторого времени, стало понятно, какую силу нужно приложить 
    // Это довольно простой способ сохранить эффект рандомности подбрасывания игральной кости для более глубокого погружения в игровой процесс
    private Vector3 roll1Torque = new Vector3(700f, 2000f, 4000f);
    private Vector3 roll2Torque = new Vector3(-30f, 360f, 360f);
    private Vector3 roll3Torque = new Vector3(-30f, -45f, -30f);
    private Vector3 roll4Torque = new Vector3(-80f, -10f, 10f);
    private Vector3 roll5Torque = new Vector3(1900f, 1100f, -2000f);
    private Vector3 roll6Torque = new Vector3(30f, -10f, 60f);

    // Если метод получает значение 1, то применяет нужную силу к кости, чтобы она упала на данное значение
    public void rollDice(int numToRoll)
    {
        if (numToRoll <= minDiceRollNum || numToRoll > maxDiceRollNum)
        {
            //Should not be possible, the number rolled should be in bounds
            Debug.LogError("Invalid number given to dice roll");
            return;
        }

        rb.velocity = new Vector3(0f, 0f, 0f);
        rb.angularVelocity = new Vector3(0f, 0f, 0f);

        rb.AddForce(forceAmount);

        if (numToRoll == 1)
        {
            rb.AddTorque(roll1Torque);
        }
        else if (numToRoll == 2)
        {
            rb.AddTorque(roll3Torque);
        }
        else if (numToRoll == 3)
        {
            rb.AddTorque(roll4Torque);
        }
        else if (numToRoll == 4)
        {
            rb.AddTorque(roll2Torque);
        }
        else if (numToRoll == 5)
        {
            rb.AddTorque(roll5Torque);
        }
        else if (numToRoll == 6)
        {
            rb.AddTorque(roll6Torque);
        }
        else
        {
            Debug.LogError("Invalid parameter given to dice roll");
        }
    }

    // Применяем нужный шейдер к игральной кости в зависимости от их кликабельности
    public void changeHighlightDice(bool toHighlight)
    {
        if (toHighlight)
        {
            renderer.material.shader = Shader.Find(HIGHLIGHTSHADERNAME);
            clickable = true;
        }
        else
        {
            renderer.material.shader = Shader.Find(DEFAULTSHADERNAME);
            clickable = false;
        }
    }


    public bool isClickable()
    {
        return clickable;
    }

}
