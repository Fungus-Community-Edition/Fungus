using System.Collections.Generic;
using UnityEngine;
using IVariable = Amanita.VScripting.Variable;

namespace Amanita.SaveSys
{
    public class CodecRegistry
    {
        private static readonly List<IVarCodec> codecs = new()
        {
            new NumericVarCodec(),
            new BooleanVarCodec(),
            new StringVarCodec(),
            new VectorVarCodec(),
            new ColorVarCodec(),
            new TransformVarCodec(),
            
            /* ... */
        };

        public static IVarCodec GetCodec(IVariable variable)
            => codecs.Find(s => s.CanHandle(variable));

        public static IVarCodec GetCodec(VariableSaveData saveData)
            => codecs.Find(s => s.CanHandle(saveData));

        public static IVarCodec GetCodec(string typeName)
            => codecs.Find(s => s.CanHandle(typeName));

        public static void RegisterCodec(IVarCodec encoder)
        {
            if (encoder == null)
            {
                Debug.LogError("EncoderRegistry: Attempted to register a null encoder.");
                return;
            }

            if (codecs.Contains(encoder))
            {
                Debug.LogWarning($"EncoderRegistry: Encoder {encoder.GetType().Name} is already registered.");
                return;
            }

            codecs.Add(encoder);
        }

        public static void UNregisterCodec(IVarCodec encoder)
        {
            if (encoder == null)
            {
                Debug.LogError("EncoderRegistry: Attempted to register a null encoder.");
                return;
            }

            codecs.Remove(encoder);
        }
    }
}