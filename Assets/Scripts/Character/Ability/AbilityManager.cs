using System;
using System.Collections.Generic;

public class AbilityManager
{
    private List<Ability> abilities;

    public AbilityManager()
    {
        this.abilities = new List<Ability>();
    }

    public void Register(Ability ability)
    {
        this.abilities.Add(ability);
    }

    public void Start()
    {
        foreach (Ability ability in abilities)
        {
            ability.Start();
        }
    }

    public void Update()
    {
        foreach (Ability ability in abilities)
        {
            if (ability.IsEnabled())
            {
                ability.Update();
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


