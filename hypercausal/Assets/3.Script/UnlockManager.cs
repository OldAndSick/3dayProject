using System.Collections;
using UnityEngine;
using Cinemachine;

public class UnlockManager : MonoBehaviour
{
    [Header("Upgrade Zones")]
    public GameObject playerUpgradeTier1; // 1. 처음 켜질 1차 업그레이드
    public GameObject playerUpgradeTier2; // 2. 나중에 켜질 최종 업그레이드
    public GameObject workerUpgrade;      // 2. 인부 고용
    public GameObject cashierUpgrade;     // 2. 판매대 직원 고용

    [Header("Camera Settings")]
    public CinemachineVirtualCamera playerCam; 
    public CinemachineVirtualCamera eventCam;

    private void Start()
    {
        if (playerUpgradeTier1 != null) playerUpgradeTier1.SetActive(false);
        if (playerUpgradeTier2 != null) playerUpgradeTier2.SetActive(false);
        if (workerUpgrade != null) workerUpgrade.SetActive(false);
        if (cashierUpgrade != null) cashierUpgrade.SetActive(false);

        if (eventCam != null) eventCam.Priority = 0;
        if (playerCam != null) playerCam.Priority = 10;
    }

    public void UnlockFirstUpgrade()
    {
        StartCoroutine(ShowFirstUpgradeRoutine());
    }

    private IEnumerator ShowFirstUpgradeRoutine()
    {
        if (eventCam != null && playerUpgradeTier1 != null)
        {
            eventCam.transform.rotation = playerCam.transform.rotation;

            Vector3 offset = playerCam.transform.position - FindObjectOfType<TutorialManager>().player.position;
            eventCam.transform.position = playerUpgradeTier1.transform.position + offset;

            eventCam.Priority = 20;
        }

        if (playerUpgradeTier1 != null) playerUpgradeTier1.SetActive(true);

        yield return new WaitForSeconds(2.0f); // 구경 시간

        if (eventCam != null) eventCam.Priority = 0; // 다시 복귀
    }

    public void UnlockSecondaryUpgrades()
    {
        if (playerUpgradeTier2 != null) playerUpgradeTier2.SetActive(true);
        if (workerUpgrade != null) workerUpgrade.SetActive(true);
        if (cashierUpgrade != null) cashierUpgrade.SetActive(true);
    }
}