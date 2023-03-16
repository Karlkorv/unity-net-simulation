using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Code
{
    public class SphereSpawner : MonoBehaviour
    {
        private class SphereState
        {
            internal readonly float SpawnTime;

            internal readonly GameObject Sphere;

            internal SphereState(GameObject sphere)
            {
                this.Sphere = sphere;
                this.SpawnTime = Time.time;
            }
        }

        [SerializeField] private GameObject spherePrefab = null;

        [SerializeField] private float ballLifeTime = 5.0f;

        private float timeSinceLastSpawn;
        private List<SphereState> spawnedSpheres;

        public void Start()
        {
            timeSinceLastSpawn = Time.time;
            spawnedSpheres = new List<SphereState>();
        }

        private void SpawnSphere()
        {
            var sphere = Instantiate(spherePrefab, transform);
            timeSinceLastSpawn = Time.time;
            spawnedSpheres.Add(new SphereState(sphere));
        }

        public void Update()
        {
            if (Input.GetMouseButton(0) && Time.time - timeSinceLastSpawn > 0.5f)
            {
                Debug.Log("Spawning sphere at position" + transform.position);
                SpawnSphere();
            }

            using var iterator = spawnedSpheres.GetEnumerator();
            foreach (var sphere in spawnedSpheres)
            {
                if (Time.time - sphere.SpawnTime > ballLifeTime)
                {
                    Destroy(sphere.Sphere);
                }
            }

            spawnedSpheres.RemoveAll(state => state.Sphere == null);
        }
    }
}