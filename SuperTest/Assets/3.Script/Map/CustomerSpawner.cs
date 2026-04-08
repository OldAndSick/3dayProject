using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject customerPrefab;
    public float spawnInterval = 3f;
    public int maxCapacity = 20;

    [Header("Map Points")]
    public Transform startPoint;
    public Transform shopPoint;
    public Transform entryPoint;
    public Transform stableCenterPoint;
    public SellZone targetShop;

    public int currentCustomerCount = 0;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }
    private IEnumerator SpawnRoutine()
    {
        while(true)
        {
            if(currentCustomerCount < maxCapacity)
            {
                SpawnCustomer();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    public void SpawnCustomer()
    {
        GameObject customer = Instantiate(customerPrefab, startPoint.position, Quaternion.identity);
        customer.SetActive(true);
        customer.GetComponent<Customer>().Init(targetShop, shopPoint,entryPoint, stableCenterPoint,this);
        currentCustomerCount++;
    }
    public void UpgradeCapacity(int addAmount)
    {
        maxCapacity += addAmount;
    }
}
