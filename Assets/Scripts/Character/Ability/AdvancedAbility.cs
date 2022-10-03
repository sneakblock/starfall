using System;

public class AdvancedAbility : Ability, ICooldown, ICastable
{
    float startTime;
    float castStart;
    float cooldownDelay;
    float castDelay;

    // Cooldown and CastDelay should both be in seconds
    public AdvancedAbility(SCharacter character, float cooldown, float castDelay) : base(character)
    {
        this.cooldownDelay = cooldown * 1000;
        this.castDelay = castDelay * 1000;
        this.startTime = -cooldownDelay;
    }

    // Happens only once every time the player activates this ability
    public override void OnEnable()
    {
        if (!IsReady())
        {
            // Parent class may have set it to true, so reset it here.
            base._enabled = false;
            NotReadyYet();
            return;
        }

        StartCast();
    }

    public void StartCast()
    {
        castStart = Time();
    }

    public void OnUpdate()
    {
        if (IsCasting())
        {
            DuringCast();
            return;
        }

        // Call the OnCast function only once and call disable afterwards.
        OnCast();
        // Internally calls OnDisable() 
        Disable();
    }

    public override void OnDisable()
    {
        this.startTime = Time();
    }

    public bool IsReady()
    {
        return Time() - startTime >= this.cooldownDelay;
    }

    public bool IsCasting()
    {
        return Time() - castStart <= this.castDelay;
    }

    // Child class should override this if it wants to do something when the player
    // activates an ability, but the ability is cooling down
    public virtual void NotReadyYet()
    {

    }

    // Similar to OnUpdate() except, it gets called ONLY when the ability is being casted
    public virtual void DuringCast()
    {

    }

    // Child class should override this and this is where you write the actual
    // ability code
    // Gets invoked once the cooldown AND castDelay has passed
    public virtual void OnCast()
    {

    }

    public float Time()
    {
        return DateTime.UtcNow.Millisecond;
    }

}


