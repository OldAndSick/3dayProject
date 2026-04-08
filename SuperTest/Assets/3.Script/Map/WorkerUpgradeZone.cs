using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WorkerUpgradeZone : MonoBehaviour
{
    public CarrotZone carrotZone;
    public MachineZone machineZone;
    public GameObject workerPrefab;
    public Transform spawnPoint;
    public Transform moneyDropPoint;
    public TextMeshPro costText;

    public int unlockCost = 50;
    private int remainingCost;
    private int visualCost;

    private PlayerStack currentPlayer;
    private Coroutine payRoutine;
    private bool isMaxLevel = false;

    private void Start()
    {
        remainingCost = unlockCost;
        visualCost = unlockCost;
        costText.text = $"{visualCost}";
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
            if (payRoutine != null) { StopCoroutine(payRoutine); payRoutine = null; }
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
                remainingCost -= 5;

                if (remainingCost < 0) remainingCost = 0;

                StartCoroutine(FlyAndCountDown(moneyObj, moneyDropPoint.position, 5, currentPlayer));
            }
            yield return new WaitForSeconds(0.1f);
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
        if (CurrentMoney.Instance != null)
        {
            CurrentMoney.Instance.TrySpendMoney(10);
        }
        for (int i = 0; i < deductValue; i++)
        {
            if (visualCost > 0)
            {
                visualCost--;
                if (costText != null) costText.text = $"{visualCost}";
                yield return new WaitForSeconds(0.02f);
            }
        }
        if (visualCost <= 0 && !isMaxLevel)
        {
            UpgradeComplete();
        }
    }
    private void UpgradeComplete()
    {
        isMaxLevel = true;
        costText.text = "MAX";

        SpawnWorker(0); 
        SpawnWorker(1);

        gameObject.SetActive(false); 
    }
    private void SpawnWorker(int rowIndex)
    {
        GameObject newWorker = Instantiate(workerPrefab, spawnPoint.position, Quaternion.identity);
        newWorker.GetComponent<Worker>().Init(carrotZone, machineZone, rowIndex);
    }
}