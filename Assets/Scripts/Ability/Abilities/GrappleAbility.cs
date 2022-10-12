using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrappleAbility : AdvancedAbility
{
    // The location of the grapple's destination.
    private Vector3 targetPos;
    // LineRenderer that draws a line representing the grapple.
    private LineRenderer grapple;

    private float startTime, grappleMaxLength, grappleSpeed;

    public GrappleAbility(SCharacter character, float cooldownTime) : base(character, cooldownTime, 0f)
    {
    }

    public GrappleAbility(SCharacter character) : base(character, 10f, 0f)
    {
    }

    public override void NotReadyYet()
    {
        
    }
    public override bool IsCasting()
    {
        // Check if the player has reached the grapple destination
        float dist = Vector3.Distance(character.transform.position, targetPos);
        return dist > 1f;
    }

    public override void OnCastStarted()
    {
        base.OnCastStarted();

        grappleMaxLength = character.characterData.grappleMaxLength;
        grappleSpeed = character.characterData.grappleSpeed;
        grapple = character.GetComponent<LineRenderer>();
        grapple.positionCount = 2;

        // Fire a raycast, starting from the player's location and in the direction of where the camera is facing
        Ray ray = new Ray(character.transform.position + new Vector3(0f, 1.3f, 0f), Camera.main.transform.forward);
        RaycastHit hitPoint;

        // If the grapple hits something, set the location of the grapple's destination
        if (Physics.Raycast(ray, out hitPoint, grappleMaxLength))
            targetPos = hitPoint.point;

        startTime = Time.time;
    }

    public override void DuringCast()
    {
        DrawGrapple(character.transform.position + new Vector3(0f, 1.3f, 0f), targetPos);

        // Move the player along the vector from their current position to the grapple destination location
        float duration = (Time.time - startTime) * grappleSpeed / Vector3.Distance(character.transform.position, targetPos);
        Vector3 grapplePos = Vector3.Lerp(character.transform.position, targetPos, duration);
        character.motor.MoveCharacter(grapplePos);
    }

    public override void OnCastEnded()
    {
        // Remove the line drawn for the grapple
        grapple.positionCount = 0;
    }

    // Draw a 2D line to represent the grapple
    private void DrawGrapple(Vector3 start, Vector3 end)
    {
        grapple.SetPosition(0, start);
        grapple.SetPosition(1, end);
    }
}