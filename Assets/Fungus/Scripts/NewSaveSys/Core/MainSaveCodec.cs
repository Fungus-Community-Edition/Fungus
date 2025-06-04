using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    public class MainSaveCodec : MonoBehaviour, ISaveCodec
    {
        public virtual int Order => 0;
        public virtual bool NeedsInput => false;

        public object ToMakeFrom { get; set; } = null;

        [SerializeField] protected List<SaveCodec> subEncoders = new List<SaveCodec>();

        public virtual SaveDataUnit EncodeToUnit(object toMakeFrom)
        {
            CompositeSaveData result = Encode(toMakeFrom);
            return result.Serialized();
        }

        public virtual CompositeSaveData Encode(object toMakeFrom)
        {
            CompositeSaveData result = new CompositeSaveData();
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

        public SaveDataUnit Encode(CompositeSaveData toMakeFrom = null)
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

        public IList<SaveDataUnit> FindAndEncodeAll()
        {
            throw new System.NotImplementedException();
        }
    }
}