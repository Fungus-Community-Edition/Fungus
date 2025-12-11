using System.Linq;
using UnityEngine;
using System.Text;
using FullSerializer;
using Amanita.FSExt;

namespace Amanita.SaveSys
{
    /// <summary>
    /// Handles the encryption algorithm that SaveWriters will use. If you want
    /// serious encryption that does more than prevent casual snooping, you'd
    /// best go with another ScriptableObject that implements IEncryptor.
    /// </summary>
    [CreateAssetMenu(fileName = "DefaultEncryptor", menuName = "Amanita/SaveSys/DefaultEncryptor", order = 1)]
    public class Encryptor : ScriptableObject, IEncryptor
    {
        public virtual object GetOutput(object input)
        {
            BaseEncryptionRequest inputRequest = input as BaseEncryptionRequest;
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
                else if (input is not BaseEncryptionRequest)
                {
                    errorMessage = "Encryptor given wrong variety of input.";
                    exception = new System.ArgumentException(errorMessage);
                }
                else if (inputRequest.SaveDataSet == null)
                {
                    errorMessage = "Encryptor given a request with no SaveDataSet.";
                    exception = new System.ArgumentNullException(errorMessage);
                }
                else if (inputRequest.SaveDataSet.Meta == null)
                {
                    errorMessage = "Encryptor given a request with no SaveMetaData.";
                    exception = new System.NullReferenceException(errorMessage);
                }
                else if (inputRequest.SaveDataSet.MainState == null)
                {
                    errorMessage = "Encryptor given a request with no MainState.";
                    exception = new System.NullReferenceException(errorMessage);
                }
                else if (string.IsNullOrEmpty(inputRequest.CompletionMarker))
                {
                    errorMessage = "Encryptor given a request with no CompletionMarker.";
                    exception = new System.NullReferenceException(errorMessage);
                }

                if (exception != null)
                {
                    throw exception;
                }
            }

                SaveDataSet dataSet = inputRequest.SaveDataSet;
                string completionMarker = inputRequest.CompletionMarker;
                string fullJson = GetFullTextToEncrypt();
                string GetFullTextToEncrypt()
                {
                    lock (Serializer)
                    {
                        string metaJson = Serializer.ToJson(dataSet.Meta, true);
                        string mainStateJson = Serializer.ToJson(dataSet.MainState, true);

                        string fullJson = $"{metaJson}{Delimiter}{mainStateJson}{completionMarker}";
                        return fullJson;
                    }
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

        protected static string Delimiter => "<<letUsSeparateTheDataGoodSir,OrMyNameIsNotWeeweeMaximus>>";

        protected virtual Encoding Encoding => Encoding.UTF8;
        protected static fsSerializer Serializer => AmanitaManager.DefaultSerializer;

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