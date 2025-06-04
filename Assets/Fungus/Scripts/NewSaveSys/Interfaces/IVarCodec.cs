using AmanitaVar = Amanita.Variable;

namespace Amanita.SaveSys
{
    public interface IVarCodec
    {
        bool CanHandle(AmanitaVar variable);
        bool CanHandle(string typeName);
        bool CanHandle(VariableSaveData variable);
        string EncodeToString(AmanitaVar variable);
        void Decode(AmanitaVar variable, string data);
        void Decode(AmanitaVar variable, VariableSaveData data);
        VariableSaveData EncodeToSave(AmanitaVar varable);
    }


}