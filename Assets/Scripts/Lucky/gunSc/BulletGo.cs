using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//子弹脚本，子弹效果和碰撞未添加。
//子弹rigidbody需调整为y轴向前
public class BulletGo : MonoBehaviour
{
    //速度
    public GameObject gun;
    public float speed;
    public float maxdistance;
    public float Damage;
    public int pierceLv;
    public float recoilForce;
    //影响自由落体，未设计
    public float mass;
    private Rigidbody body;
    private float _damage;
    public string tag_str;
    public string layer_str;
    public List<string> part_Name;
    public damageContor temp_obj;
    public List<float> damage_scale;

    //激活即发射
    private void OnEnable()
    {
        body = this.GetComponent<Rigidbody>();
        //mass设置
        body.mass = mass;
        //速度归零
        body.velocity = Vector3.zero;
        //速度初始化
        body.AddForce(transform.up * speed);
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(layer_str))
        {
            Debug.Log("击中");
            var index = part_Name.IndexOf(collision.gameObject.name);
            try
            { temp_obj.gethurt(Damage * damage_scale[index], pierceLv);
            } catch
            {
                Debug.Log(index+ collision.gameObject.name);
            }
        }
        this.gameObject.SetActive(false);
    }
}
