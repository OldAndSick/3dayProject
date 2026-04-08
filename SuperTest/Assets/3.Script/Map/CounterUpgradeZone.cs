using UnityEngine;
using TMPro;
using System.Collections;

public class CounterUpgradeZone : MonoBehaviour
{
    public MachineZone machine;
    public SellZone sellZone;
    public GameObject workerPrefab;
    public Transform spawnPoint;
    public Transform moneyDropPoint;
    public TextMeshPro costText;

    public int unlockCost = 50;
    private int remainingCost;
    private int visualCost;
    private bool isUnlocked = false;

    private PlayerStack currentPlayer;
    private Coroutine payRoutine;

    private void Start()
    {
        remainingCost = unlockCost;
        visualCost = unlockCost;
        costText.text = $"{visualCost}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isUnlocked)
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
        while (remainingCost > 0 && !isUnlocked)
        {
            if (currentPlayer != null && currentPlayer.HasItem("Money"))
            {
                GameObject moneyObj = currentPlayer.RemoveItem("Money");
                remainingCost -= 5;
                if (remainingCost < 0) remainingCost = 0;

                // 돈 날아가는 연출 시작!
                StartCoroutine(FlyAndCountDown(moneyObj, moneyDropPoint.position, 5));
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator FlyAndCountDown(GameObject item, Vector3 targetPos, int deductValue)
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
                yield return new WaitForSeconds(0.02f);
            }
        }

        if (visualCost <= 0 && !isUnlocked)
        {
            UpgradeComplete();
        }
    }

    private void UpgradeComplete()
    {
        isUnlocked = true;
        costText.text = "MAX";

        
        GameObject worker = Instantiate(workerPrefab, spawnPoint.position, Quaternion.identity);
        worker.GetComponent<CounterWorker>().Init(machine, sellZone);

        gameObject.SetActive(false); // 고용 완료 후 발판 사라짐 (선택)
    }
}