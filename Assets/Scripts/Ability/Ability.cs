
using System;
using UnityEngine;

public class Ability : MonoBehaviour
{
    protected SCharacter character;
    protected bool _enabled;

    public bool IsEnabled()
    {
        return _enabled;
    }

    public void Enable()
    {
        _enabled = true;
        OnEnableAbility();
    }

    public void Disable()
    {
        _enabled = false;
        OnDisableAbility();
    }

    public void Toggle()
    {
        if (IsEnabled())
        {
            Disable();
        }
        else
        {
            Enable();
        }
    }

    public virtual void StartAbility()
    {

    }

    public virtual void OnEnableAbility()
    {
        
    }

    public virtual void Tick()
    {

    }

    public virtual void OnDisableAbility()
    {

    }

    public void SetCharacter(SCharacter aCharacter)
    {
        this.character = aCharacter;
        SetupReferences(aCharacter);
    }

    protected virtual void SetupReferences(SCharacter character)
    {
        
    }
}

