using System;
using Newtonsoft.Json.Linq;

namespace ProjectCrovus
{
    public class Ammo : ProjectCrovus.InternalGameObject, Attackable
    {
        public enum DamageType
        {

        }

        private string _itemID;

        public int Damage { get; private set; }

        public int BaseDamage { get; private set; }

        public int ArmorPenetration { private get; set; }

        public void Attack<T>(T gameObject) where T : Injureable
        {
            throw new NotImplementedException();
        }


        public Ammo(JObject jsonObj) : base(Type.AMMUNATION, jsonObj)
        {
            Damage = (int)jsonObj["damage"];
            _itemID = (string)jsonObj["item_id"];
            BaseDamage = (int)jsonObj["base_damage"];
        }

        public override string ItemID()
        {
            throw new NotImplementedException();
        }

    }

}
