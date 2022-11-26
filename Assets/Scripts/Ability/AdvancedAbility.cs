using System;
using UnityEngine;

public class AdvancedAbility : Ability, ICooldown, ICastable
{
    [SerializeField]
    [Tooltip("The time, in seconds, after casting that the ability will be ready to use again.")]
    public float cooldownTime;
    
    [SerializeField]
    [Tooltip("The time, in seconds, that the ability will be in its casting state on use.")]
    public float castTime;

    protected float cooldownTimer;
    protected float castTimer;

    protected bool castCompleted = true;

    // Happens only once every time the player activates this ability
    public override void OnEnableAbility()
    {
        if (!castCompleted) return;
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
        castTimer = castTime;
        cooldownTimer = cooldownTime;
        base._enabled = true;
        OnCastStarted();
    }

    public override void Tick()
    {
        if (IsCasting())
        {
            DuringCast();
            return;
        }

        // Call the OnCastEnded function only once and call disable afterwards.
        OnCastEnded();
        // Internally calls OnDisable() 
        Disable();
    }

    public override void OnDisableAbility()
    {
    }

    public bool IsReady()
    {
        return cooldownTimer <= 0;
    }

    public virtual bool IsCasting()
    {
        castTimer -= Time.deltaTime;
        return castTimer > 0 && !castCompleted;
    }

    public void DecrementCooldownTimer()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        } 
    }

    // Child class should override this if it wants to do something when the player
    // activates an ability, but the ability is cooling down
    public virtual void NotReadyYet()
    {

    }

    public virtual void OnCastStarted()
    {
        castCompleted = false;
    }

    // Similar to OnUpdate() except, it gets called ONLY when the ability is being casted
    public virtual void DuringCast()
    {
        
    }

    // Child class should override this and this is where you write the actual
    // ability code
    // Gets invoked once the cooldown AND castDelay has passed
    public virtual void OnCastEnded()
    {
        castCompleted = true;
    }

}


