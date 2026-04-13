using UnityEngine;
using Unity.Netcode.Components;

public class PickableTool : PickableBase
{
    [SerializeField] private ComponentController _componentController;

    protected override void ApplyAvailabilityState(bool newValue)
    {
        if (IsServer)
        {
            _componentController.SetEnabled(newValue);
        }
    }

    protected override void OnPickedUp()
    {
        
    }

    public void Drop(Vector3 position)
    {
        if (!IsServer) return;

        transform.position = new Vector3(position.x, transform.position.y, position.z);
        _isAvailable.Value = true;
    }
}
