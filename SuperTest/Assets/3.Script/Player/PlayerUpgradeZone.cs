using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUpgradeZone : MonoBehaviour
{
    [Header("References")]
    public CarrotZone carrotZone;
    public Transform moneyDropPoint;
    public TextMeshPro costText;
    public GameObject modelInHand;

    [Header("Upgrade Settings")]
    public int level1Cost = 20; 
    public int level2Cost = 50; 
    public int moneyValue = 5;
    public int capacityAddAmount = 5;
    public float speedDecreaseAmount = 0.1f; // 한 번에 단축될 캐는 시간 (초)
    public float minSpeed = 0.1f;         // 캐는 속도 최대치 (이하로는 안 떨어짐)
    public float takeInterval = 0.1f;

    private int currentUpgradeLevel = 0;
    private int remainingCost;
    private int visualCost;
    private PlayerStack currentPlayer;
    private Coroutine payRoutine;
    private bool isMaxLevel = false;

    private void Start()
    {
        remainingCost = level1Cost;
        visualCost = level1Cost;
        if (costText != null) costText.text = $"{visualCost}";
        if (modelInHand != null) modelInHand.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isMaxLevel)
        {
            currentPlayer = other.GetComponent<PlayerStack>();
            if (payRoutine == null) payRoutine = StartCoroutine(PayRoutine());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (payRoutine != null)
            {
                StopCoroutine(payRoutine);
                payRoutine = null;
            }
            currentPlayer = null;
        }
    }
    private IEnumerator PayRoutine()
    {
        while (remainingCost > 0 && !isMaxLevel)
        {
            if (currentPlayer != null && currentPlayer.HasItem("Money"))
            {
                GameObject moneyObj = currentPlayer.RemoveItem("Money");
                remainingCost -= moneyValue;

                if (remainingCost < 0) remainingCost = 0;
                PlayerStack payingPlayer = currentPlayer;
                StartCoroutine(FlyAndCountDown(moneyObj, moneyDropPoint.position, moneyValue, payingPlayer));
            }
            yield return new WaitForSeconds(takeInterval);
        }
    }
    private IEnumerator FlyAndCountDown(GameObject item, Vector3 targetPos, int deductValue, PlayerStack player)
    {
        Vector3 startPos = item.transform.position;
        float duration = 0.3f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float percent = time / duration;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, percent);
            currentPos.y += Mathf.Sin(percent * Mathf.PI) * 2f;
            item.transform.position = currentPos;
            yield return null;
        }
        Destroy(item);

        for (int i = 0; i < deductValue; i++)
        {
            if (visualCost > 0)
            {
                visualCost--;
                if (costText != null) costText.text = $"{visualCost}";
                yield return new WaitForSeconds(0.02f); // 엄청 빠른 속도로 깎임
            }
        }
        if (visualCost <= 0 && !isMaxLevel)
        {
            visualCost = 0;
            UpgradeComplete(player);
        }
    }
    private void UpdateUI()
    {
        if(costText != null)
        {
            costText.text = $"{remainingCost}";
        }
    }
    private void UpgradeComplete(PlayerStack player)
    {
        currentUpgradeLevel++;

        if (currentUpgradeLevel == 1) // level 1
        {
            player.maxCarryAmount = 15;
            carrotZone.harvestAmount = 3;
            carrotZone.farmRange = 2.0f; 
            if (carrotZone.farmInterval > 0.1f) carrotZone.farmInterval -= 0.1f;

            if (modelInHand != null) modelInHand.SetActive(true);

            FindObjectOfType<UnlockManager>().UnlockSecondaryUpgrades();
            remainingCost = level2Cost;
            visualCost = level2Cost;
            if (costText != null) costText.text = $"{visualCost}";
        }
        else if (currentUpgradeLevel == 2) // level 2
        {
            player.maxCarryAmount = 30;
            carrotZone.harvestAmount = 8;
            carrotZone.farmRange = 3.5f; 
            if (carrotZone.farmInterval > 0.1f) carrotZone.farmInterval -= 0.1f;

            isMaxLevel = true;
            if (costText != null) costText.text = "MAX";

            GetComponent<Collider>().enabled = false;
        }
    }
}
