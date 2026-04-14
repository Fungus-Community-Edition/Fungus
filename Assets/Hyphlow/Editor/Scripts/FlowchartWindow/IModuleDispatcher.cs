namespace AtMycelia.Hyphlow.EditorUtils
{
    internal interface IModuleDispatcher
    {
        void AddModule(object module);
        void RemoveModule(object module);
        void ClearModules();
        void ToggleSubs(bool on);
    }

    internal interface IModuleDispatcher<T> : IModuleDispatcher
    {
        void AddModule(T module);
        void RemoveModule(T module);

    }
}