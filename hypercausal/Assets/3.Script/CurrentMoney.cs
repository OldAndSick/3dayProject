using UnityEngine;
using TMPro;

public class CurrentMoney : MonoBehaviour
{
    public static CurrentMoney Instance; 

    public TextMeshProUGUI moneyText; 
    private int currentMoney = 0;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        UpdateUI();
    }
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();
    }

    public bool TrySpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    private void UpdateUI()
    {
        moneyText.text = currentMoney.ToString("N0");
    }
}