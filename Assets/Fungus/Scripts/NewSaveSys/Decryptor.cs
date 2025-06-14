using Amanita.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Amanita.SaveSys
{
    /// <summary>
    /// Handles the decryption algorithm that SaveWriters will use. If you want
    /// serious encryption that does more than prevent casual snooping, you'd
    /// best go with another ScriptableObject that implements IDecryptor.
    /// Note that this class expects that the save file uses JSON.
    /// </summary>
    public class Decryptor : ScriptableObject, IDecryptor
    {
        protected virtual void OnEnable()
        {
            delimiterArr = new string[] { DelimiterText };
        }

        protected static string[] delimiterArr;
        protected static string DelimiterText => "\n\n<<letUsSeparateTheDataGoodSir,OrMyNameIsNotWeeweeMaximus>>\n\n";

        /// <summary>
        /// What we expect the client's input to be is an object array with the 
        /// first element being the raw text, and the second argument letting us
        /// know whether it already is full readable json.
        public virtual ISaveMetaData DecryptMeta(object input)
        {
            string fullReadableJson = DecryptIntoPlainJson(input);
            ISaveMetaData result = DecryptMeta(fullReadableJson);
            return result;
        }

        protected string DecryptIntoPlainJson(object input)
        {
            Validate(input, out byte[] rawBytes, out bool writtenAsPlainText);
            string plainJson;

            if (writtenAsPlainText)
            {
                plainJson = Encoding.GetString(rawBytes);
            }
            else
            {
                byte key = 0xAA;
                // ^We assume that the original encryption was UTF8 outputting
                // a byte array with the bytes shifted by this exact key.
                byte[] originalBytes = rawBytes.Select(b => (byte)(b ^ key))
                    .ToArray();
                plainJson = Encoding.GetString(originalBytes);
            }

            bool shouldRemoveBOMAtTheStart = !string.IsNullOrEmpty(plainJson) && plainJson[0] == '\uFEFF';
            if (shouldRemoveBOMAtTheStart)
            {
                plainJson = plainJson[1..];
            }

            return plainJson;
        }

        protected Encoding Encoding => Encoding.UTF8;

        /// <summary>
        /// Checks if the input is legit. If so, it sets the passed objArray to
        /// what we expected it to be to begin with. Otherwise, throws exceptions.
        /// </summary>
        protected virtual void Validate(object input, out byte[] rawBytes, out bool writtenAsPlainText)
        {
            object[] objArray = input as object[];
            string errorMessage;
            System.Exception exception = null;

            // We expect to be given an array with the raw string as the first elem, 
            // and whether it's already json or not
            
            if (input == null)
            {
                errorMessage = "Null input given to decryptor.";
                exception = new System.NullReferenceException(errorMessage);
            }

            else if (objArray == null ||
                objArray.Length != expectedInputArgCount ||
                objArray[0] is not byte[] ||
                objArray[1] is not bool)
            {
                errorMessage = "Decryptor given wrong variety of input.";
                exception = new System.ArgumentException(errorMessage);
            }

            if (exception != null)
            {
                throw exception;
            }

            rawBytes = (byte[])objArray[0];
            writtenAsPlainText = (bool)objArray[1];
        }

        protected static int expectedInputArgCount = 2;

        protected virtual ISaveMetaData DecryptMeta(string fullPlainJson)
        {
            IList<string> splitIntoJsons = fullPlainJson.Split(delimiterArr, StringSplitOptions.None);
            string jsonForMetadata = splitIntoJsons[0];
            ISaveMetaData result = null;

            // Need to be careful with threads here due to how SaveMetaData's constructor
            // calls stuff that is not thread-safe.
            if (UnityThreadUtil.IsMainThread)
            {
                result = JsonUtility.FromJson<SaveMetaData>(jsonForMetadata);
            }
            else
            {
                using (var countdown = new CountdownEvent(1))
                {
                    Exception threadException = null;
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        // We want to make sure that any exceptions get thrown back to this thread,
                        // so we catch them here.
                        try
                        {
                            result = JsonUtility.FromJson<SaveMetaData>(jsonForMetadata);
                        }
                        catch (Exception ex)
                        {
                            threadException = ex;
                        }
                        finally
                        {
                            countdown.Signal();
                        }
                    });
                    countdown.Wait();
                    if (threadException != null)
                    {
                        throw threadException;
                    }
                }
            }

            return result;
        }

        public ISaveData DecryptMainState(object input)
        {
            string fullReadableJson = DecryptIntoPlainJson(input);
            ISaveData result = DecryptMainState(fullReadableJson);
            
            return result;
        }

        protected virtual ISaveData DecryptMainState(string fullPlainJson)
        {
            IList<string> splitIntoJsons = fullPlainJson.Split(delimiterArr, StringSplitOptions.None);

            ValidateSplit();
            void ValidateSplit()
            {
                if (splitIntoJsons.Count < 2)
                {
                    string errorMessage = "Invalid json passed.";
                    throw new System.ArgumentException(errorMessage);
                }
            }

            string jsonForMainState = splitIntoJsons[1];
            ISaveData result = JsonUtility.FromJson<CompositeSaveData>(jsonForMainState);
            return result;
        }

        public ISaveDataSet DecryptWholeSet(object input)
        {
            string fullReadableJson = DecryptIntoPlainJson(input);
            ISaveDataSet result = DecryptWholeSet(fullReadableJson);
            return result;
        }

        protected virtual ISaveDataSet DecryptWholeSet(string fullPlainJson)
        {
            IList<string> splitIntoJsons = fullPlainJson.Split(delimiterArr, StringSplitOptions.None);
            string jsonForMeta = splitIntoJsons[0];
            string jsonForMainState = splitIntoJsons[1];

            ISaveMetaData meta = JsonUtility.FromJson<SaveMetaData>(jsonForMeta);
            ISaveData mainState = JsonUtility.FromJson<SaveData>(jsonForMainState);

            ISaveDataSet result = new SaveDataSet(meta, mainState);
            return result;
        }

    }

}