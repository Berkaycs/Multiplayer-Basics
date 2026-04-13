using UnityEngine;

public class PickableResource : PickableBase
{
    protected override void ApplyAvailabilityState(bool newValue)
    {

    }

    protected override void OnPickedUp()
    {
        if (!IsServer) return;

        NetworkObject.Despawn();
    }
}
