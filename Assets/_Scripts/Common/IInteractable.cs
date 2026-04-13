using Unity.Netcode;

public interface IInteractable
{
    void ToggleSelection(bool isSelected);
    NetworkObject NetworkObject { get; } // we dont need to implement this, because it is already implemented in the NetworkBehaviour class
}
