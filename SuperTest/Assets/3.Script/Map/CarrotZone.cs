using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrotZone : MonoBehaviour
{
    [Header("Auto Settings")]
    public float farmInterval = 0.5f;
    public float respawnTime = 3f;
    public GameObject prefabToGive;

    [Header("Field Grid Settings")]
    public int rows = 5;
    public int columns = 5;
    public float spacing = 1.2f;

    [Header("Optimization (Object Pool")]
    public int poolSize = 50;

    [Header("Farm Power Settings")]
    public int harvestAmount = 1; 
    public float farmRange = 1.5f;

    private Coroutine currentFarmingRoutine;
    private PlayerStack currentPlayer;
    private Queue<GameObject> carrotPool = new Queue<GameObject>();
    private List<GameObject> plantCarrot  = new List<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefabToGive, transform.position, Quaternion.identity);
            obj.transform.SetParent(transform); 
            obj.SetActive(false); 
            carrotPool.Enqueue(obj); 
        }
    }
    private void Start()
    {
            GenerateField();
    }
    private void GenerateField()
    {
        float startX = -(columns - 1) * spacing / 2f;
        float startZ = -(rows - 1) * spacing / 2f;

        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (carrotPool.Count > 0)
                {
                    GameObject carrot = carrotPool.Dequeue();

                    Vector3 localPos = new Vector3(startX + (x * spacing), 1.5f, startZ + (z * spacing));
                    carrot.transform.position = transform.position + localPos;

                    carrot.SetActive(true); 
                    plantCarrot.Add(carrot); 
                }
            }
        }
    }
    private void SpawnCarrotAt(Vector3 pos)
    {
        if (carrotPool.Count > 0)
        {
            GameObject obj = carrotPool.Dequeue();
            obj.transform.position = pos;
            obj.transform.rotation = Quaternion.identity;
            obj.SetActive(true);
            plantCarrot.Add(obj);
        }
        else
        {
            GameObject obj = Instantiate(prefabToGive, pos, Quaternion.identity);
            obj.transform.SetParent(transform);
            plantCarrot.Add(obj);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            currentPlayer = other.GetComponent<PlayerStack>();
            if(currentPlayer != null)
            {
                Debug.Log("캐는중...");
                if(currentFarmingRoutine != null)
                {
                    StopCoroutine(currentFarmingRoutine);
                }
                currentFarmingRoutine = StartCoroutine(FarmingRoutine());
            }
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("캐기 종료");
            if (currentFarmingRoutine != null)
            {
                StopCoroutine(currentFarmingRoutine);
                currentFarmingRoutine = null;
            }
            currentPlayer = null;
        }
    }

    private IEnumerator FarmingRoutine()
    {
        while(true)
        {
            if (currentPlayer != null && plantCarrot.Count > 0)
            {
                List<GameObject> reachableCarrots = new List<GameObject>();

                for (int i = 0; i < plantCarrot.Count; i++)
                {
                    Vector3 playerPos = currentPlayer.transform.position;
                    Vector3 carrotPos = plantCarrot[i].transform.position;
                    playerPos.y = 0; carrotPos.y = 0;

                    if (Vector3.Distance(playerPos, carrotPos) <= farmRange)
                    {
                        reachableCarrots.Add(plantCarrot[i]);
                    }
                }
                int countToHarvest = Mathf.Min(harvestAmount, reachableCarrots.Count);
                for (int i = 0; i < countToHarvest; i++)
                {
                    GameObject targetCarrot = reachableCarrots[i];
                    Vector3 emptyPos = targetCarrot.transform.position;

                    plantCarrot.Remove(targetCarrot);
                    StartCoroutine(RespawnRoutine(emptyPos));

                    if (currentPlayer.CanAddItem("Carrot"))
                    {
                        currentPlayer.AddItem(targetCarrot);
                    }
                    else
                    {
                        ReturnCarrotToPool(targetCarrot);
                        currentPlayer.ShowMaxText();
                    }
                }
            }
            yield return new WaitForSeconds(farmInterval);
        }
    }
    private IEnumerator RespawnRoutine(Vector3 pos)
    {
        yield return new WaitForSeconds(respawnTime);
        SpawnCarrotAt(pos);
    }
    public void ReturnCarrotToPool(GameObject carrot)
    {
        carrot.SetActive(false);
        carrot.transform.SetParent(transform);
        carrotPool.Enqueue(carrot);
    }
    public GameObject GetRandomCarrot()
    {
        if (plantCarrot.Count > 0) return plantCarrot[Random.Range(0, plantCarrot.Count)];
        return null;
    }

    public void PickCarrotByWorker(GameObject carrot)
    {
        if (plantCarrot.Contains(carrot))
        {
            Vector3 pos = carrot.transform.position;
            plantCarrot.Remove(carrot);
            StartCoroutine(RespawnRoutine(pos));
        }
    }
    public GameObject GetCarrotAt(Vector3 workerPos, float checkRadius = 0.5f)
    {
        for (int i = 0; i < plantCarrot.Count; i++)
        {
            Vector3 carrotPos = plantCarrot[i].transform.position;
            if (Vector2.Distance(new Vector2(workerPos.x, workerPos.z), new Vector2(carrotPos.x, carrotPos.z)) <= checkRadius)
            {
                return plantCarrot[i];
            }
        }
        return null;
    }
}
