using Amanita.VScripting;
using System.Linq;
using UnityEngine;
using Type = System.Type;
using UnityObj = UnityEngine.Object;
using System.Collections.Generic;

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
            if (variable == null)
            {
                string errorMessage = "Variable passed to CanHandle is null. Cannot handle.";
                throw new System.NullReferenceException(errorMessage);
            }

            bool result = subCodecs.ContainsKey(variable.GetType());
            if (!result)
            {
                Debug.LogWarning($"No codec found for variable type {variable.GetType()}. Cannot handle.");
            }
            return result;
        }

        // Mapping of variable types to their respective codecs. Some variable types work
        // with the same codec type. Also note that we are only supporting Muscariables here.
        protected static IDictionary<Type, IVarCodec> subCodecs = new Dictionary<Type, IVarCodec>(new TypeNameComparer())
        {
            // Numerics
            { typeof(IntMuscariable),  new NumericVarCodec() },
            { typeof(FloatMuscariable), new NumericVarCodec() },
            { typeof(BoolMuscariable),  new BooleanVarCodec() },
            { typeof(VectorTwoMuscariable), new VectorVarCodec() },
            { typeof(VectorThreeMuscariable), new VectorVarCodec() },

            // Graphics
            { typeof(StringMuscariable), new StringVarCodec() },
            { typeof(ColorMuscariable), new ColorVarCodec() },
            // Commenting out for codecs we don't have yet
            //{ typeof(SpriteMuscariable), new SpriteVarCodec() },
            //{ typeof(MaterialMuscariable), new MaterialVarCodec() },
            //{ typeof(TextureMuscariable), new TextureVarCodec() },
            // { typeof(AnimatorMuscariable), new AnimatorVarCodec() },


            // Unity General
            //{ typeof(GameObjectMuscariable), new GameObjectVarCodec() },
            //{ typeof(UnityObjectMuscariable), new UnityObjVarCodec() },
            { typeof(TransformMuscariable), new TransformVarCodec() },

            // Audio
            //{ typeof(AudioClipMuscariable), new AudioClipVarCodec() },
            //{ typeof(AudioSourceMuscariable), new AudioSourceVarCodec() },
            
            
        };
        
        public bool CanHandle(VariableSaveData variable)
        {
            return CanHandle(variable.VarTypeName);
        }

        public bool CanHandle(string typeName)
        {
            bool result = subCodecs.Keys.Any(elem => elem.Name == typeName);
            return result;
        }

        public void Decode(IVariable toApplyStateTo, string stateAsStr)
        {
            if (!CanHandle(toApplyStateTo))
            {
                Debug.LogWarning($"No codec found for variable type {toApplyStateTo.GetType()}. Cannot decode.");
                return;
            }

            IVarCodec codec = subCodecs[toApplyStateTo.GetType()];
            codec.Decode(toApplyStateTo, stateAsStr);
        }

        public void Decode(IVariable variable, VariableSaveData data)
        {
            if (!CanHandle(variable))
            {
                Debug.LogWarning($"No codec found for variable type {variable.GetType()}. Cannot decode.");
                return;
            }

            IVarCodec codec = subCodecs[variable.GetType()];
            codec.Decode(variable, data);
        }

        public T DecodeTo<T>(string data)
        {
            Type varType = typeof(T);
            if (!subCodecs.ContainsKey(varType))
            {
                Debug.LogWarning($"No codec found for variable type {varType.Name}. Cannot decode.");
                return default;
            }
            IVarCodec codec = subCodecs[varType];
            return codec.DecodeTo<T>(data);
        }

        public VariableSaveData EncodeToSave(IVariable variable)
        {
            if (!CanHandle(variable))
            {
                Debug.LogWarning($"No codec found for variable type {variable.GetType()}. Cannot encode.");
                return null;
            }
            IVarCodec codec = subCodecs[variable.GetType()];
            return codec.EncodeToSave(variable);
        }

        public string EncodeToString(IVariable variable)
        {
            if (!CanHandle(variable))
            {
                Debug.LogWarning($"No codec found for variable type {variable.GetType()}. Cannot encode.");
                return string.Empty;
            }

            IVarCodec codec = subCodecs[variable.GetType()];
            return codec.EncodeToString(variable);
        }
    }
}