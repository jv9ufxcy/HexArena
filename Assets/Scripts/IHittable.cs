using System.Numerics;

internal interface IHittable
{
    void Hit(int dam,int effect,int bounceLvl, UnityEngine.Vector2 dir);
}