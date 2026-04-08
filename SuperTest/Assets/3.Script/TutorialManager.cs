using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialStep { Harvest, DropToMachine, PickupProcessed, Sell, CollectMoney, Done }

    [Header("Current Status")]
    public TutorialStep currentStep = TutorialStep.Harvest;

    [Header("UI References")]
    public GameObject moveGuideUI;
    public TextMeshProUGUI promptText;

    [Header("Arrow Settings")]
    public Transform arrow;         
    public float nearDistance = 8f;  

    [Header("Target Points (목표 지점들)")]
    public Transform player;
    public Transform carrotZone;      // 1. 당근 밭
    public Transform machineDropZone; // 2. 기계 넣는 곳
    public Transform machineSpawnZone;// 3. 기계 완성품 나오는 곳
    public Transform sellZone;        // 4. 판매대
    public Transform moneyDropZone;

    [Header("Item Check Settings")]
    public string rawCarrotTag = "Carrot";            
    public string processedCarrotTag = "ProcessedCarrot"; 
    public string moneyTag = "Money";
    public int maxCarrotGoal = 10;                    

    private bool hasMoved = false;
    private Vector3 initialPlayerPos;

    private void Start()
    {
        if (moveGuideUI != null) moveGuideUI.SetActive(true);
        if (promptText != null) promptText.gameObject.SetActive(true);
        if (arrow != null) arrow.gameObject.SetActive(true);

        if (player != null) initialPlayerPos = player.position;
    }

    private void Update()
    {
        if (player == null || arrow == null) return;

        // 1. 첫 움직임 감지 -> 8자 UI 끄기
        if (!hasMoved && Vector3.Distance(player.position, initialPlayerPos) > 0.5f)
        {
            hasMoved = true;
            if (moveGuideUI != null) moveGuideUI.SetActive(false);
            if (promptText != null) promptText.gameObject.SetActive(false);
        }

        // 2. 튜토리얼 체크 및 변경
        CheckStepConditions();

        // 3. 화살표 연출 (멀면 길안내, 가까우면 머리 위)
        Transform currentTarget = GetCurrentTarget();
        if (currentStep != TutorialStep.Done && currentTarget != null)
        {
            UpdateArrowVisuals(currentTarget);
        }
        else
        {
            arrow.gameObject.SetActive(false);
        }
    }

    private void CheckStepConditions()
    {
        int rawCount = CountPlayerItems(rawCarrotTag);
        int processedCount = CountPlayerItems(processedCarrotTag);
        int moneyCount = CountPlayerItems(moneyTag);

        switch (currentStep)
        {
            case TutorialStep.Harvest:
                // 10개 다 캐면 기계로 가
                if (rawCount >= maxCarrotGoal) currentStep = TutorialStep.DropToMachine;
                break;

            case TutorialStep.DropToMachine:
                // 생당근이 0개가 되면 완성품 나오는 곳
                if (rawCount == 0 && hasMoved) currentStep = TutorialStep.PickupProcessed;
                break;

            case TutorialStep.PickupProcessed:
                // 가공 당근을 하나라도 들면 판매대로
                if (processedCount > 0) currentStep = TutorialStep.Sell;
                break;

            case TutorialStep.Sell:
                // 가공 당근을 다 팔았으면 돈 줍는 곳으로
                if (processedCount == 0 && rawCount == 0 && hasMoved) currentStep = TutorialStep.CollectMoney;
                break;

            case TutorialStep.CollectMoney:
                // 돈을 주웠으면 튜토리얼 끝 + 업그레이드 해금!
                if (moneyCount > 0)
                {
                    currentStep = TutorialStep.Done;

                    FindObjectOfType<UnlockManager>().UnlockFirstUpgrade();
                }
                break;
        }
    }

    private Transform GetCurrentTarget()
    {
        switch (currentStep)
        {
            case TutorialStep.Harvest: return carrotZone;
            case TutorialStep.DropToMachine: return machineDropZone;
            case TutorialStep.PickupProcessed: return machineSpawnZone;
            case TutorialStep.Sell: return sellZone;
            case TutorialStep.CollectMoney: return moneyDropZone;
            default: return null;
        }
    }

    private void UpdateArrowVisuals(Transform target)
    {
        float dist = Vector3.Distance(player.position, target.position);

        if (dist > nearDistance)
        {
            Vector3 dir = (target.position - player.position).normalized;
            dir.y = 0;
            arrow.position = player.position + Vector3.up * 1.5f + dir * 2.0f;

            if (dir != Vector3.zero)
                arrow.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90, 0, 0);

        }
        else
        {
            float bounce = Mathf.Sin(Time.time * 6f) * 0.3f; // 둥둥거리는 효과
            arrow.position = target.position + Vector3.up * 3.5f + new Vector3(0, bounce, 0);

            arrow.rotation = Quaternion.Euler(180, 0, 0);

        }
    }

    private int CountPlayerItems(string tagToFind)
    {
        int count = 0;
        foreach (Transform child in player.GetComponentsInChildren<Transform>(false))
        {
            if (child.CompareTag(tagToFind)) count++;
        }
        return count;
    }
}