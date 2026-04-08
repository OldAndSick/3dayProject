using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStack : MonoBehaviour
{

    [Header("Stack Settings")]
    public Transform stackPoint;
    public float itemHeight = 0.5f;
    public float columnDepth = 0.5f;
    public int maxTypes = 2;
    public float flyDuration = 0.3f;

    [Header("Capacity Settings")]
    public int maxCarryAmount = 10;

    [Header("UI Settings")]
    public GameObject maxTextObj; 

    private Coroutine maxRoutine;
    private string[] slotTypes = new string[2] { "","" };
    private Dictionary<string, List<GameObject>> stackDict = new Dictionary<string, List<GameObject>>();

    public int GetItemCount(string tag)
    {
        if(stackDict.ContainsKey(tag))
        {
            return stackDict[tag].Count;
        }
        return 0;
    }
    public bool CanAddItem(string itemTag)
    {
        if(itemTag == "Carrot")
        {
            if (GetItemCount("Carrot") >= maxCarryAmount) return false;
        }
        for(int i = 0; i < maxTypes; i++)
        {
            if (slotTypes[i] == itemTag) return true;
        }
        for(int i = 0; i< maxTypes; i++)
        {
            if (string.IsNullOrEmpty(slotTypes[i])) return true;
        }
        return false;
    }
    public void AddItem(GameObject item)
    {
        string tag = item.tag;
        if (!stackDict.ContainsKey(tag))
        {
            stackDict[tag] = new List<GameObject>();
        }

        int colIndex = -1;
        for(int i = 0; i< maxTypes; i++)
        {
            if(slotTypes[i] == tag)
            {
                colIndex = i;
                break;
            }
        }
        if (colIndex == -1)
        {
            for (int i = 0; i < maxTypes; i++)
            {
                if (string.IsNullOrEmpty(slotTypes[i]))
                {
                    slotTypes[i] = tag;
                    colIndex = i;
                    break;
                }
            }
        }
        int rowIndex = stackDict[tag].Count;

        Vector3 targetLocalPos = new Vector3(-colIndex * columnDepth, rowIndex * itemHeight, 0);

        Quaternion targetLocalRot = Quaternion.identity;
            if (tag == "Carrot") targetLocalRot = Quaternion.Euler(90f, 0f, 0f);
            else targetLocalRot = Quaternion.identity;

            stackDict[tag].Add(item);
            item.transform.SetParent(stackPoint);

            StartCoroutine(FlyToBag(item, targetLocalPos, targetLocalRot));
        
    }
    private IEnumerator FlyToBag(GameObject item, Vector3 targetLocalPos, Quaternion targetLocalRot)
    {
        Vector3 startLocalPos = item.transform.localPosition;
        Quaternion startLocalRot = item.transform.localRotation;

        float time = 0f;

        while(time<flyDuration)
        {
            time += Time.deltaTime;
            float percent = time / flyDuration;
            Vector3 currentPos = Vector3.Lerp(startLocalPos, targetLocalPos, percent);
            currentPos.y += Mathf.Sin(percent * Mathf.PI) * 5f; // sin -> make goksun

            item.transform.localPosition = currentPos;
            item.transform.localRotation = Quaternion.Slerp(startLocalRot, targetLocalRot, percent);

            yield return null;
        }

        item.transform.localPosition = targetLocalPos;
        item.transform.localRotation = targetLocalRot;

    }

    public bool HasItem(string tag)
    {
        return stackDict.ContainsKey(tag) && stackDict[tag].Count > 0;
    }
    public GameObject RemoveItem(string tag)
    {
        if (HasItem(tag))
        {
            var list = stackDict[tag];
            GameObject item = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);

            if(list.Count == 0)
            {
                stackDict.Remove(tag);
                for(int i = 0; i < maxTypes; i++)
                {
                    if(slotTypes[i] == tag)
                    {
                        slotTypes[i] = "";
                        break;
                    }
                }
            }
            return item;
        }
        return null;
    }
    
    public void ShowMaxText()
    {
        if (maxTextObj != null)
        {
            if (maxRoutine != null) StopCoroutine(maxRoutine);
            maxRoutine = StartCoroutine(MaxTextRoutine());
        }
    }

    private IEnumerator MaxTextRoutine()
    {
        maxTextObj.SetActive(true);

        Transform camTransform = Camera.main.transform;

        if (maxTextObj.TryGetComponent<TMPro.TextMeshPro>(out var tmp))
        {
            tmp.fontMaterial.shader = Shader.Find("TextMeshPro/Distance Field Overlay");
        }
        maxTextObj.transform.SetParent(null);

        float duration = 1.0f; 
        float time = 0f;

        while (time < duration)
        {
            if (gameObject == null || !gameObject.activeSelf) break;

            if (maxTextObj == null || !maxTextObj.activeSelf) break;

            time += Time.deltaTime;

            Vector3 targetPos = transform.position + Vector3.up * 2.5f; 
            maxTextObj.transform.position = targetPos;

            maxTextObj.transform.rotation = camTransform.rotation;

            yield return null; 
        }

        if (maxTextObj != null)
        {
            maxTextObj.SetActive(false);
            maxTextObj.transform.SetParent(transform);
        }
        maxRoutine = null;
    }
}
