using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICooldown
{
    bool IsReady();

    // gets called if player activates ability BUT its not ready yet
    // maybe child class will apply negative damage if the player tries to
    // activate delicate ability when it is not ready yet
    void NotReadyYet();

    void DecrementCooldownTimer();

}
