using Amanita;
using Amanita.SaveSys;
using Amanita.VScripting; // or your Flowchart namespace
using Collections;
using DoTweenita;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObj = UnityEngine.Object;

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
        command.ParentBlock = block;

        adapter = ScriptableObject.CreateInstance<AmaniDoTweenAdapter>();

        ConfigureCommand(command);

        toDestroyInTearDown.Add(manager.gameObject);
        toDestroyInTearDown.Add(go);
        toDestroyInTearDown.Add(adapter);

    }

    protected AmanitaManager manager;
    protected AmaniDoTweenAdapter adapter;
    protected readonly IList<UnityObj> toDestroyInTearDown = new List<UnityObj>();

    [TearDown]
    public virtual void TearDown()
    {
        foreach (var elem in toDestroyInTearDown)
        {
            if (elem != null) 
            {
                Object.DestroyImmediate(elem);
            }
        }
        go = null;
        manager = null;
        adapter = null;
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