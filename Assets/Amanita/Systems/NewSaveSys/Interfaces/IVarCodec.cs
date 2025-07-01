using AmanitaVar = Amanita.Variable;

namespace Amanita.SaveSys
{
    public interface IVarCodec
    {
        bool CanHandle(AmanitaVar variable);
        bool CanHandle(string typeName);
        bool CanHandle(VariableSaveData variable);
        string EncodeToString(AmanitaVar variable);

        /// <summary>
        /// Decodes the specified data and applies the result to the given variable.
        /// </summary>
        void Decode(AmanitaVar variable, string data);

        /// <summary>
        /// Decodes the specified VariableSaveData and applies the result to the given AmanitaVar.`
        /// </summary>
        void Decode(AmanitaVar variable, VariableSaveData data);

        /// <summary>
        /// Decodes the specified data and returns the result as an object of type T. Will
        /// throw an exception if the type is not supported.
        /// </summary>
        T DecodeTo<T>(string data);

        VariableSaveData EncodeToSave(AmanitaVar varable);
    }


}