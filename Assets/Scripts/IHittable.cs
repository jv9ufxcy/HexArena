using System.Numerics;

internal interface IHittable
{
    void Hit(int dam,int effect, UnityEngine.Vector2 dir);
}