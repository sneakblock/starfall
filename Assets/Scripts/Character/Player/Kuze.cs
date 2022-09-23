using UnityEngine;


public class Kuze : APlayer
{

    //TEMPORARY, HACKY ANIMATION CONTROLLER FOR PITCH
    //TODO (ben): FIX THIS TRASH
    // TODO(mish question): This should be refactored? Maybe have an automatic animation
    // loader.
    public Animator anim;

    protected override void StartPlayer()
    {
        // TODO(ben): Will this be player specific or general for all the
        // players?
        anim = base.GetComponentInChildren<Animator>();

        // TODO(ben): Describe the usage of the following bc why?
        var o = base.gameObject;
        foreach (Transform t in o.GetComponentsInChildren<Transform>())
        {
            var gameObject1 = t.gameObject;
            gameObject1.layer = 6;
            gameObject1.tag = "Player";
        }
    }

    protected override void UpdatePlayer()
    {
        
        // Implement Kuze specific update code here

    }

    protected override void HandlePlayerInputs()
    {
        HandleAnimationInputs();
        //Implement other Kuze specific inputs here
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

