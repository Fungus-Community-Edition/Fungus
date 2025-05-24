using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    public class MainSaveEncoder : SaveEncoder, ISaveEncoder
    {

        new public virtual AmanitaSaveData Encode(object toMakeFrom)
        {
            AmanitaSaveData result = new AmanitaSaveData();
            foreach (var encoder in subEncoders as IEnumerable<ISaveEncoder>)
            {
                // Some sub encoders are not supposed to take in any particular input;
                // they fetch the input themselves from the scene or other sources.
                if (encoder.CanHandle(toMakeFrom))
                {
                    SaveDataUnit unit = encoder.Encode(toMakeFrom);
                    result.Add(unit);
                }
            }

            return result;
        }

        public IList<SaveDataUnit> EncodeMulti(IList<object> toMakeFrom)
        {
            throw new System.NotImplementedException();
        }


        public SaveDataUnit Encode(AmanitaSaveData toMakeFrom = null)
        {
            throw new System.NotImplementedException();
        }

        SaveDataUnit ISaveEncoder.Encode(object toMakeFrom)
        {
            return Encode(toMakeFrom as AmanitaSaveData);
        }

        public IList<SaveDataUnit> EncodeMulti(object toMakeFrom = null)
        {
            throw new System.NotImplementedException();
        }
    }
}