using System;
using Newtonsoft.Json.Linq;

namespace ProjectCrovus
{
    public abstract class InternalGameObject
    {
        public enum Type
        {
            BARTER, MATERIAL, EQUIPMENT, CURRENCY, AMMUNATION, CONSUMABLE
        }

        public enum SubType
        {
            CRAP, DAILY_ELECTRICO, SECURE_INTEL, RAW_MATERIAL, ENGINEERING_PARTS,
            ELECTRONIC_COMPONENT, ENERGY_COMPONENT, CHEMICAL_SUPPLIES, INTEGRATE_TOOLS,
            WEAPON, ARMOR, MICRO_CHIP, PROSTHETIC, WETWARE, OPERATING_DEVICE, BASIC_AMMO, SPECIAL_AMMO,
            LAUNCHER_ROUNDS, MEDICAL_SUPPLIES, DOPING, EQUIPMENT_MAINTAINCE, DEPOLYMENT_DEVICE
        }

        private Type _type;

        private SubType _subType;

        public string Name { get; private set; }

        public string Description { get; private set; }

        abstract public string ItemID();

        public InternalGameObject(Type type, JObject jsonObj)
        {
            this._type = type;
            this.Name = (string)jsonObj["name"];
            this.Description = (string)jsonObj["description"];
            this._subType = (SubType)Enum.Parse(typeof(SubType), (string)jsonObj["sub_type"]);
        }

        public string ObjectID()
        {
            return _type.ToString() + "_" + _subType.ToString() + "_" + ItemID();
        }
    }
}

