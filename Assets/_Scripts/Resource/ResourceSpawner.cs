using UnityEngine;
using Unity.Netcode;

public class ResourceSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject _woodResourcePrefab;
    [SerializeField] private NetworkObject _stoneResourcePrefab;

    public void SpawnResource(ObjectType objectType, Vector3 position)
    {
        if (!IsServer) return;

        NetworkObject resource = Instantiate(
            objectType == ObjectType.Wood ? _woodResourcePrefab : _stoneResourcePrefab, 
            position, 
            Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f)
        );
        
        resource.GetComponent<NetworkObject>().Spawn();
    }
}
