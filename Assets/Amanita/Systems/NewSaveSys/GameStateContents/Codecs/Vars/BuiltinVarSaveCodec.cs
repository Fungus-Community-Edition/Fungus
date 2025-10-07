using Amanita.VScripting;
using System.Linq;
using UnityEngine;
using Type = System.Type;
using UnityObj = UnityEngine.Object;

namespace Amanita.SaveSys
{
    /// <summary>
    /// As it sounds: for saving the variable types built into Amanita's base package.
    /// This codec will not handle custom user variable types; you'll have to 
    /// code those yourself.
    /// </summary>
    public class BuiltinVarSaveCodec : ScriptableObject, IVarCodec
    {
        public bool CanHandle(IVariable variable)
        {
            bool result = false;

            if (variable == null)
            {
                Debug.LogWarning("Cannot handle a null variable.");
            }
            else
            {
                Type varType = variable.GetType();
                result = supportedVarTypes.Contains(varType);
                if (!result)
                {
                    Debug.LogWarning($"{this.GetType().Name} cannot handle variable type: {varType.Name}");
                }
            }

            return result;
        }

        // We are only including Muscariables, since we'll get the legacy vars converted to those
        protected static Type[] supportedVarTypes
        = new Type[]
        {
            // Numeric
            typeof(IntMuscariable),
            typeof(FloatMuscariable),
            typeof(VectorTwoMuscariable),
            typeof(VectorThreeMuscariable),
            typeof(BoolMuscariable),

            // Graphic
            typeof(StringMuscariable),
            typeof(ColorMuscariable),
            typeof(SpriteMuscariable),
            typeof(TextureMuscariable),
            typeof(MaterialMuscariable),
            typeof(AnimatorMuscariable),

            // UnityGeneral
            typeof(GameObjectMuscariable),
            typeof(TransformMuscariable),
            typeof(UnityObjectMuscariable),
            
            // Audio
            typeof(AudioClipMuscariable), 
            typeof(AudioSourceMuscariable),

        };

        public bool CanHandle(string typeName)
        {
            bool result = supportedVarTypes.Any(elem => elem.Name == typeName);
            if (!result)
            {
                Debug.LogWarning($"{this.GetType().Name} cannot handle variable type name: {typeName}");
            }
            return result;
        }

        public bool CanHandle(VariableSaveData variable)
        {
            return CanHandle(variable.TypeName);
        }

        public void Decode(IVariable toApplyStateTo, string stateAsStr)
        {
            throw new System.NotImplementedException();
        }

        public void Decode(IVariable variable, VariableSaveData data)
        {
            throw new System.NotImplementedException();
        }

        public T DecodeTo<T>(string data)
        {
            throw new System.NotImplementedException();
        }

        public VariableSaveData EncodeToSave(IVariable varable)
        {
            throw new System.NotImplementedException();
        }

        public string EncodeToString(IVariable variable)
        {
            throw new System.NotImplementedException();
        }
    }
}