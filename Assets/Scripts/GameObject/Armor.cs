using System;
using Newtonsoft.Json.Linq;

namespace ProjectCrovus
{
    public class Armor : ProjectCrovus.InternalGameObject
    {
        public enum Status
        {

        };

        public enum Type
        {
            HELMET, INNER_UNIFORM, BULLETPROOF_VEST, COMBAT_ARMOR, EXO_SKELETON, EXO_COMBAT_SUIT, ARMOR_MOD
        }

        private Type _type;

        private int _defense;
        private double _noize;
        private double _puncture_prob;

        public Armor(JObject jsonObj) : base(InternalGameObject.Type.EQUIPMENT, jsonObj)
        {
        }

        public override string ItemID()
        {
            throw new NotImplementedException();
        }
    }

}
