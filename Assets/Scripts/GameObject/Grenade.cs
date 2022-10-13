using System;
using Newtonsoft.Json.Linq;

namespace ProjectCrovus
{
    public class Grenade : ProjectCrovus.InternalGameObject
    {
        private int _armorPenetration;

        private int _noizeWhenUsed;

        private int _noizeWhenExploded;

        private double _explosionRadius;

        private double _explosionDamage;


        public Grenade(JObject jsonObj) : base(Type.AMMUNATION, jsonObj)
        {

        }

        public override string ItemID()
        {
            throw new NotImplementedException();
        }

    }

}
