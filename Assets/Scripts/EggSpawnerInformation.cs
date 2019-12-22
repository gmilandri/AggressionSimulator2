using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggSpawnerInformation : MonoBehaviour
{
    public bool IsDove { get; private set; }

    public void SetPopType(Pop original) {
        if (original is Dove)
            IsDove = true;
        else
            IsDove = false; 
    }

    public IEnumerator BirthCountdown()
    {
        yield return new WaitForSeconds(Random.Range(1,20));
        GameManager.Instance.CreateNewPop(this);
        gameObject.SetActive(false);
    }

}
