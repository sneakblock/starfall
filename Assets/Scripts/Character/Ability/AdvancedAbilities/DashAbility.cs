using System;

public class DashAbility : AdvancedAbility
{
    public DashAbility(SCharacter character, float cooldown, float castDelay) : base(character, cooldown, castDelay)
    {

    }

    public DashAbility(SCharacter character) : base(character, 30f, 2.5f)
    {

    }

    public override void NotReadyYet()
    {

    }

    public override void DuringCast()
    {
        
    }

    public override void OnCast()
    {
        
    }
}


