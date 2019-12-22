using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyPop
{
    public float Energy { get; private set; }
    public int EnergyTickSpeed { get; private set; }
    public int CountdownEnergyCheck { get; private set; }
    public void IncreaseEnergyBy(float amount) => Energy += amount;
    public void TimeTick() => CountdownEnergyCheck--;
    public void EnergyTick(float energySpentPerTick)
    {
        if (Energy > GameManager.Instance.MinimumPopEnergy)
        {
            Energy -= energySpentPerTick;
            GameManager.Instance.AvailableBiomassIncreaseBy(energySpentPerTick);
        }
    }
    public void ResetCountdown() => CountdownEnergyCheck = EnergyTickSpeed * (11 - GameManager.Instance.SpeedFactor);
    public bool CountdownHasEnded => CountdownEnergyCheck == 0 ? true : false;
    public bool EnergyEnded => Energy <= GameManager.Instance.MinimumPopEnergy ? true : false;
    public bool EnergyHigherThan(float amount) => Energy >= amount ? true : false;
    public void DecreaseEnergyBy(float amount) => Energy -= amount;
    public void ResetEnergy() => Energy = GameManager.Instance.MinimumPopEnergy;

    public void ResetValues(float Energy, int EnergyTickSpeed)
    {
        this.Energy = Energy;
        this.EnergyTickSpeed = EnergyTickSpeed;
        this.CountdownEnergyCheck = EnergyTickSpeed * (11 - GameManager.Instance.SpeedFactor);
    }

    public EnergyPop (float Energy, int EnergyTickSpeed)
    {
        this.Energy = Energy;
        this.EnergyTickSpeed = EnergyTickSpeed;
        this.CountdownEnergyCheck = EnergyTickSpeed * (11 - GameManager.Instance.SpeedFactor);
    }
}
