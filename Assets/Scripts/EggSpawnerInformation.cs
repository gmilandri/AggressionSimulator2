using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggSpawnerInformation : MonoBehaviour
{
    public bool IsDove { get; private set; }

    public float OriginalSpeed;

    public int OriginalMetabolism;

    public float OriginalSize;

    public void SetPopType(Pop original) {
        if (original is Dove)
            IsDove = true;
        else
            IsDove = false; 
    }

    public void SetOriginalSpeed(float speed)
    {
        OriginalSpeed = speed;
    }

    public void SetOriginalSize(float size)
    {
        OriginalSize = size;
    }

    public void SetOriginalMetabolism(int metabolism)
    {
        OriginalMetabolism = metabolism;
    }

    public IEnumerator BirthCountdown()
    {
        yield return new WaitForSeconds(Random.Range(1,20));
        GameManager.Instance.CreateNewPop(this);
        gameObject.SetActive(false);
    }

}
