using System.Collections;
using UnityEngine;
using TMPro;

public class EndingUpgradeZone : MonoBehaviour
{
    public FenceManager fenceManager;
    public Transform moneyDropPoint;
    public TextMeshPro costText;

    public int endingCost = 100; // 마지막 장식을 위한 비용 (알아서 조절해!)
    private int remainingCost;
    private int visualCost;

    private PlayerStack currentPlayer;
    private Coroutine payRoutine;
    private bool isFinished = false;

    private void Start()
    {
        remainingCost = endingCost;
        visualCost = endingCost;
        if (costText != null) costText.text = $"{visualCost}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isFinished)
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
        while (remainingCost > 0 && !isFinished)
        {
            if (currentPlayer != null && currentPlayer.HasItem("Money"))
            {
                GameObject moneyObj = currentPlayer.RemoveItem("Money");
                remainingCost -= 5; // 돈 1다발당 5원
                if (remainingCost < 0) remainingCost = 0;

                PlayerStack payingPlayer = currentPlayer;
                StartCoroutine(FlyAndCountDown(moneyObj, moneyDropPoint.position, 5, payingPlayer));
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator FlyAndCountDown(GameObject item, Vector3 targetPos, int deductValue, PlayerStack player)
    {
        Vector3 startPos = item.transform.position;
        float duration = 0.2f;
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

        if (visualCost <= 0 && !isFinished)
        {
            isFinished = true;
            GetComponent<Collider>().enabled = false; // 발판 끄기
            costText.text = "CLEAR!";

            fenceManager.ExpandFenceAndEndGame();
        }
    }
}