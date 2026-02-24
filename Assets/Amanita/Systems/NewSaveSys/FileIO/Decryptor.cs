using Amanita.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using Amanita.IO;
using FullSerializer;
using Amanita.FSExt;
using Collections;

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
        protected static string[] delimiterArr = SaveDiskAccessor.ReadWriteDelimiters;

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
                plainJson = plainText[..^SaveDiskAccessor.CompletionMarkers.Length]; 
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
                    throw new IOException(errorMessage);
                }
                // The marker should only take up one line at the end of the plain text. Thus, the whole
                // plain json should be everything up to the marker. And thus to extract said json, we can
                // just take the text up to the marker.
                string firstMarker = SaveDiskAccessor.CompletionMarkers[0];
                byte[] completionMarkerBytes = Encoding.GetBytes(firstMarker);
                int markerLength = completionMarkerBytes.Length;
                originalBytes = originalBytes.Take(originalBytes.Length - markerLength).ToArray();
                plainJson = Encoding.GetString(originalBytes);
            }

            bool shouldRemoveBOMAtTheStart = !string.IsNullOrEmpty(plainJson) && plainJson[0] == '\uFEFF';
            if (shouldRemoveBOMAtTheStart)
            {
                plainJson = plainJson[1..];
            }

            // For all we know, one or both halves of the data could be corrupted. Best validate that
            // here to avoid things getting messy
            IList<string> splitIntoJsons = plainJson.Split(delimiterArr, StringSplitOptions.None);

            ValidateSplit();
            void ValidateSplit()
            {
                string errorMessage = string.Empty;
                if (splitIntoJsons.Count != 2)
                {
                    errorMessage = "Invalid json passed. Expected exactly one delimiter separating metadata and main state.";
                    throw new ArgumentException(errorMessage);
                }

                string metaDataRead = splitIntoJsons[0], mainDataRead = splitIntoJsons[1];
                SaveMetaData metaObj = new SaveMetaData();
                CompositeSaveData saveDataObj = new CompositeSaveData();
                bool validMeta = JsonHelpers.TryFromJsonOverwrite(metaDataRead, ref metaObj);
                bool validMain = JsonHelpers.TryFromJsonOverwrite(mainDataRead, ref saveDataObj);
                errorMessage = string.Empty;
                if (!validMeta)
                {
                    errorMessage += "Corrupted meta detected. ";
                }
                if (!validMain)
                {
                    errorMessage += "Corrupted main state detected.";
                }

                if (errorMessage != string.Empty)
                {
                    throw new InvalidDataException(errorMessage);
                }

            }

            return plainJson;
        }

        protected virtual bool EndsWithCompletionMarker(string text)
        {
            var allMarkers = SaveDiskAccessor.CompletionMarkers;
            for (int i = 0; i < allMarkers.Length; i++)
            {
                string marker = allMarkers[i];
                if (text.EndsWith(marker))
                {
                    return true;
                }
            }

            return false;
        }

        // When encrypted, the marker should be in the form of bytes instead of plain text.
        protected virtual bool EndsWithCompletionMarker(byte[] bytes)
        {
            // We'll need to go through each completion marker, convert it to bytes, and
            // check if the end of the given byte array matches any of them. If it matches
            // at least one, then we can say that the bytes end with a completion marker.
            var allMarkers = SaveDiskAccessor.CompletionMarkers;

            for (int i = 0; i < allMarkers.Length; i++)
            {
                string marker = allMarkers[i];
                byte[] markerBytes = Encoding.GetBytes(marker);
                if (bytes.EndsWith(markerBytes))
                {
                    return true;
                }
            }

            return false;
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
            Exception exception = null;

            // We expect to be given an array with the raw string as the first elem, 
            // and whether it's already json or not
            
            if (input == null)
            {
                errorMessage = "Null input given to decryptor.";
                exception = new NullReferenceException(errorMessage);
            }

            else if (input == null || input is not BaseDecryptionRequest)
            {
                errorMessage = "Decryptor given wrong variety of input.";
                exception = new ArgumentException(errorMessage);
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
                // FullSerializer is not thread-safe; lock the shared serializer instance.
                lock (AmanitaManager.DefaultSerializer)
                {
                    result = Serializer.FromJson<SaveMetaData>(jsonForMetadata);
                }
            }
            else
            {
                using (var countdown = new CountdownEvent(1))
                {
                    Exception threadException = null;
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        try
                        {
                            // Still lock here to avoid overlapping with other main-thread FS work.
                            lock (AmanitaManager.DefaultSerializer)
                            {
                                result = Serializer.FromJson<SaveMetaData>(jsonForMetadata);
                            }
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
            Debug.Log("Decrypted into plain json");
            ISaveData result = DecryptMainState(fullReadableJson);
            
            return result;
        }

        protected virtual ISaveData DecryptMainState(string fullPlainJson)
        {
            IList<string> splitIntoJsons = fullPlainJson.Split(delimiterArr, StringSplitOptions.None);
            string jsonForMainState = splitIntoJsons[1];
            CompositeSaveData compositeSaveDataRead = new CompositeSaveData();
            bool validJson = JsonHelpers.TryFromJsonOverwrite(jsonForMainState, ref compositeSaveDataRead);

            if (!validJson)
            {
                string errorMessage = "Invalid json found.";
                throw new InvalidDataException(errorMessage);
            }
            ISaveData result = compositeSaveDataRead;
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

            ISaveMetaData meta;
            ISaveData mainState;

            // FullSerializer calls must be serialized to avoid concurrent mutations of internal caches.
            lock (AmanitaManager.DefaultSerializer)
            {
                meta = Serializer.FromJson<SaveMetaData>(jsonForMeta);
                mainState = Serializer.FromJson<CompositeSaveData>(jsonForMainState);
            }

            ISaveDataSet result = new SaveDataSet(meta, mainState);
            return result;
        }

        protected fsSerializer Serializer => AmanitaManager.DefaultSerializer;

    }

}