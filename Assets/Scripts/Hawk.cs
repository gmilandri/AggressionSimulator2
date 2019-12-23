using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hawk : Pop
{
    GameObject m_target;

    public override void MyUpdate()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward) * VisionDistance;
        Debug.DrawRay(transform.position, forward, Color.red);

        m_Energy.TimeTick();

        Debug.Log(gameObject.name + " is determining his target.");

        DetermineTarget();

        if (HasSafeTarget())
        {
            Debug.Log(gameObject.name + " has found a new target and set his destination to such target.");
            m_agent.SetDestination(m_target.transform.position);
        }

        if (HasSafeTarget() && DistanceFromFood() < 2f)
        {
            Debug.Log(gameObject.name + " is close to target and ate it.");
            float totalEnergyFromEatenPop = m_target.GetComponent<Pop>().m_Energy.Energy - GameManager.Instance.MinimumPopEnergy;
            float totalEnergyToHawk = totalEnergyFromEatenPop / 100 * MetabolismRate;
            m_Energy.IncreaseEnergyBy(totalEnergyToHawk);

            m_target.GetComponent<Pop>().m_Energy.ResetEnergy();

            DespawnPopAndResetMyTarget();
        }

        if (!HasSafeTarget() && (!m_agent.hasPath || m_agent.remainingDistance < 1f))
        {
            TargetRandomSpot();
        }

        if (m_Energy.CountdownHasEnded)
        {
            m_Energy.EnergyTick(EnergySpentPerTick);
            m_Energy.ResetCountdown();
            CheckStatus();
        }
    }

    private void DespawnPopAndResetMyTarget()
    {
        Debug.Log("Food was despawned.");
        m_target.GetComponent<Pop>().ResetSelf();
        ResetTarget();
    }

    public void ResetTarget()
    {
        Debug.Log(gameObject.name + " target has been reset.");
        m_target = null;
        m_agent.ResetPath();
    }

    private void DetermineTarget()
    {
        m_target = ClosestTarget();
    }

    public override float DistanceFromFood() => Vector3.Distance(transform.position, m_target.transform.position);

    private GameObject ClosestTarget()
    {
        //if (m_gameManager.DoveGenerosity)
        //    GiveFood();

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance: VisionDistance * 3f))
        {
            if (hit.collider.gameObject.CompareTag("Hawk") || hit.collider.gameObject.CompareTag("Dove"))
            {
                if (hit.collider.gameObject.GetComponent<Pop>().Size >= Size)
                {
                    return hit.collider.gameObject;
                }
            }
        }
        return null;
    }

    private bool HasSafeTarget()
    {
        if (m_target == null)
            return false;
        if (!m_target.activeSelf)
            return false;
        return true;
    }

    private void TargetRandomSpot()
    {
        Debug.Log(gameObject.name + " is targeting a random spot.");

        bool foundSpot = false;
        Vector3 tempDest = Vector3.zero;

        while (!foundSpot)
        {
            var randomX = Random.Range(-5f, 5f);
            var randomZ = Random.Range(-5f, 5f);
            tempDest = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
            foundSpot = m_gameManager.IsDestinationOnFloor(tempDest);
        }
        m_agent.SetDestination(tempDest);
    }
    //{
    //return Vector3.Distance(transform.position, m_gameManager.PopObjectPool[m_indexTarget].transform.position);
    //}

    // public void ResetTarget() => m_indexTarget = -1;

    //private bool HasPossibleTarget => m_indexTarget == -1 ? false : true;

    //private void SelectPrey() => m_indexTarget = m_gameManager.ClosestPopDestination(transform.position, Size);


}
