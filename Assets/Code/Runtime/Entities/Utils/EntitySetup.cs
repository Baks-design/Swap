using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;

namespace SwapChains.Runtime.Entities
{
    public class EntitySetup : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] int numberOfAIToSpawn = 5;
        [SerializeField] Vector3 spawnAreaCenter;
        [SerializeField] Vector3 spawnAreaSize;
        [Header("Refs")]
        [SerializeField] PlayerController aiPrefab;
        readonly List<PlayerController> controllers;

        void Awake()
        {
            for (var i = 0; i < numberOfAIToSpawn; i++)
            {
                var randomPosition = GetRandomPositionInArea();
                var inst = Instantiate(aiPrefab, randomPosition, Quaternion.identity);
                controllers.Add(inst);
            }
            var r = Random.Range(0, controllers.Count);
            controllers[r].TryGetComponent<PlayerController>(out var controller);
            controller.NPCComponentsHandle(true);
        }

        Vector3 GetRandomPositionInArea()
        {
            Vector3 pos;
            pos.x = Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2f, spawnAreaCenter.x + spawnAreaSize.x / 2f);
            pos.y = spawnAreaCenter.y;
            pos.z = Random.Range(spawnAreaCenter.z - spawnAreaSize.z / 2f, spawnAreaCenter.z + spawnAreaSize.z / 2f);
            return pos;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
        }
    }
}