using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BaseObject = System.Object;
using System.Text;

namespace Amanita.SaveSys
{
    /// <summary>
    /// Handles the encryption algorithm that SaveWriters will use. If you want
    /// serious encryption that does more than prevent casual snooping, you'd
    /// best go with another ScriptableObject that implements IEncryptor.
    /// </summary>
    public class Encryptor : ScriptableObject, IEncryptor
    {
        public virtual object GetOutput(object input)
        {
            SaveDataSet dataSet = input as SaveDataSet;
            Validate();
            void Validate()
            {
                string errorMessage = string.Empty;
                System.Exception exception = null;

                if (input == null)
                {
                    errorMessage = "Null input given to encryptor.";
                    exception = new System.NullReferenceException(errorMessage);
                }
                else if (input is not SaveDataSet)
                {
                    errorMessage = "Encryptor given wrong variety of input.";
                    exception = new System.ArgumentException(errorMessage);
                }

                if (exception != null)
                {
                    throw exception;
                }
            }

            string fullJson = GetFullTextToEncrypt();
            string GetFullTextToEncrypt()
            {
                string metaJson = JsonUtility.ToJson(dataSet.Meta, true);
                string mainStateJson = JsonUtility.ToJson(dataSet.MainState, true);
                string fullJson = $"{metaJson}{Delimiter}{mainStateJson}";
                return fullJson;
            }
            
            byte[] endResult = EncryptToBytes(fullJson);
            byte[] EncryptToBytes(string textToEncrypt)
            {
                byte key = 0xAA;
                byte[] result = Encoding.GetBytes(fullJson)
                    .Select(b => (byte)(b ^ key))
                    .ToArray();
                return result;
            }

            return endResult;
        }

        protected static string Delimiter => "\n\n<<letUsSeparateTheDataGoodSir,OrMyNameIsNotWeeweeMaximus>>\n\n";

        protected virtual Encoding Encoding => Encoding.UTF8;

    }
    
    public interface IEncryptor
    {
        object GetOutput(object input);
    }

    public interface IEncryptor<TInput, TOutput> : IEncryptor
    {
        TOutput GetOutput(TInput input);
    }
}