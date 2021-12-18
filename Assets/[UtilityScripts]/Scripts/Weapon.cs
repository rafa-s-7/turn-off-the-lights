﻿using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Weapon : XRBaseInteractable
{
    public int recoil_amount = 25;

    private new Rigidbody rigidbody;
    private Barrel barrel;

    private GripHold grip_hold;
    private GuardHold guard_hold;

    public XRBaseInteractor grip_hand;
    public XRBaseInteractor guard_hand;

    private readonly Vector3 grip_rotation;// = new Vector3(45, 0, 0);

    public float break_distance = 0.25f;

    private bool holding = false;
    protected override void Awake()
    {
        base.Awake();
        SetupHolds();
        SetupExtras();

        onSelectEntered.AddListener(SetInitialRotation);
    }

    private void SetupHolds()
    {
        grip_hold = GetComponentInChildren<GripHold>();
        grip_hold.Setup(this);

        guard_hold = GetComponentInChildren<GuardHold>();
        guard_hold.Setup(this);
    }

    private void SetupExtras()
    {
        rigidbody = GetComponent<Rigidbody>();
        barrel = GetComponentInChildren<Barrel>();
        barrel.Setup(this);
    }

    private void SetInitialRotation(XRBaseInteractor interactor)
    {
        Quaternion new_rotation_quaternion = Quaternion.Euler(grip_rotation);
        interactor.attachTransform.localRotation = new_rotation_quaternion;
        transform.localRotation = new_rotation_quaternion;
    }

    public void SetGripHand(XRBaseInteractor interactor)
    {
        grip_hand = interactor;
        holding = true;
        ManualSelect(interactor);
    }

    public void ManualSelect(XRBaseInteractor interactor)
    {
        OnSelectEntering(interactor);
        OnSelectEntered(interactor);
    }

    public void ClearGripHand(XRBaseInteractor interactor)
    {
        grip_hand = null;
        holding = false;
        ManualDeselect(interactor);
    }

    public void ManualDeselect(XRBaseInteractor interactor)
    {
        OnSelectExiting(interactor);
        OnSelectExited(interactor);
    }
    public void SetGuardHand(XRBaseInteractor interactor)
    {
        guard_hand = interactor;
        ManualSelect(interactor);
    }

    public void ClearGuardHand(XRBaseInteractor interactor)
    {
        guard_hand = null;
        ManualDeselect(interactor);
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        if (grip_hand && guard_hand)
        {
            SetGripRotation();
            print("IN HERE");
        }
        CheckDistance(grip_hand,grip_hold);
        CheckDistance(guard_hand,guard_hold);
    }

    private void SetGripRotation()
    {
        Vector3 target = guard_hand.transform.position - grip_hold.transform.position;
        Quaternion look_rotation = Quaternion.LookRotation(target);

        Vector3 grip_rotation = Vector3.zero;
        grip_rotation.z = grip_hand.transform.eulerAngles.z;

        look_rotation*= Quaternion.Euler(grip_rotation);

        grip_hand.attachTransform.rotation = look_rotation;
        transform.rotation = look_rotation;
    }

    private void CheckDistance(XRBaseInteractor interactor, HandHold handHold)
    {
        if (interactor)
        {
            float distance_square = GetDistanceSqrToInteractor(interactor);
            if (distance_square > break_distance)
            {
                handHold.BreakHold(interactor);
            }
        }
    }

    public void PullTrigger()
    {
        print("TRIGGER PRESSED");
        barrel.StartFiring();
    }

    public void ReleaseTrigger()
    {
        barrel.StopFiring();
    }

    public void ApplyRecoil()
    {
        rigidbody.AddRelativeForce(Vector3.back*recoil_amount,ForceMode.Impulse);
    }
}
