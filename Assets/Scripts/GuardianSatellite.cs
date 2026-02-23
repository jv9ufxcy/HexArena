using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GuardianSatellite : MonoBehaviour
{
    public GameObject queen;
    [SerializeField]private float speed=4,angle,radius=1/*, lifeTime = 6*/;
    [SerializeField] private int damage = 64;

    // Update is called once per frame
    void FixedUpdate()
    {
        Satellite();
        //if (lifeTime>0)
        //{
        //    lifeTime-=Time.fixedDeltaTime;
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}
    }
    private void Satellite()
    {
        angle += speed * Time.deltaTime;

        var offset = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad)) * radius;
        transform.position = (Vector2)queen.transform.position + offset;//why NRE
    }
    public void SetQueen(GameObject objectToFollow)
    {
        queen = objectToFollow;
    }
    public void SetStartingAngle(float iceIndex, float totalSatellites)
    {
        float spacing = 360 / totalSatellites;//how much space should be between each block?
        angle = iceIndex * spacing;//set the angle to be evenly spaced based on which number it is and where it should be on the circle
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        IHittable hit = other.GetComponent<IHittable>();
        Vector2 direction = (this.transform.position-other.transform.position).normalized;
        if (hit != null && other.gameObject != queen)
        {
            hit.Hit(damage, 1, 0,direction);
        }
    }
}
