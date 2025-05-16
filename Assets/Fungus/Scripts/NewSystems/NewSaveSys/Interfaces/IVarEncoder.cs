using FungusVar = Fungus.Variable;

namespace Amanita.SaveSys
{
    public interface IVarEncoder
    {
        bool CanHandle(FungusVar variable);
        bool CanHandle(string typeName);
        string Encode(FungusVar variable);
        void Decode(FungusVar variable, string data);
    }
}