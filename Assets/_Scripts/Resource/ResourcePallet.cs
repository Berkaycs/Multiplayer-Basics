using UnityEngine;
using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections.Generic;

public class ResourcePallet : NetworkBehaviour, IInteractable
{
    public event Action OnPalletFilled;

    [Header("References")]
    [SerializeField] private SelectionOutline _selectionOutline;
    [SerializeField] private List<ComponentController> _componentControllers;
    [SerializeField] private ObjectType _acceptedObjectType;
    [SerializeField] private ItemsAudio _itemsAudio;

    private NetworkVariable<int> _stackedResources = new NetworkVariable<int>(0);
    public int StackedResources => _stackedResources.Value;

    public bool Interact(ObjectType objectType)
    {
        if (!IsServer) return false;
        if (objectType != _acceptedObjectType) return false;
        if (_stackedResources.Value >= _componentControllers.Count) return false;

        PlayedAudioClientRpc();
        _componentControllers[_stackedResources.Value].SetEnabled(true);
        _stackedResources.Value++;
        
        if (_stackedResources.Value >= _componentControllers.Count)
        {
            OnPalletFilled?.Invoke();
        }

        return true;
    }

    public void ToggleSelection(bool isSelected)
    {
        if (_selectionOutline != null)
        {
            _selectionOutline.ToggleOutline(isSelected);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayedAudioClientRpc()
    {
        if (_itemsAudio != null)
        {
            _itemsAudio.PlaySound();
        }
    }
}
