using System;

public class MoveFastAbility : Ability
{
    // public MoveFastAbility(SCharacter character) : base(character)
    // {
    //     // Create instance variables if needed
    // }

    public override void StartAbility()
    {
        // By default, abilities are disabled when it is registered
        // for this specific example, this ability is enabled when registered
        // for testing purposes
        base.Enable();
    }

    public override void OnEnableAbility()
    {
        // This gets called once when enabled
        base.character.maxStableMoveSpeed = 20f;
    }

    public override void Tick()
    {
        // Not needed for this ability, but other complex abilities may implement
        // this function
    }

    public override void OnDisableAbility()
    {
        // Reset speed back to initial speed when ability is disabled
        base.character.maxStableMoveSpeed = 10f;
    }
}


