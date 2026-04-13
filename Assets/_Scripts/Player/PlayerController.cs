using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private MyPlayerInput _playerInput;
    [SerializeField] private AgentMover _agentMover;
    [SerializeField] private InteractionDetector _interactionDetector;
    [SerializeField] private Animator _animator;
    [SerializeField] private AnimationEvents _animationEvents;
    
    private bool _isInteracting = false;

    private void OnEnable()
    {
        _playerInput.OnPickUpPressed += OnPickUpPressed;
        _playerInput.OnInteractPressed += OnInteractPressed;
    }

    private void OnDisable()
    {
        _playerInput.OnPickUpPressed -= OnPickUpPressed;
        _playerInput.OnInteractPressed -= OnInteractPressed;
    }

    public override void OnNetworkSpawn()
    {
        _interactionDetector.Initialize(IsOwner);

        if (IsOwner)
        {
            _animationEvents.OnInteract += OnInteract;
            _animationEvents.OnAnimationDone += OnAnimationDone;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            _animationEvents.OnInteract -= OnInteract;
            _animationEvents.OnAnimationDone -= OnAnimationDone;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector2 movementInput = _playerInput.MovementInput;
        _agentMover.Move(movementInput);
    }

    private void OnPickUpPressed()
    {
        if (_isInteracting) return;

        if (_interactionDetector.ClosestInteractable == null) return;

        _animator.SetBool("Interact", true);
        _isInteracting = true;
    }

    private void OnInteractPressed()
    {
        Debug.Log("Interact Pressed");
    }

    private void OnInteract()
    {
        if (_interactionDetector.ClosestInteractable is PickableBase)
        {
            RequestPickUpServerRpc(_interactionDetector.ClosestInteractable.NetworkObject.NetworkObjectId);
        }
    }

    private void OnAnimationDone()
    {
        _isInteracting = false;
    }

    [Rpc(SendTo.Server)]
    private void RequestPickUpServerRpc(ulong pickableId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(pickableId, out NetworkObject pickable)) return;
        if (!pickable.TryGetComponent(out PickableBase pickableItem)) return;
        if (!pickableItem.CanBePickedUp) return;

        pickableItem.PickUp();
    }
}
