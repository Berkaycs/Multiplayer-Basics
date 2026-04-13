using UnityEngine;
using Unity.Netcode;

public abstract class PickableBase : NetworkBehaviour, IInteractable
{
    [SerializeField] private SelectionOutline _selectionOutline;
    [SerializeField] private ObjectType _objectType;

    protected NetworkVariable<bool> _isAvailable = new NetworkVariable<bool>
    (   
        true, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public bool CanBePickedUp => _isAvailable.Value;
    public ObjectType ObjectType => _objectType;

    public override void OnNetworkSpawn()
    {
        _isAvailable.OnValueChanged += OnIsAvailabilityChanged;
        ApplyAvailabilityState(_isAvailable.Value);
    }

    public override void OnNetworkDespawn()
    {
        _isAvailable.OnValueChanged -= OnIsAvailabilityChanged;
    }

    private void OnIsAvailabilityChanged(bool previousValue, bool newValue)
    {
        ApplyAvailabilityState(newValue);
    }

    protected abstract void ApplyAvailabilityState(bool newValue);

    public void PickUp()
    {
        if (!IsServer) return;

        _isAvailable.Value = false;
        OnPickedUp();
    }

    protected abstract void OnPickedUp();

    public void ToggleSelection(bool isSelected)
    {
        if (_selectionOutline != null)
        {
            _selectionOutline.ToggleOutline(isSelected);
        }
    }
}
