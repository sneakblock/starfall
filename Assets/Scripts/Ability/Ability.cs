using System;

public class Ability
{
    protected SCharacter character;
    private bool _enabled;

    public Ability(SCharacter character)
    {
        this.character = character;
        this._enabled = false;
    }

    public bool IsEnabled()
    {
        return _enabled;
    }

    public void Enable()
    {
        _enabled = true;
        OnEnable();
    }

    public void Disable()
    {
        _enabled = false;
        OnDisable();
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

    public virtual void Start()
    {

    }

    public virtual void OnEnable()
    {
        
    }

    public virtual void Update()
    {

    }

    public virtual void OnDisable()
    {

    }
}

