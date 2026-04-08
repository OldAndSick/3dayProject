using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SellZone : MonoBehaviour
{
    [Header("Sell settings")]
    public Transform counterPoint;
    public Transform queueStartPoint;
    public Transform moneyPoint;
    public float itemHeight = 0.5f;
    public float interactionRange = 1.5f;

    [Header("Prefab")]
    public GameObject moneyPrefab;

    [Header("Money Stacking Settings")]
    public float moneyHeight = 0.1f;
    public float moneySpacingX = 0.6f;
    public float moneySpacingZ = 0.8f;

    [Header("Queue Settings")]
    public float queueSpacing = 1.5f;

    private List<Customer> customerQueue = new List<Customer>();
    private List<GameObject> counterItems = new List<GameObject>();
    private List<GameObject> moneyList = new List<GameObject>();
    private PlayerStack currentPlayer;
    private Coroutine takeRoutine;
    private Coroutine takeMoneyRoutine;
    private bool isWorkerIn = false; 

    public bool IsStaffed()
    {
        return (currentPlayer != null) || isWorkerIn;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentPlayer = other.GetComponent<PlayerStack>();
            if (takeRoutine == null) takeRoutine = StartCoroutine(TakeFromPlayerRoutine());
            if (takeMoneyRoutine == null) takeMoneyRoutine = StartCoroutine(TakeMoneyRoutine());
        }
        else if (other.CompareTag("Worker"))
        {
            isWorkerIn = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (takeRoutine != null) { StopCoroutine(takeRoutine); takeRoutine = null; }
            if (takeMoneyRoutine != null) { StopCoroutine(takeMoneyRoutine); takeMoneyRoutine = null; }
            currentPlayer = null;
        }
        else if (other.CompareTag("Worker"))
        {
            isWorkerIn = false;
        }
    }
    
    public bool TakeCarrotFromCounter(out GameObject carrot)
    {
        carrot = null;
        if (counterItems.Count > 0)
        {
            int lastIdx = counterItems.Count - 1;
            carrot = counterItems[lastIdx];
            counterItems.RemoveAt(lastIdx);

            carrot.SetActive(false);
            Destroy(carrot, 0.1f);
            return true;
        }
        return false;
    }
    private IEnumerator FlyToPoint(GameObject item, Vector3 targetLocalPos)
    {
        Vector3 startLocalPos = item.transform.localPosition;

        float time = 0f;
        float duration = 0.2f;
        while (time < duration)
        {
            time += Time.deltaTime;
            Vector3 currentPos = Vector3.Lerp(startLocalPos, targetLocalPos, time / duration);
            currentPos.y += Mathf.Sin((time / duration) * Mathf.PI) * 1.5f;
            item.transform.localPosition = currentPos;
            yield return null;
        }
        item.transform.localPosition = targetLocalPos;
    }
    public void DropMoney(int buyCarrotCount)
    {
        int billsToDrop = buyCarrotCount * 2;
        for (int i = 0; i < billsToDrop; i++)
        {
            int index = moneyList.Count;
            int layer = index / 6;
            int indexInLayer = index % 6;

            int col = indexInLayer % 2;
            int row = indexInLayer / 2;

            float x = (col - 0.5f) * moneySpacingX;
            float z = (row - 1.0f) * moneySpacingZ;
            float y = layer * moneyHeight;

            Vector3 targetLocalPos = new Vector3(x, y, z);
            GameObject money = Instantiate(moneyPrefab, moneyPoint);
            moneyList.Add(money);

            StartCoroutine(FlyToPoint(money, targetLocalPos));
        }
    }
    public Vector3 EnterQueue(Customer customer)
    {
        if (!customerQueue.Contains(customer))
        {
            customerQueue.Add(customer);
        }
        return GetQueuePosition(customer);
    }
    public Vector3 GetQueuePosition(Customer customer)
    {
        int index = customerQueue.IndexOf(customer);
        return queueStartPoint.position + (queueStartPoint.forward * -1 * index * queueSpacing);
    }
    public void ExitQueue(Customer customer)
    {
        if (customerQueue.Contains(customer))
        {
            customerQueue.Remove(customer);
            foreach (var c in customerQueue)
            {
                c.UpdateQueuePosition();
            }
        }
    }
    public bool IsFirstInLine(Customer customer)
    {
        return customerQueue.Count > 0 && customerQueue[0] == customer;
    }
    public bool TryTakeCarrot()
    {
        if (counterItems.Count > 0)
        {
            int lastIdx = counterItems.Count - 1;
            GameObject carrot = counterItems[lastIdx];
            counterItems.RemoveAt(lastIdx);

            carrot.SetActive(false);
            return true;
        }
        return false;
    }
    private IEnumerator TakeMoneyRoutine()
    {
        while (true)
        {
            if (currentPlayer != null && moneyList.Count > 0 && currentPlayer.CanAddItem("Money"))
            {
                Vector3 playerPos = currentPlayer.transform.position;
                Vector3 moneyPos = moneyPoint.position;

                playerPos.y = 0;
                moneyPos.y = 0;
                float distanceToMoneyZone = Vector3.Distance(playerPos, moneyPos);
                if (distanceToMoneyZone <= interactionRange)
                {
                    int lastIdx = moneyList.Count - 1;
                    GameObject moneyToGive = moneyList[lastIdx];
                    moneyList.RemoveAt(lastIdx);
                    currentPlayer.AddItem(moneyToGive);
                    CurrentMoney.Instance.AddMoney(5);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    public void AddCarrotByWorker(GameObject carrot)
    {
        carrot.transform.SetParent(counterPoint);
        Vector3 targetPos = GetNextCounterPos(); 

        carrot.transform.localPosition = targetPos;
        carrot.transform.localRotation = Quaternion.identity;

        counterItems.Add(carrot);
    }
    private Vector3 GetNextCounterPos()
    {
        return new Vector3(0, counterItems.Count * itemHeight, 0);
    }
    private IEnumerator TakeFromPlayerRoutine()
    {
        while (true)
        {
            if (currentPlayer != null && currentPlayer.HasItem("ProcessedCarrot"))
            {
                GameObject item = currentPlayer.RemoveItem("ProcessedCarrot");
                if (item != null)
                {
                    AddCarrotToCounter(item); 
                }
            }
            yield return new WaitForSeconds(0.15f); 
        }
    }
    public void AddCarrotToCounter(GameObject carrot)
    {
        carrot.SetActive(true);
        carrot.transform.SetParent(counterPoint);

        Vector3 targetPos = new Vector3(0, counterItems.Count * itemHeight, 0);
        carrot.transform.localPosition = targetPos;
        carrot.transform.localRotation = Quaternion.identity;

        counterItems.Add(carrot);
    }
    public int GetCounterItemCount()
    {
        return counterItems.Count;
    }
}
