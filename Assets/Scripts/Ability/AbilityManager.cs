using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityManager
{
    private List<Ability> abilities;

    public AbilityManager()
    {
        abilities = new List<Ability>();
    }

    public void Register(Ability ability)
    {
        this.abilities.Add(ability);
    }

    public void Start()
    {
        foreach (Ability ability in abilities)
        {
            ability.StartAbility();
        }
    }

    public void Update()
    {
        foreach (Ability ability in abilities)
        {
            if (ability.IsEnabled())
            {
                ability.Tick();
            } 
            else
            {
                if (ability is ICooldown)
                {
                    ICooldown abilityWithCooldown = ability as ICooldown;
                    abilityWithCooldown.DecrementCooldownTimer();
                }
            }
        }
    }

    public void DisableAll()
    {
        foreach (Ability ability in abilities)
        {
            ability.Disable();
        }
    }
}


