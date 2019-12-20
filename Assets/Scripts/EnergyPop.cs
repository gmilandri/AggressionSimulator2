using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyPop
{
    public int Energy { get; private set; }
    public int EnergyTickSpeed { get; private set; }
    public int CountdownEnergyCheck { get; private set; }

    public void IncreaseEnergy(int amount) => Energy += amount;
    public void TimeTick() => CountdownEnergyCheck--;
    public void EnergyTick()
    {
        if (Energy > 0)
        {
            Energy--;
            GameManager.Instance.AvailableBiomass++;
        }
    }
    public void ResetCountdown() => CountdownEnergyCheck = EnergyTickSpeed;
    public bool CountdownHasEnded => CountdownEnergyCheck == 0 ? true : false;
    public bool EnergyEnded => Energy <= 0 ? true : false;
    public bool EnergyHigherThan(int amount) => Energy >= amount ? true : false;
    public void DecreaseEnergyBy(int amount) => Energy -= amount;

    public void ResetValues(int Energy, int EnergyTickSpeed)
    {
        this.Energy = Energy;
        this.EnergyTickSpeed = EnergyTickSpeed;
        this.CountdownEnergyCheck = EnergyTickSpeed;
    }

    public EnergyPop (int Energy, int EnergyTickSpeed)
    {
        this.Energy = Energy;
        this.EnergyTickSpeed = EnergyTickSpeed;
        this.CountdownEnergyCheck = EnergyTickSpeed;
    }
}
