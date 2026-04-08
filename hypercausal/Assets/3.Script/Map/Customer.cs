using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Customer : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;

    [Header("Shopping Settings")]
    public int wantedAmount;
    public float dealInterval = 0.5f;

    [Header("UI Settings")]
    public TextMeshProUGUI buyCountText;

    public SellZone targetShop;
    private Transform entryPoint;
    private Transform stableCenter;
    private FenceManager fenceManager;

    private int boughtAmount;
    private Vector3 myTargetPos;
    private bool isMovingToQueue = false;



    public void Init(SellZone shop, Transform shopPt, Transform entryPt, Transform stableCenterPt, CustomerSpawner mySpawner)
    {
        targetShop = shop;
        entryPoint = entryPt;
        stableCenter = stableCenterPt;

        fenceManager = FindObjectOfType<FenceManager>();
        boughtAmount = 0;
        if(buyCountText != null) buyCountText.gameObject.SetActive(false);
        StartCoroutine(CustomerRoutine());
    }

    private IEnumerator CustomerRoutine()
    {
        wantedAmount = Random.Range(2, 4);
        myTargetPos = targetShop.EnterQueue(this);
        isMovingToQueue = true;

        while (true)
        {
            yield return StartCoroutine(MoveToPoint(myTargetPos));
            isMovingToQueue = false;

            if (targetShop.IsFirstInLine(this)) break;

            while (!isMovingToQueue && !targetShop.IsFirstInLine(this))
            {
                yield return null;
            }
        }

        if (buyCountText != null)
        {
            buyCountText.gameObject.SetActive(true);
            UpdateBuyUI();
        }

        while (boughtAmount < wantedAmount)
        {
            if (targetShop.IsStaffed())
            {
                if (targetShop.TakeCarrotFromCounter(out GameObject carrot))
                {
                    boughtAmount++;
                    UpdateBuyUI();
                    yield return new WaitForSeconds(dealInterval);
                }
                else yield return null; // 물건 없으면 대기
            }
            else yield return null; // 판매자 없으면 대기
        }

        if (buyCountText != null) buyCountText.gameObject.SetActive(false);

        targetShop.ExitQueue(this);
        targetShop.DropMoney(boughtAmount);

        if (fenceManager != null)
        {
            if (fenceManager.IsFull)
            {
                Vector3 waitPos = fenceManager.EnterQueue(this);
                yield return StartCoroutine(MoveToPoint(waitPos));
                yield return StartCoroutine(LookAtPoint(stableCenter.position));

                while (fenceManager.IsFull) yield return new WaitForSeconds(0.5f);

                fenceManager.ExitQueue(this);
            }

            fenceManager.ReserveSpot();
            yield return StartCoroutine(MoveToPoint(entryPoint.position));

            Vector3 randomPos = stableCenter.position + new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));
            yield return StartCoroutine(MoveToPoint(randomPos));
            yield return StartCoroutine(LookAtPoint(entryPoint.position));

            fenceManager.AddAnimal();
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        yield break;
    }
    public void UpdateQueuePosition()
    {
        if (targetShop != null)
        {
            myTargetPos = targetShop.GetQueuePosition(this);
            isMovingToQueue = true;
        }
    }
    private IEnumerator MoveToPoint(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            Vector3 direction = (targetPos - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
    private IEnumerator LookAtPoint(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                yield return null;
            }
            transform.rotation = targetRot;
        }
    }
    private void UpdateBuyUI()
    {
        if (buyCountText != null)
        {
            int remaining = wantedAmount - boughtAmount;
            buyCountText.text = remaining.ToString();
        }
    }
}
