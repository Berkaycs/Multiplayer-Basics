using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private MyPlayerInput _playerInput;
    [SerializeField] private AgentMover _agentMover;
    [SerializeField] private InteractionDetector _interactionDetector;
    [SerializeField] private Animator _animator;
    [SerializeField] private AnimationEvents _animationEvents;
    private ResourceSpawner _resourceSpawner;

    [Header("Held Tool")]
    [SerializeField] private GameObject _axeModel;
    [SerializeField] private GameObject _pickAxeModel;
    [SerializeField] private GameObject _woodModel;
    [SerializeField] private GameObject _stoneModel;

    private bool _isInteracting = false;
    private bool _isChopping = false;

    private NetworkVariable<ulong> _heldToolNetworkObjectId = new NetworkVariable<ulong>(ulong.MaxValue);
    private NetworkVariable<ObjectType> _heldToolObjectType = new NetworkVariable<ObjectType>(ObjectType.None);

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

    private void Awake()
    {
        _resourceSpawner = FindFirstObjectByType<ResourceSpawner>();
    }

    public override void OnNetworkSpawn()
    {
        _interactionDetector.Initialize(IsOwner);

        _heldToolObjectType.OnValueChanged += OnHeldToolObjectTypeChanged;

        HandleItemOnJoin();

        if (IsOwner)
        {
            _animationEvents.OnInteract += OnInteract;
            _animationEvents.OnAnimationDone += OnAnimationDone;
            _animationEvents.OnChop += OnChop;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            RequestDropCurrentItemServerRpc();
            _animationEvents.OnInteract -= OnInteract;
            _animationEvents.OnAnimationDone -= OnAnimationDone;
            _animationEvents.OnChop -= OnChop;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector2 movementInput = _playerInput.MovementInput;

        if (_isChopping || _isInteracting)
        {
            movementInput = Vector2.zero;
        }

        _agentMover.Move(movementInput);
    }

    private void OnPickUpPressed()
    {
        if (_isInteracting || _isChopping) return;

        if (_interactionDetector.ClosestInteractable == null) return;

        _animator.SetBool("Interact", true);
        _isInteracting = true;
    }

    private void OnInteractPressed()
    {
        if (!IsOwner) return;

        if (_isChopping || _isInteracting) return;

        if (_heldToolObjectType.Value is ObjectType.Axe or ObjectType.PickAxe)
        {
            _isChopping = true;
            _animator.SetTrigger("Chop");
        }
    }

    private void OnInteract()
    {
        if (_interactionDetector.ClosestInteractable is PickableBase)
        {
            RequestPickUpServerRpc(_interactionDetector.ClosestInteractable.NetworkObject.NetworkObjectId);
        }

        if (_interactionDetector.ClosestInteractable is ResourcePallet)
        {
            RequestGiveItemToPalletServerRpc(_interactionDetector.ClosestInteractable.NetworkObject.NetworkObjectId);
        }
    }

    private void OnAnimationDone()
    {
        _isInteracting = false;
        _isChopping = false;
    }

    private void OnChop()
    {
        if (_heldToolObjectType.Value is ObjectType.Axe or ObjectType.PickAxe)
        {
            if (_interactionDetector.ClosestInteractable is ResourceNode)
            {
                RequestResourceNodeInteractionServerRpc(_interactionDetector.ClosestInteractable.NetworkObject.NetworkObjectId);
            }
        }
    }

    private void OnHeldToolObjectTypeChanged(ObjectType previousValue, ObjectType newValue)
    {
        _axeModel.SetActive(newValue == ObjectType.Axe);
        _pickAxeModel.SetActive(newValue == ObjectType.PickAxe);
        _woodModel.SetActive(newValue == ObjectType.Wood);
        _stoneModel.SetActive(newValue == ObjectType.Stone);
    }

    private void HandleItemOnJoin()
    {
        if (_heldToolObjectType.Value != ObjectType.None)
        {
            OnHeldToolObjectTypeChanged(ObjectType.None, _heldToolObjectType.Value);
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestPickUpServerRpc(ulong networkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject pickable)) return;
        if (!pickable.TryGetComponent(out PickableBase pickableItem)) return;
        if (!pickableItem.CanBePickedUp) return;

        if (_heldToolObjectType.Value != ObjectType.None)
        {
            DropCurrentItem();
        }

        if (pickableItem is PickableTool)
        {
            _heldToolNetworkObjectId.Value = networkObjectId;
        }

        _heldToolObjectType.Value = pickableItem.ObjectType;
        pickableItem.PickUp();
    }

    [Rpc(SendTo.Server)]
    private void RequestGiveItemToPalletServerRpc(ulong networkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject pallet)) return;
        if (!pallet.TryGetComponent(out ResourcePallet resourcePallet)) return;

        if (resourcePallet.Interact(_heldToolObjectType.Value))
        {
            _heldToolObjectType.Value = ObjectType.None;
            _heldToolNetworkObjectId.Value = ulong.MaxValue;
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestResourceNodeInteractionServerRpc(ulong networkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject target)) return;
        if (!target.TryGetComponent(out ResourceNode resourceNode)) return;

        resourceNode.Harvest(_heldToolObjectType.Value);
    }

    [Rpc(SendTo.Server)]
    private void RequestDropCurrentItemServerRpc()
    {
        DropCurrentItem();
    }

    private void DropCurrentItem()
    {
        if (!IsServer) return;

        if (_heldToolObjectType.Value == ObjectType.None)
        {
            _heldToolNetworkObjectId.Value = ulong.MaxValue;
            return;
        }

        if (_heldToolObjectType.Value is ObjectType.Axe or ObjectType.PickAxe)
        {
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(_heldToolNetworkObjectId.Value, out NetworkObject target))
            {
                if (target.TryGetComponent(out PickableTool pickableItem))
                {
                    pickableItem.Drop(transform.position);
                }
            }
        }
        else
        {
            _resourceSpawner.SpawnResource(_heldToolObjectType.Value, transform.position);
        }

        _heldToolObjectType.Value = ObjectType.None;
        _heldToolNetworkObjectId.Value = ulong.MaxValue;
    }
}
