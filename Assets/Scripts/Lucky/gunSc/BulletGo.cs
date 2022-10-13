using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//�ӵ��ű����ӵ�Ч������ײδ��ӡ�
//�ӵ�rigidbody�����Ϊy����ǰ
public class BulletGo : MonoBehaviour
{
    //�ٶ�
    public GameObject gun;
    public float speed;
    public float maxdistance;
    public float Damage;
    public int pierceLv;
    public float recoilForce;
    //Ӱ���������壬δ���
    public float mass;
    private Rigidbody body;
    private float _damage;
    public string tag_str;
    public string layer_str;
    public List<string> part_Name;
    public damageContor temp_obj;
    public List<float> damage_scale;

    //�������
    private void OnEnable()
    {
        body = this.GetComponent<Rigidbody>();
        //mass����
        body.mass = mass;
        //�ٶȹ���
        body.velocity = Vector3.zero;
        //�ٶȳ�ʼ��
        body.AddForce(transform.up * speed);
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(layer_str))
        {
            Debug.Log("����");
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
