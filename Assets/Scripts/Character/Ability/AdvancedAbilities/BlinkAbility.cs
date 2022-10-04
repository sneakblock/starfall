using System;

public class BlinkAbility : AdvancedAbility
{
    public BlinkAbility(SCharacter character, float cooldown) : base(character, cooldown, 0.0f)
    {

    }

    public BlinkAbility(SCharacter character) : base(character, 60f, 0f)
    {

    }

    public override void NotReadyYet()
    {

    }

    public override void OnCast()
    {

    }
}