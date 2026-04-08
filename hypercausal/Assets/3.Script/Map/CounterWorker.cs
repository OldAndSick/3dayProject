using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterWorker : MonoBehaviour
{
    [Header("References")]
    public Transform stackPoint; 
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 10f;
    public int maxCapacity = 10;
    public float itemSpacing = 0.5f;

    private MachineZone machine;
    private SellZone sellZone;
    private List<GameObject> carriedItems = new List<GameObject>();
    private float initialY;

    public void Init(MachineZone m, SellZone s)
    {
        machine = m;
        sellZone = s;
        initialY = transform.position.y;
        StartCoroutine(WorkRoutine());
    }

    private IEnumerator WorkRoutine()
    {
        while (true)
        {
            if (machine.GetProcessedCount() > 0 && carriedItems.Count < maxCapacity)
            {
                Vector3 targetPos = new Vector3(machine.spawnZone.position.x, initialY, machine.spawnZone.position.z);
                yield return StartCoroutine(MoveToPoint(targetPos));

                while (machine.GetProcessedCount() > 0 && carriedItems.Count < maxCapacity)
                {
                    GameObject item = machine.TakeItemByWorker();
                    if (item != null)
                    {
                        item.transform.SetParent(stackPoint);
                        item.transform.localPosition = new Vector3(0, carriedItems.Count * itemSpacing, 0);
                        item.transform.localRotation = Quaternion.identity;
                        carriedItems.Add(item);
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }

            if (carriedItems.Count > 0)
            {
                Vector3 targetPos = new Vector3(sellZone.counterPoint.position.x, initialY, sellZone.counterPoint.position.z);
                yield return StartCoroutine(MoveToPoint(targetPos));

                while (carriedItems.Count > 0)
                {
                    int lastIdx = carriedItems.Count - 1;
                    GameObject itemToDrop = carriedItems[lastIdx];
                    carriedItems.RemoveAt(lastIdx);

                    sellZone.AddCarrotByWorker(itemToDrop);
                    yield return new WaitForSeconds(0.1f);
                }

                while (sellZone.GetCounterItemCount() > 0)
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }
            else
            {
                Vector3 waitPos = (machine.transform.position + sellZone.transform.position) / 2f;
                waitPos.y = initialY;
                yield return StartCoroutine(MoveToPoint(waitPos));
                yield return new WaitForSeconds(0.5f);
            }
            yield return null;
        }
    }

    private IEnumerator MoveToPoint(Vector3 targetPos)
    {
        while (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                               new Vector3(targetPos.x, 0, targetPos.z)) > 0.5f)
        {
            Vector3 dir = (targetPos - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);
            }
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}