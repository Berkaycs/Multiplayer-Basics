using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;

public class ResourceNode : NetworkBehaviour, IInteractable
{
    [SerializeField] private SelectionOutline _selectionOutline;
    [SerializeField] private ComponentController _componentController;
    [SerializeField] private List<ObjectType> _toolTypeRequired;
    [SerializeField] private ObjectType _producedObjectType;
    [SerializeField] private int _amountToSpawn = 3;
    [SerializeField] private InteractAnimation _interactAnimation;
    [SerializeField] private ItemsAudio _itemsAudio;

    private NetworkVariable<int> _health = new NetworkVariable<int>(3);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _health.Value = 3;
        }
    }

    public void Harvest(ObjectType toolType)
    {
        if (!IsServer) return;

        if (_toolTypeRequired.Contains(toolType))
        {
            _health.Value--;
            PlayAudioClientRpc();
            
            if (_health.Value > 0)
            {
                PlayAnimationClientRpc();
            }
            else
            {
                _componentController.SetEnabled(false);

                for (int i = 0; i < _amountToSpawn; i++)
                {
                    Vector3 position = transform.position;
                    position.y += 0f;
                    Vector2 offset = UnityEngine.Random.insideUnitCircle;
                    position.x += offset.x;
                    position.z += offset.y;
                    Debug.Log($"Spawning item at {position}");
                }
            }
        }
    }

    public void ToggleSelection(bool isSelected)
    {
        if (_selectionOutline != null)
        {
            _selectionOutline.ToggleOutline(isSelected);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayAudioClientRpc()
    {
        if (_itemsAudio != null)
        {
            if (_health.Value > 0)
            {
                _itemsAudio.PlaySound();
            }
            else
            {
                _itemsAudio.PlaySoundSeparate();
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayAnimationClientRpc()
    {
        if (_interactAnimation != null)
        {   
            _interactAnimation.Shake();
        }
    }
}