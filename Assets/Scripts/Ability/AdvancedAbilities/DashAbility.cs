using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.VFX;

public class DashAbility : AdvancedAbility
{
    [SerializeField] private Material effectMaterial;
    [SerializeField] private VisualEffect effect;
    [SerializeField] private GameObject characterMeshObject;
    [SerializeField] private bool applyVertexJitterOnCast = true;
    //Different for bags and mesh? Different using Vu's mesh?
    [SerializeField] private float vertexResolutionDuringCast = 50f;

    [SerializeField] private bool applyVertexDisplacementOnCast = true;
    [SerializeField] private float vertexDisplacementAmountDuringCast = 10f;
    //ITS A BLINK NOW!
    [SerializeField] private float speed;
    [SerializeField] private float damage;
    [SerializeField] private float coolDownReductionPerKill;
    
    Vector3 targetDirection;
    Vector3 movementVector;
    private Animator anim;
    private float _stopwatch;
    private float effectDuration;
    private Dictionary<Renderer, List<Material>> _renderersMats = new();

    //I set it to a max of 16 because that is the KinematicCharacterMotor's max rigidbody overlap budget
    Collider[] enemiesOverlapped = new Collider[16];

    List<Collider> enemiesToHit = new List<Collider>();
    private static readonly int Dash = Animator.StringToHash("dash");
    private static readonly int UseVertexJitter = Shader.PropertyToID("_useVertexJitter");
    private static readonly int VertexResolution = Shader.PropertyToID("Vector1_B2CC132");
    private static readonly int UseVertexDisplacement = Shader.PropertyToID("_UseVertexDisplacement");
    private static readonly int VertexDisplacmentAmount = Shader.PropertyToID("_VertexDisplacementAmount");

    protected override void SetupReferences(SCharacter character)
    {
        anim = character.gameObject.GetComponentInChildren<Animator>();
        foreach (var r in characterMeshObject.GetComponentsInChildren<Renderer>())
        {
            var materials = new List<Material>();
            foreach (var m in r.materials)
            {
                materials.Add(m);
            }
            _renderersMats.Add(r, materials);
        }
    }

    public override void NotReadyYet()
    {
        
    }

    public override void OnCastStarted()
    {
        base.OnCastStarted();
        if (character is APlayer player)
        {
            movementVector = player.orbitCamera.transform.forward * speed;
            player.orientationMethod = APlayer.OrientationMethod.TowardsCamera;
            if (effect)
            {
                effect.Play();
            }
        }
        
        if (anim)
        {
            anim.SetTrigger(Dash);
        }
        
        ToggleMaterialEffects(true);

    }

    public override void DuringCast()
    {
        character.motor.BaseVelocity = Vector3.zero;
        character.motor.MoveCharacter(character.motor.GetState().Position + (movementVector * Time.deltaTime));
        UpdateMaterialEffects();
        // CheckCollisions();
    }

    public override void OnCastEnded()
    {
        base.OnCastEnded();
        if (character is APlayer player)
        {
            player.orientationMethod = APlayer.OrientationMethod.TowardsMovement;
            if (effect)
            {
                
            }
        }
        
        ToggleMaterialEffects(false);
        
    }

    private void CheckCollisions()
    {
        // Discrete collision detection
        int layerMask = 0;
        if (character is SAi)
        {
            layerMask |= (1 << 6);
        }

        if (character is APlayer)
        {
            layerMask |= (1 << 7);
        }
        int numberOfHits = character.motor.CharacterOverlap(
                                 character.motor.TransientPosition,
                                 character.motor.TransientRotation,
                                 enemiesOverlapped,
                                 layerMask,
                                 QueryTriggerInteraction.Ignore);

        for (int i = 0; i < numberOfHits; i++)
        {
            if (enemiesOverlapped[i] != null && enemiesToHit.Contains(enemiesOverlapped[i]) == false)
            {
                enemiesToHit.Add(enemiesOverlapped[i]);
            }
        }
    }

    void ToggleMaterialEffects(bool enabled)
    {
        foreach (var kv in _renderersMats)
        {
            //Swap actual material used
            //Is this dumb?
            if (enabled)
            {
                Material[] newMaterials = new Material[kv.Key.materials.Length];
                for (var i = 0; i < newMaterials.Length; i++)
                {
                    newMaterials[i] = effectMaterial;
                }
                kv.Key.materials = newMaterials;
            }
            else
            {
                kv.Key.materials = _renderersMats[kv.Key].ToArray();
            }
            
            //Configuring shader properties
            foreach (var m in kv.Key.materials)
            {
                if (applyVertexJitterOnCast)
                {
                    m.SetInt(UseVertexJitter, enabled ? 1 : 0);
                    m.SetFloat(VertexResolution, enabled ? vertexResolutionDuringCast : 1000f);
                }

                if (applyVertexDisplacementOnCast)
                {
                    m.SetInt(UseVertexDisplacement, enabled ? 1 : 0);
                    m.SetFloat(VertexDisplacmentAmount, enabled ? vertexDisplacementAmountDuringCast : 0);
                }
            }
            
        }
    }

    void UpdateMaterialEffects()
    {
        foreach (var kv in _renderersMats)
        {
            foreach (var m in kv.Key.materials)
            {
                if (applyVertexDisplacementOnCast)
                {
                    m.SetFloat(VertexDisplacmentAmount, Mathf.Lerp(m.GetFloat(VertexDisplacmentAmount), .1f, 1 - castTimer / castTime));
                }
            }
        }
    }
}