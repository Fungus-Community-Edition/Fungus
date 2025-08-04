using Amanita.VScripting.EditorUtils;

namespace Amanita.Tests
{
    class Animal { }
    class Mammal : Animal { }
    class Dog : Mammal { }
    class Reptile : Animal { }

    [RowVisualHandler("Testing",
        typeof(Animal),
        "Animal",
        "_EditorResources/UIToolkitTemplates/VarRows/VariableRowTemplate")]
    class AnimalHandler : RowVisualHandler
    {
        
    }

    [RowVisualHandler("Testing",
        typeof(Mammal),
        "Mammal",
        "_EditorResources/UIToolkitTemplates/VarRows/VariableRowTemplate")]
    class MammalHandler { }

    [RowVisualHandler("Testing",
        typeof(Dog),
        "Dog",
        "_EditorResources/UIToolkitTemplates/VarRows/VariableRowTemplate")]
    class DogHandler { }

    // We already have a default handler in the actual editor namespace, so no need to define it here


}