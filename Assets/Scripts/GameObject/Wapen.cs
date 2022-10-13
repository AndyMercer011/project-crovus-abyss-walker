using UnityEngine;
using Newtonsoft.Json.Linq;


namespace ProjectCrovus
{
    public class Wapen : ProjectCrovus.InternalGameObject
    {

        private int _rof;

        private int _baseRecoil;

        private int _fillingTime;

        private int _armorPenetration;

        private int _noizeWhenExploded;

        private double _explosionRadius;

        private double _explosionDamage;

        public Wapen(JObject jsonObj) : base(InternalGameObject.Type.EQUIPMENT, jsonObj)
        {
        }

        public override string ItemID()
        {
            throw new System.NotImplementedException();
        }
    }

}
