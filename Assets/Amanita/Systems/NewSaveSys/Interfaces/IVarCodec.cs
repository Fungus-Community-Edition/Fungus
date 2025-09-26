using Amanita.VScripting;

namespace Amanita.SaveSys
{
    public interface IVarCodec
    {
        bool CanHandle(IVariable variable);
        bool CanHandle(string typeName);
        bool CanHandle(VariableSaveData variable);
        string EncodeToString(IVariable variable);

        /// <summary>
        /// Decodes the specified data and applies the result to the given variable.
        /// </summary>
        void Decode(IVariable variable, string data);

        /// <summary>
        /// Decodes the specified VariableSaveData and applies the result to the given IVariable.`
        /// </summary>
        void Decode(IVariable variable, VariableSaveData data);

        /// <summary>
        /// Decodes the specified data and returns the result as an object of type T. Will
        /// throw an exception if the type is not supported.
        /// </summary>
        T DecodeTo<T>(string data);

        VariableSaveData EncodeToSave(IVariable varable);
    }


}