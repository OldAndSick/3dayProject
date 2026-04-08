using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineZone : MonoBehaviour
{
    [Header("Machine points")]
    public Transform dropZone;
    public Transform spawnZone;

    [Header("Settings")]
    public GameObject processedPrefab;
    public float dropInterval = 0.1f;
    public float processTime = 1.0f;
    public float itemHeight = 0.5f;
    public float interactionRange = 1.5f;

    [Header("Stack Settings")]
    public int dropColumns = 2;
    public float columnSpacing = 0.6f;
    public int maxProcessedItmes = 10;

    private List<GameObject> dropItems = new List<GameObject>();
    private List<GameObject> processItems = new List<GameObject>();

    private PlayerStack currentPlayer;
    private Coroutine takeRoutine;
    private Coroutine giveRoutine;

    private Queue<GameObject> outputPool = new Queue<GameObject>();
    private void Awake()
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(processedPrefab, spawnZone);
            obj.SetActive(false);
            outputPool.Enqueue(obj);
        }
    }

    private void Start()
    {
        StartCoroutine(ProcessRoutine());
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentPlayer = other.GetComponent<PlayerStack>();
            if (currentPlayer != null)
            {
                if (takeRoutine == null) takeRoutine = StartCoroutine(TakeFromPlayerRoutine());
                if (giveRoutine == null) giveRoutine = StartCoroutine(GiveToPlayerRoutine());
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (takeRoutine != null)
            {
                StopCoroutine(takeRoutine);
                takeRoutine = null;
            }
            if (giveRoutine != null)
            {
                StopCoroutine(giveRoutine);
                giveRoutine = null;

                currentPlayer = null;
            }
        }
    }

    private IEnumerator TakeFromPlayerRoutine()
    {
        while (true)
        {
            if (currentPlayer != null && currentPlayer.HasItem("Carrot"))
            {
                GameObject item = currentPlayer.RemoveItem("Carrot");

                AddCarrotFromPlayer(item);
            }
            yield return new WaitForSeconds(dropInterval);
        }
    }
    private IEnumerator FlyToPoint(GameObject item, Vector3 targetLocalPos)
    {
        if (item == null) yield break;

        Vector3 startLocalPos = item.transform.localPosition;
        Quaternion startLocalRot = item.transform.localRotation;
        Quaternion targetLocalRot = Quaternion.Euler(90f, 0f, 0f);
        float time = 0f;
        float duration = 0.2f;
        while (time < duration)
        {
            if (item == null) yield break;
            time += Time.deltaTime;
            float percent = time / duration;

            Vector3 currentPos = Vector3.Lerp(startLocalPos, targetLocalPos, percent);
            currentPos.y += Mathf.Sin(percent * Mathf.PI) * 1.5f;
            item.transform.localPosition = currentPos;

            item.transform.localRotation = Quaternion.Slerp(startLocalRot, targetLocalRot, percent);
            yield return null;
        }
        if (item != null)
        {
            item.transform.localPosition = targetLocalPos;
        }
        item.transform.localPosition = targetLocalPos;
        item.transform.localRotation = targetLocalRot;
    }

    private IEnumerator GiveToPlayerRoutine()
    {
        while (true)
        {
            if (currentPlayer != null && processItems.Count > 0)
            {
                Vector3 playerPos = currentPlayer.transform.position;
                Vector3 spawnPos = spawnZone.position;
                playerPos.y = 0;
                spawnPos.y = 0;

                float distanceToSpawnZone = Vector3.Distance(playerPos, spawnPos);
                if (distanceToSpawnZone <= interactionRange)
                {
                    if (currentPlayer.CanAddItem("ProcessedCarrot"))
                    {
                        int lastIdx = processItems.Count - 1;
                        GameObject itemToGive = processItems[lastIdx];
                        processItems.RemoveAt(lastIdx);

                        currentPlayer.AddItem(itemToGive);
                    }
                }
            }
            yield return new WaitForSeconds(dropInterval);
        }
    }

    private IEnumerator ProcessRoutine()
    {
        while (true)
        {
            if (dropItems.Count > 0 && processItems.Count < maxProcessedItmes)
            {
                yield return new WaitForSeconds(processTime);

                GameObject rawItem = dropItems[dropItems.Count - 1];
                dropItems.RemoveAt(dropItems.Count - 1);
                Destroy(rawItem);

                GameObject newProcessed = null;
                if(outputPool.Count >0)
                {
                    newProcessed = outputPool.Dequeue();
                }
                else
                {
                    newProcessed = Instantiate(processedPrefab, spawnZone);
                }

                Vector3 spawnLocalPos = new Vector3(0, processItems.Count * itemHeight, 0);
                newProcessed.transform.SetParent(spawnZone);
                newProcessed.transform.localPosition = spawnLocalPos;
                newProcessed.SetActive(true);

                processItems.Add(newProcessed);
            }
            else
            {
                yield return null;
            }
        }
    }
    public void AddCarrotFromWorker(GameObject carrot)
    {
        Vector3 targetLocalPos = GetNextStackPos(); // 💡 똑같은 계산기 사용!
        dropItems.Add(carrot);
        carrot.transform.SetParent(dropZone);
        StartCoroutine(FlyToPoint(carrot, targetLocalPos));
    }
    private Vector3 GetNextStackPos()
    {
        int index = dropItems.Count;
        int row = index / dropColumns;
        int col = index % dropColumns;
        return new Vector3(col * columnSpacing, row * itemHeight, 0);
    }
    private void AddCarrotFromPlayer(GameObject carrot)
    {
        Vector3 targetLocalPos = GetNextStackPos(); 
        dropItems.Add(carrot);
        carrot.transform.SetParent(dropZone);
        StartCoroutine(FlyToPoint(carrot, targetLocalPos));
    }
    public int GetProcessedCount() => processItems.Count;

    public GameObject TakeItemByWorker()
    {
        if (processItems.Count > 0)
        {
            int lastIdx = processItems.Count - 1;
            GameObject item = processItems[lastIdx];
            processItems.RemoveAt(lastIdx);
            return item;
        }
        return null;
    }
}
