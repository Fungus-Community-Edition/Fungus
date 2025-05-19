using FungusVar = Fungus.Variable;

namespace Amanita.SaveSys
{
    public interface IVarEncoder
    {
        bool CanHandle(FungusVar variable);
        bool CanHandle(string typeName);
        bool CanHandle(VariableSaveData variable);
        string EncodeToString(FungusVar variable);
        void Decode(FungusVar variable, string data);
        void Decode(FungusVar variable, VariableSaveData data);
        VariableSaveData Encode(FungusVar varable);
    }


}