using AtMycelia.Hyphlow; // or your Flowchart namespace
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityObj = UnityEngine.Object;
using Type = System.Type;
using System.Reflection;
using AtMycelia.SaveSys;

/// <summary>
/// Generic base for testing Flowchart commands with different tween adapters.
/// </summary>
/// <typeparam name="TCommand">The command type to test (e.g., FadeSprite)</typeparam>
public abstract class CommandTestBase<TCommand> where TCommand : Command
{
    protected const float _duration = 0.5f;
    protected const float _epsilon = 0.01f;

    protected GameObject _go;
    protected Flowchart _flowchart;
    protected Block _block;
    protected TCommand _command;

    [SetUp]
    public virtual void SetUp()
    {
        _go = new GameObject(typeof(TCommand).Name + "_TestGO");
        _flowchart = _go.AddComponent<Flowchart>();
        _block = _flowchart.CreateBlock(Vector2.zero);
        _block.BlockName = "TestBlock";

        _command = _block.gameObject.AddComponent<TCommand>();
        _block.CommandList.Add(_command);

        _cmdType = _command.GetType();
        ConfigureCommand(_command);
        
    }

    protected Type _cmdType;
    protected readonly BindingFlags _flags = BindingFlags.NonPublic | BindingFlags.Instance;

    [TearDown]
    public virtual void TearDown()
    {
        UnityObj.DestroyImmediate(_go);
        _go = null;
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
        _flowchart.ExecuteBlock(_block);
        yield return new WaitForSeconds(_duration + 0.05f);
    }
}