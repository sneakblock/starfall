using UnityEngine;


public class Kuze : APlayer
{

    //TEMPORARY, HACKY ANIMATION CONTROLLER FOR PITCH
    //TODO (ben): FIX THIS TRASH
    // TODO(mish question): This should be refactored? Maybe have an automatic animation
    // loader.
    public Animator anim;

    private MoveFastAbility _moveFastAbility;

    protected override void StartPlayer()
    {
        // TODO(ben): Will this be player specific or general for all the
        // players?
        anim = base.GetComponentInChildren<Animator>();

        // Giving a new ability to kuze
        base.RegisterAbility(_moveFastAbility = new MoveFastAbility(this));
    }

    protected override void UpdatePlayer()
    {
        base.UpdatePlayer();
        
        // Implement Kuze specific update code here

    }

    protected override void HandlePlayerInputs()
    {
        HandleAnimationInputs();

        // Implement other Kuze specific inputs here

        // If you press a specific key, call this function to toggle the ability
        // _moveFastAbility.Toggle();
    }

    // Other players could have different animations.
    private void HandleAnimationInputs()
    {
        if (anim != null)
        {
            float moveAxisRight = Input.GetAxisRaw(HorizontalInput);
            float moveAxisForward = Input.GetAxisRaw(VerticalInput);

            bool isMoving = moveAxisRight != 0 || moveAxisForward != 0;

            anim.SetBool("isMoving", isMoving);
            anim.SetBool("isFiring", Input.GetMouseButton(0));
        }
    }
}

