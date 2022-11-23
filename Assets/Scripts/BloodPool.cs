using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodPool : GameObjectPool
{
    [SerializeField] private int maxBloodOnScreen = 50;
    
    private readonly Queue<BFX_BloodSettings> _bloodFadeQueue = new();
    
    protected override GameObject CreatePooledItem()
    {
        var blood = base.CreatePooledItem();
        var settings = blood.GetComponent<BFX_BloodSettings>();
        settings.DecalRenderinMode = BFX_BloodSettings._DecalRenderinMode.AverageRayBetwenForwardAndFloor;
        settings.AnimationSpeed = 2;
        if (GameManager.Instance.dirLight) settings.LightIntensityMultiplier = GameManager.Instance.dirLight.intensity;
        return blood;
    }

    protected override void OnTakeFromPool(GameObject obj)
    {
        base.OnTakeFromPool(obj);
        var settings = obj.GetComponent<BFX_BloodSettings>();
        settings.FreezeDecalDisappearance = true;
        _bloodFadeQueue.Enqueue(settings);
        if (_bloodFadeQueue.Count > maxBloodOnScreen)
        {
            _bloodFadeQueue.Dequeue().FreezeDecalDisappearance = false;
        }
    }
}
