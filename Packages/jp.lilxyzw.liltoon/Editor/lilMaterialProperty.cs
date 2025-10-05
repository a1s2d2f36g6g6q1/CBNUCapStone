#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace lilToon
{
    internal class lilMaterialProperty
    {
        public HashSet<PropertyBlock> blocks;
        public bool isTexture;
        public MaterialProperty p;
        public string propertyName;

        public lilMaterialProperty()
        {
            p = null;
            blocks = new HashSet<PropertyBlock>();
            isTexture = false;
            propertyName = null;
        }

        public lilMaterialProperty(string name, params PropertyBlock[] inBrocks)
        {
            p = null;
            blocks = inBrocks.ToHashSet();
            isTexture = false;
            propertyName = name;
        }

        public lilMaterialProperty(string name, bool isTex, params PropertyBlock[] inBrocks)
        {
            p = null;
            blocks = inBrocks.ToHashSet();
            isTexture = isTex;
            propertyName = name;
        }

        public lilMaterialProperty(MaterialProperty prop)
        {
            p = prop;
        }

        public float floatValue
        {
            get => p.floatValue;
            set => p.floatValue = value;
        }

        public Vector4 vectorValue
        {
            get => p.vectorValue;
            set => p.vectorValue = value;
        }

        public Color colorValue
        {
            get => p.colorValue;
            set => p.colorValue = value;
        }

        public Texture textureValue
        {
            get => p.textureValue;
            set => p.textureValue = value;
        }

        // Other
        public string name
        {
            get => p.name;
            private set { }
        }

        public string displayName
        {
            get => p.displayName;
            private set { }
        }

        public MaterialProperty.PropFlags flags
        {
            get => p.flags;
            private set { }
        }

        public bool hasMixedValue
        {
            get => p.hasMixedValue;
            private set { }
        }

        public Vector2 rangeLimits
        {
            get => p.rangeLimits;
            private set { }
        }

        public Object[] targets
        {
            get => p.targets;
            private set { }
        }

        public TextureDimension textureDimension
        {
            get => p.textureDimension;
            private set { }
        }

        public MaterialProperty.PropType type
        {
            get => p.type;
            private set { }
        }

        public void FindProperty(MaterialProperty[] props)
        {
            p = props.FirstOrDefault(prop => prop != null && prop.name == propertyName);
        }

        public static implicit operator MaterialProperty(lilMaterialProperty prop)
        {
            return prop.p;
        }
    }
}
#endif