using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    public class MainSaveEncoder : MonoBehaviour, ISaveEncoder
    {
        public virtual int Priority => 0;
        public virtual bool NeedsInput => false;

        public object ToMakeFrom { get; set; } = null;

        [SerializeField] protected List<SaveEncoder> subEncoders = new List<SaveEncoder>();

        public virtual SaveDataUnit EncodeToUnit(object toMakeFrom)
        {
            AmanitaSaveData result = Encode(toMakeFrom);
            return result.Serialized();
        }

        public virtual AmanitaSaveData Encode(object toMakeFrom)
        {
            AmanitaSaveData result = new AmanitaSaveData();
            foreach (var encoder in subEncoders)
            {
                // Some sub encoders are not supposed to take in any particular input;
                // they fetch the input themselves from the scene or other sources.
                if (encoder.CanHandle(toMakeFrom))
                {
                    SaveDataUnit unit = null; // encoder.EncodeToUnit(toMakeFrom);
                    result.Add(unit);
                }
            }

            return result;
        }

        public SaveDataUnit Encode(AmanitaSaveData toMakeFrom = null)
        {
            throw new System.NotImplementedException();
        }

        

        public IList<SaveDataUnit> EncodeMulti(object toMakeFrom = null)
        {
            throw new System.NotImplementedException();
        }

        public SaveDataUnit EncodeToUnit()
        {
            throw new System.NotImplementedException();
        }
    }
}