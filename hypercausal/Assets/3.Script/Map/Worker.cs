using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;

    private CarrotZone targetFarm;
    private MachineZone targetMachine;

    private int myRow = 0;
    private int currentCol = 0;
    private bool goingRight = true;

    public void Init(CarrotZone farm, MachineZone machine, int rowIndex)
    {
        targetFarm = farm;
        targetMachine = machine;
        myRow = rowIndex; 
        StartCoroutine(WorkSequence());
    }

    private IEnumerator WorkSequence()
    {
        while (true)
        {
            Vector3 targetPos = GetGridWorldPos(myRow, currentCol);
            yield return StartCoroutine(MoveToPoint(targetPos));

            GameObject carrot = targetFarm.GetCarrotAt(transform.position);
            if (carrot != null && carrot.activeSelf)
            {
                targetFarm.PickCarrotByWorker(carrot);
                targetMachine.AddCarrotFromWorker(carrot); 
                yield return new WaitForSeconds(0.1f);
            }

            if (goingRight)
            {
                currentCol++;
                if (currentCol >= targetFarm.columns) { currentCol = targetFarm.columns - 1; goingRight = false; }
            }
            else
            {
                currentCol--;
                if (currentCol < 0) { currentCol = 0; goingRight = true; }
            }
        }
    }

    private Vector3 GetGridWorldPos(int r, int c)
    {
        float startX = -(targetFarm.columns - 1) * targetFarm.spacing / 2f;
        float startZ = -(targetFarm.rows - 1) * targetFarm.spacing / 2f;

        float x = startX + (c * targetFarm.spacing);
        float z = startZ + (r * targetFarm.spacing);

        return targetFarm.transform.position + (targetFarm.transform.rotation * new Vector3(x, 0, z));
    }


    private IEnumerator MoveToPoint(Vector3 targetPos)
    {
        while (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                               new Vector3(targetPos.x, 0, targetPos.z)) > 0.1f)
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
}