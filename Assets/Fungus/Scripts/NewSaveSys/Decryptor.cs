using Amanita.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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
    [CreateAssetMenu(fileName = "NewDefaultDecryptor", menuName = "Amanita/SaveSys/DefaultDecryptor", order = 1)]
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
            Validate(input, out BaseDecryptionRequest decryptionReq);

            bool writtenAsPlainText = decryptionReq.WrittenAsPlainText;
            byte[] rawBytes = decryptionReq.RawBytes;
            string plainText;
            string plainJson;

            if (writtenAsPlainText)
            {
                plainText = Encoding.GetString(decryptionReq.RawBytes);

                // The text should have the completion marker. If it doesn't, we want
                // to throw an exception.
                if (!EndsWithCompletionMarker(plainText))
                {
                    string errorMessage = "Decrypted text does not end with the expected completion marker.";
                    throw new IOException(errorMessage);
                }

                // The marker should only take up one line at the end of the plain text. Thus, the whole
                // plain json should be everything up to the marker. And thus to extract said json, we can
                // just take the text up to the marker.
                plainJson = plainText[..^SaveDiskAccessor.CompletionMarker.Length]; 
                // ^Equals plainText.Substring(0, plainText.Length - SaveDiskAccessor.CompletionMarker.Length);

            }
            else
            {
                byte key = 0xAA;
                // ^We assume that the original encryption was UTF8 outputting
                // a byte array with the bytes shifted by this exact key.
                byte[] originalBytes = rawBytes.Select(b => (byte)(b ^ key))
                    .ToArray();

                // We need to check if the bytes end with the completion marker.
                if (!EndsWithCompletionMarker(originalBytes))
                {
                    string errorMessage = "Decrypted bytes do not end with the expected completion marker.";
                    throw new System.ArgumentException(errorMessage);
                }
                // The marker should only take up one line at the end of the plain text. Thus, the whole
                // plain json should be everything up to the marker. And thus to extract said json, we can
                // just take the text up to the marker.
                byte[] completionMarkerBytes = Encoding.GetBytes(SaveDiskAccessor.CompletionMarker);
                int markerLength = completionMarkerBytes.Length;
                originalBytes = originalBytes.Take(originalBytes.Length - markerLength).ToArray();
                plainJson = Encoding.GetString(originalBytes);
            }

            bool shouldRemoveBOMAtTheStart = !string.IsNullOrEmpty(plainJson) && plainJson[0] == '\uFEFF';
            if (shouldRemoveBOMAtTheStart)
            {
                plainJson = plainJson[1..];
            }

            return plainJson;
        }

        protected virtual bool EndsWithCompletionMarker(string text)
        {
            return text.EndsWith(SaveDiskAccessor.CompletionMarker);
        }

        // When encrypted, the marker should be in the form of bytes instead of plain text.
        protected virtual bool EndsWithCompletionMarker(byte[] bytes)
        {
            byte[] completionMarkerBytes = Encoding.GetBytes(SaveDiskAccessor.CompletionMarker);

            if (bytes.Length < completionMarkerBytes.Length)
                return false;

            return bytes.Skip(bytes.Length - completionMarkerBytes.Length).SequenceEqual(completionMarkerBytes);
        }

        protected Encoding Encoding => Encoding.UTF8;

        /// <summary>
        /// Checks if the input is legit. If so, it sets the passed objArray to
        /// what we expected it to be to begin with. Otherwise, throws exceptions.
        /// </summary>
        protected virtual void Validate(object input, out BaseDecryptionRequest decReq)
        {
            decReq = input as BaseDecryptionRequest;
            string errorMessage;
            System.Exception exception = null;

            // We expect to be given an array with the raw string as the first elem, 
            // and whether it's already json or not
            
            if (input == null)
            {
                errorMessage = "Null input given to decryptor.";
                exception = new System.NullReferenceException(errorMessage);
            }

            else if (input == null || input is not BaseDecryptionRequest)
            {
                errorMessage = "Decryptor given wrong variety of input.";
                exception = new System.ArgumentException(errorMessage);
            }

            if (exception != null)
            {
                throw exception;
            }
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

            result.OnDeserialize();
            Validate(result);
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
            result.OnDeserialize();
            Validate(result);
            return result;
        }

        protected virtual void Validate(ISaveData decryptedSaveData)
        {
            bool probablyInvalidJson = decryptedSaveData == null || string.IsNullOrEmpty(decryptedSaveData.TypeName);
            if (probablyInvalidJson)
            {
                throw new IOException("Decrypted save data is null or has no type name. " +
                    "This probably means that the decryption failed or the JSON was malformed.");
            }
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