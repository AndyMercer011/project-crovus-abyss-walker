using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PlayerAnimatorController
{
    
    private bool stand_sataus = true;

    public enum LowerBodyState:int{
        STAND = 0,
        WALK = 1,
        RUN = 2,
    }

    private LowerBodyState prev_lower_body_state = LowerBodyState.STAND;

    private Animator animatorController;

    public float moveSpeed = 10;

    public PlayerAnimatorController(Animator animatorController)
    {
        this.animatorController = animatorController;
    }

    public void SetLowerBodyState(LowerBodyState state)
    {  
        if(state != prev_lower_body_state){
            prev_lower_body_state = state;
            animatorController.SetInteger("move_status", (int)state);
        }
    }

    public void InvertStandMode(){
        // first restore LowerBodyState to STAND
        SetLowerBodyState(LowerBodyState.STAND);

        // second reverse the stand_sataus
        animatorController.SetInteger("stand_mode", (stand_sataus = !stand_sataus) ? 1 : 0);
    }
}
