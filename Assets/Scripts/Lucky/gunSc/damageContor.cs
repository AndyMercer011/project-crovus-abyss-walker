using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class damageContor : MonoBehaviour
{
    public Transform hp;
    public Transform armor;
    public float pierceLv_base;
    private float maxarmor = 500f;
    public float armor_value = 500f;
    private float maxhp=1000f;
    public float hp_value=1000f;
    void Update()
    {
        hp.localScale= new Vector3(hp_value / maxhp, 1f, 1f);
        armor.localScale=new Vector3(armor_value / maxarmor, 1f, 1f);
    }
    public void gethurt(float damage,int pierceLv)
    {
        
       var armDamage = pierceLv_base* (4-pierceLv)*damage;
        if (armor_value> armDamage)
        {
            armor_value -= armDamage;
            hp_value -= damage - armDamage;
        }
        else
        {
            hp_value += armor_value - damage;
        }
        Debug.Log("…À∫¶armDamage"+armDamage);
        if (hp_value <= 0f)
        {
            // this.enabled = false;
            armor_value = 500f;
            hp_value = 1000f;
        }
    }
}
