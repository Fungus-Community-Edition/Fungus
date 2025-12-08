using Amanita.VScripting; // or your Flowchart namespace
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using Amanita;
using UnityObj = UnityEngine.Object;
using Amanita.SaveSys;
using Type = System.Type;
using System.Reflection;

/// <summary>
/// Generic base for testing Flowchart commands with different tween adapters.
/// </summary>
/// <typeparam name="TCommand">The command type to test (e.g., FadeSprite)</typeparam>
public abstract class CommandTestBase<TCommand> where TCommand : Command
{
    protected const float Duration = 0.5f;
    protected const float Epsilon = 0.01f;

    protected GameObject go;
    protected Flowchart flowchart;
    protected Block block;
    protected TCommand command;

    [SetUp]
    public virtual void SetUp()
    {
        if (AmanitaManager.S != null)
        {
            UnityObj.DestroyImmediate(AmanitaManager.S.gameObject);
        }

        string pathToManager = "Prefabs/AmanitaManager";
        AmanitaManager managerPrefab = Resources.Load<AmanitaManager>(pathToManager);

        if (managerPrefab == null)
        {
            throw new System.MissingFieldException("Wrong path to the Amanita Manager");
        }

        manager = UnityObj.Instantiate(managerPrefab);

        go = new GameObject(typeof(TCommand).Name + "_TestGO");
        flowchart = go.AddComponent<Flowchart>();
        block = flowchart.CreateBlock(Vector2.zero);
        block.BlockName = "TestBlock";

        command = block.gameObject.AddComponent<TCommand>();
        block.CommandList.Add(command);

        cmdType = command.GetType();
        ConfigureCommand(command);
        
    }

    protected AmanitaManager manager;
    protected Type cmdType;
    protected readonly BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

    [TearDown]
    public virtual void TearDown()
    {
        UnityObj.DestroyImmediate(go);
        UnityObj.DestroyImmediate(manager.gameObject);
        go = null;
        manager = null;
        SaveSystem.ResetStaticsForTest();
        Flowchart.ResetStaticsForTest();
    }

    /// <summary>
    /// Override to set up command‑specific fields (target component, duration, etc.).
    /// </summary>
    protected abstract void ConfigureCommand(TCommand cmd);

    /// <summary>
    /// Override to assert the final state after the tween completes.
    /// </summary>
    protected abstract void AssertFinalState();

    protected IEnumerator RunBlockAndWait()
    {
        flowchart.ExecuteBlock(block);
        yield return new WaitForSeconds(Duration + 0.05f);
    }
}