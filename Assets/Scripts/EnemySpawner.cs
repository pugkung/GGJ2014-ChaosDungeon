using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public float spawnRate;
    public GameObject spawnAnimation;
    public List<GameObject> spawnPool;

    private float tick;

    void Start()
    {
    }

    void Update()
    {
        if (tick > spawnRate)
        {
            int spawnIndex = Random.Range(0, spawnPool.Count - 1);
            if (spawnPool[spawnIndex])
            {
                float spawnY = Random.Range
                    (Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).y, 
                        Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height)).y);
                float spawnX = Random.Range
                    (Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).x, 
                        Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x);
 
                Vector2 spawnPosition = new Vector2(spawnX, spawnY);
                // GameObject.Instantiate(spawnPool[spawnIndex], spawnPosition, Quaternion.identity);
                GameObject target = GameObject.Instantiate(spawnAnimation, spawnPosition, Quaternion.identity);
                target.GetComponent<AnimationDelay>().assignSpawnObject(3, spawnPool[spawnIndex]);
            }
            tick = 0;
        }
        tick += UnityEngine.Time.deltaTime;
    }
}