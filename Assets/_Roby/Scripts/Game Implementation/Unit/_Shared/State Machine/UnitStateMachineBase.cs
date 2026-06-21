using Animancer;
using RAXY.Animation;
using RAXY.Movement;
using RAXY.StateMachine;
using UnityEngine;

public class UnitStateMachineBase : StateMachine
{
    private UnitControllerBase ownerCont;

    public UnitStateMachineBase(UnitControllerBase ownerCont)
    {
        if (ownerCont == null)
            return;

        GetGameObject = ownerCont.gameObject;
        GetTransform = ownerCont.transform;

        Cont = ownerCont;
        Animator = GetGameObject.GetComponent<Animator>();
        Animancer = GetGameObject.GetComponent<AnimancerComponent>();
        GroundChecker = GetGameObject.GetComponent<GroundChecker>();
        AnimancerCont = GetGameObject.GetComponent<AnimancerController>();

        MovementCont = GetGameObject.GetComponent<UnitMovement>();
        AnimationClips = Cont.AnimationClips;
    }

    public GameObject GetGameObject { get; set; }
    public Transform GetTransform { get; set; }
    public Animator Animator { get; set; }
    public AnimancerComponent Animancer { get; set; }
    public GroundChecker GroundChecker { get; set; }
    public AnimancerController AnimancerCont { get; set; }
    public UnitMovement MovementCont { get; set; }
    public UnitControllerBase Cont { get; set; }

    public UnitAnimationClipsSO AnimationClips { get; set; }
}
