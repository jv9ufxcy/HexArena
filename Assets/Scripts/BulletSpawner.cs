using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Runtime.CompilerServices;

public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] projectiles;
    [SerializeField] private BulletWave wave;
    [SerializeField] private float curTime, maxTime, cookTime = 1;
    private SpriteRenderer rend;
    private bool cooked = false;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponentInChildren<SpriteRenderer>();
        curTime = maxTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameEngine.hitStop<=0)
        {
            if (curTime > 0)
            {
                curTime -= Time.deltaTime;
            }
            else
            {
                FireBulletWave(wave);
                Destroy(gameObject);
            }
            if (curTime <= cookTime&&!cooked)
            {
                rend.transform.DOPunchScale(new Vector3 (1f,1.25f,1f), cookTime / 4);
                rend.transform.DOPunchScale(new Vector3(1.25f, 1f, 1f), cookTime / 4).SetDelay(cookTime / 4);
                rend.transform.DOPunchScale(new Vector3(1f, 1.25f, 1f), cookTime / 4).SetDelay(cookTime / 2);
                cooked = true;
            }
        }
    }
    float[] rotations;
    private void FireBulletWave(BulletWave wave)
    {
        rotations = new float[(int)wave.numberOfBullets];
        Vector2 velocity = new Vector2(wave.velocityX, wave.velocityY);

        if (wave.numberOfBullets > 1)
        {
            if (wave.isRandom != 0)
                RandomRotations((int)wave.numberOfBullets, wave.minRot, wave.maxRot);
            else
                DistributedRotations((int)wave.numberOfBullets, wave.minRot, wave.maxRot);
        }
        else
        {
            for (int i = 0; i < wave.numberOfBullets; i++)
            {
                rotations[i] = wave.minRot;
            }
        }
        SpawnBullets((int)wave.bulletIndex, (int)wave.numberOfBullets, wave.speed, velocity);
    }
    // Select a random rotation from min to max for each bullet
    public float[] RandomRotations(int numberOfBullets, float minRotation, float maxRotation)
    {
        for (int i = 0; i < numberOfBullets; i++)
        {
            rotations[i] = UnityEngine.Random.Range(minRotation, maxRotation);
        }
        return rotations;

    }

    // This will set random rotations evenly distributed between the min and max Rotation.
    public float[] DistributedRotations(int numberOfBullets, float minRotation, float maxRotation)
    {
        for (int i = 0; i < numberOfBullets; i++)
        {
            var fraction = (float)i / ((float)numberOfBullets - 1);
            var difference = maxRotation - minRotation;
            var fractionOfDifference = fraction * difference;
            rotations[i] = fractionOfDifference + minRotation; // We add minRotation to undo Difference
        }
        //foreach (var r in rotations) print(r);
        return rotations;
    }
    public GameObject[] SpawnBullets(int bulletResource, int numberOfBullets, float bulletSpeed, Vector2 bulletVelocity)
    {
        // Spawn Bullets
        GameObject[] spawnedBullets = new GameObject[numberOfBullets];
        for (int i = 0; i < numberOfBullets; i++)
        {
            spawnedBullets[i] = Instantiate(projectiles[bulletResource], transform.position, Quaternion.identity);

            var b = spawnedBullets[i].GetComponent<Bullet>();
            b.rotation = rotations[i];
            b.speed = bulletSpeed;
            b.velocity = bulletVelocity;
            b.owner = this.gameObject;
        }
        return spawnedBullets;
    }
}
[Serializable]
public class BulletWave
{
    public float bulletIndex;
    public float numberOfBullets;
    public float speed;
    public float velocityX;
    public float velocityY;
    public float minRot;
    public float maxRot;
    public float isRandom;
}
