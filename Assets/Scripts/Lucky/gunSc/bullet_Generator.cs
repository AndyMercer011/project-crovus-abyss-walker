using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet_Generator : MonoBehaviour
{
    public GameObject baseBullet;
    public List<GameObject> Bullet_pool;
    private Transform _start;
    //private Vector3 _tagpos;
    public int poolMaxNub;
    public int _index;
    public gunType GunType;
    private GameObject _bullet;
    private void Start()
    {
        reSet();
    }
    public void shoot(Vector3 tag)
    {
        _bullet = BulletInstance();
        _bullet.SetActive(false);
        _bullet.transform.position = baseBullet.transform.position;
        _bullet.transform.rotation = baseBullet.transform.rotation;
        _bullet.SetActive(true);
        _index += 1;
    }

    private void reSet()
    {
        baseBullet.SetActive(false);
        _start = this.transform;
        Bullet_pool.Clear();
        _index = 0;
    }
    public GameObject BulletInstance()
    {
        
        
        try { return Bullet_pool[_index];
        }
        catch
        {
            if (_index < poolMaxNub)
            {
                var InitBullet = Instantiate(baseBullet, _start, false);
                Bullet_pool.Add(InitBullet);
                return Bullet_pool[_index];
            }
        }
        _index = 0;
        return Bullet_pool[_index];

    }
    public Vector3 RandomTagSet(Vector3 tag,float range)
    {

        tag.x += Random.Range(-range, range);
        tag.y += Random.Range(-range, range);
        tag.z += Random.Range(-range, range);
        return tag;
    }
    public enum gunType
    {
        longgun01,
        hangun01,
        spreadGun01
    }
    public enum bulletTpye
    {
        seven,
        five,
        spread
    }
    public struct GunInfo
    {
        public gunType gunType;
        public bulletTpye BulletTpye;
        public int maxBulletNumb;
        public float cooldown;
    }
}
