using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

public class FenceManager : MonoBehaviour
{
    [Header("Stable Status")]
    public int currentAnimals = 0;
    public int reservedAnimals = 0;
    public int maxAnimals = 20;
    public bool IsFull => reservedAnimals >= maxAnimals;

    [Header("Ending References")]
    public GameObject endingUpgradeZone;
    public GameObject gameTitleUI;

    [Header("Fence Visuals")]
    public Transform backWall;
    public GameObject gate;
    public GameObject expandedFenceGroup;

    [Header("Waiting Queue Settings")]
    public Transform queueStartPoint;
    public float queueSpacing = 1.5f;
    private List<Customer> waitingQueue = new List<Customer>();

    [Header("Cinemachine Ending")]
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera eventCam;
    public Transform stableCenter;
    public Transform player;

    [Header("UI Settings")]
    public TextMeshProUGUI statusText;

    private void Start()
    {
        UpdateStatusUI();

        if (expandedFenceGroup != null) expandedFenceGroup.SetActive(false);
        if (endingUpgradeZone != null) endingUpgradeZone.SetActive(false);
        if (gameTitleUI != null) gameTitleUI.SetActive(false);
        if (gate != null) gate.SetActive(false);
    }
    public void ReserveSpot()
    {
        reservedAnimals++;
    }
    public void AddAnimal()
    {
        currentAnimals++;
        UpdateStatusUI();
        Debug.Log($"현재 마구간: {currentAnimals} / {maxAnimals}");

        if (currentAnimals >= maxAnimals && endingUpgradeZone != null && !endingUpgradeZone.activeSelf)
        {
            // 20마리 꽉 차면 연출 시작! (바로 안 켜고 뜸들임)
            if (endingUpgradeZone != null) endingUpgradeZone.SetActive(true);
            if (gate != null) gate.SetActive(true);
            StartCoroutine(ShowFullStableRoutine());
        }
    }
    public Vector3 EnterQueue(Customer customer)
    {
        if (!waitingQueue.Contains(customer)) waitingQueue.Add(customer);
        int index = waitingQueue.IndexOf(customer);
        return queueStartPoint.position + (queueStartPoint.forward * index * queueSpacing);
    }
    public void ExitQueue(Customer customer)
    {
        if (waitingQueue.Contains(customer)) waitingQueue.Remove(customer);
    }
    public void ExpandFenceAndEndGame()
    {
        StartCoroutine(EndingRoutine());
    }

    private IEnumerator EndingRoutine()
    {
        // 엔딩 줌아웃 연출
        eventCam.transform.rotation = playerCam.transform.rotation;
        Vector3 offset = playerCam.transform.position - player.position;
        // 원래 거리보다 훨씬 뒤/위로 이동 (줌아웃)
        eventCam.transform.position = stableCenter.position + (offset * 2.5f);

        if (backWall != null) backWall.gameObject.SetActive(false);
        if (expandedFenceGroup != null) expandedFenceGroup.SetActive(true);

        eventCam.Priority = 30; // 줌아웃 시작
        yield return new WaitForSeconds(2.5f); // 줌아웃 상태 유지

        // 2. 줌인 (다시 플레이어로 복귀)
        eventCam.Priority = 0; // PlayerCam으로 돌아가면서 줌인 효과
        yield return new WaitForSeconds(1.5f); // 돌아오는 시간 대기

        // 3. 마지막 엔딩 UI 띄우기
        if (gameTitleUI != null) gameTitleUI.SetActive(true);
    }
    private IEnumerator ShowFullStableRoutine()
    {
        eventCam.transform.rotation = playerCam.transform.rotation;
        Vector3 offset = playerCam.transform.position - player.position;
        eventCam.transform.position = stableCenter.position + offset;

        eventCam.Priority = 20;

        yield return new WaitForSeconds(0.5f);
        endingUpgradeZone.SetActive(true);
        if (gate != null) gate.SetActive(true);

        yield return new WaitForSeconds(1.5f);
        eventCam.Priority = 0;
    }
    private void UpdateStatusUI()
    {
        if (statusText != null)
        {
            statusText.text = $"{currentAnimals} / {maxAnimals}";
        }
    }
}