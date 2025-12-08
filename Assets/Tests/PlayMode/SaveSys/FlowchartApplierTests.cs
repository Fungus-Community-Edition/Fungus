using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
using System;
using Amanita.VScripting;
using Amanita;

namespace SaveSystemTests
{
    public class FlowchartApplierTests : CommonTestFunctionality
    {
        [Test]
        public virtual async Task AppliesVarStates()
        {
            Debug.Log("At start of AppliesVarStates");
            await CommonSetupAsync();
            Debug.Log("Done awaiting common setup async");

            string expectedNameVarValue = nameVar.Value;
            int expectedScoreVarValue = scoreVar.Value;
            bool expectedNewPlayerVarValue = isNewPlayerVar.Value;
            float expectedFastestTimeVarValue = fastestTimeVar.Value;
            Vector3 expectedThreeDPosVarValue = threeDPosVar.Value;
            Vector2 expectedTwoDPosVarValue = twoDPosVar.Value;
            string expectedStringVarValue = stringVar.Value;
            Transform expectedTransformVarValue = transformVar.Value;

            // Change the states of the vars so that when loading the save data, we
            // can see if they were applied correctly.
            nameVar.Value = "Not Hello, World!";
            scoreVar.Value = 0;
            isNewPlayerVar.Value = false;
            fastestTimeVar.Value = -10.0f;
            threeDPosVar.Value = new Vector3(0.0f, 0.0f, 0.0f);
            twoDPosVar.Value = new Vector2(0.0f, 0.0f);
            stringVar.Value = "Not Hello, World!";
            transformVar.Value = null;

            Task applyTask = flowchartApplier.ApplyRange(new FlowchartSaveData[] { flowchartSaveData });
            await applyTask;

            bool appliedCorrectName = nameVar.Value == expectedNameVarValue;
            bool appliedCorrectScore = scoreVar.Value == expectedScoreVarValue;
            bool appliedCorrectNewPlayer = isNewPlayerVar.Value == expectedNewPlayerVarValue;
            bool appliedCorrectFastestTime = fastestTimeVar.Value == expectedFastestTimeVarValue;
            bool appliedCorrectThreeDPos = threeDPosVar.Value == expectedThreeDPosVarValue;
            bool appliedCorrectTwoDPos = twoDPosVar.Value == expectedTwoDPosVarValue;
            bool appliedCorrectString = stringVar.Value == expectedStringVarValue;
            bool appliedCorrectTransform = transformVar.Value == expectedTransformVarValue;
            bool success = appliedCorrectName && appliedCorrectScore && appliedCorrectNewPlayer &&
                appliedCorrectFastestTime && appliedCorrectThreeDPos && appliedCorrectTwoDPos &&
                appliedCorrectString && appliedCorrectTransform;
            Assert.IsTrue(success, "FlowchartApplier did not apply the variable states correctly.");
        }

        [UnityTest]
        public virtual IEnumerator ReexecutesBlocks()
        {
            yield return CommonSetup();
            yield return new WaitForSeconds(0.1f);
            flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);
            Task applyTask = flowchartApplier.ApplyRange(new FlowchartSaveData[] { flowchartSaveData });
            yield return new WaitUntil(() => applyTask.IsCompleted);
            yield return new WaitForSeconds(0.1f);
            // The block should be executed at this time

            Block testBlock = flowchart.FindBlock("TestBlock");
            bool blockExecuted = testBlock.IsExecuting();
            Assert.IsTrue(blockExecuted, "FlowchartApplier did not apply the block states correctly.");

        }

        [Test]
        public async Task Apply_WarnsAndSkips_WhenFlowchartIsMissing()
        {
            await CommonSetupAsync();

            // Cache these before destroying anything
            string expectedId = flowchartSaveData.UniqueId;
            string expectedName = flowchartSaveData.FlowchartName;

            RemoveAllFlowchartsFromTheScene();

            // Try to apply save data for a flowchart that no longer exists
            // Should not throw, should log a warning
            LogAssert.Expect(LogType.Warning, $"Flowchart with ID {flowchartSaveData.UniqueId} or name {flowchartSaveData.FlowchartName} not found.");
            await flowchartApplier.Apply(flowchartSaveData);
        }

        protected virtual void RemoveAllFlowchartsFromTheScene()
        {
            IList<Flowchart> toRemove = UnityObject.FindObjectsByType<Flowchart>(FindObjectsSortMode.None);
            foreach (var fc in toRemove)
            {
                UnityObject.DestroyImmediate(fc.gameObject);
            }
        }

        [Test]
        public async Task Apply_WarnsAndSkips_WhenVariableIsMissing()
        {
            await CommonSetupAsync();

            // Remove a variable from the flowchart
            var removedVar = flowchart.Variables.FirstOrDefault();
            Assume.That(removedVar != null, "Test scene must have at least one variable.");
            flowchart.RemoveVariable(removedVar);

            // SaveData still refers to the removed variable
            // Should not throw, should log a warning for the missing variable
            LogAssert.Expect(LogType.Warning, $"Variable {removedVar.Key} not found in flowchart {flowchart.name}.");
            await flowchartApplier.Apply(flowchartSaveData);
        }

        [Test]
        public async Task Apply_WarnsAndSkips_WhenBlockIsMissing()
        {
            await CommonSetupAsync();

            // Remove a block from the flowchart
            var removedBlock = flowchart.GetComponents<Block>().FirstOrDefault();
            Assume.That(removedBlock != null, "Test scene must have at least one block.");
            UnityObject.DestroyImmediate(removedBlock);

            // SaveData still refers to the removed block
            // Should not throw, should log a warning for the missing block
            LogAssert.Expect(LogType.Warning, $"Block {removedBlock.BlockName} not found in flowchart {flowchart.name}.");
            await flowchartApplier.Apply(flowchartSaveData);
        }

        [Test]
        public async Task Apply_WarnsAndSkips_WhenCommandIsMissing()
        {
            await CommonSetupAsync();

            // Find a block and its active command
            var block = flowchart.GetComponents<Block>().FirstOrDefault(b => b.CommandList.Count > 0);
            Assume.That(block != null, "Test scene must have at least one block with commands.");
            var command = block.CommandList.FirstOrDefault();
            Assume.That(command != null, "Block must have at least one command.");

            // Remove all Commands from the Block. Can't just be one, since the loading logic checks
            // for index when the one with the right ID isn't found. Fallbacks and all.
            block.CommandList.Clear();

            // Find the corresponding BlockSaveData in the save data
            var blockSave = flowchartSaveData.SavedBlocks
                .FirstOrDefault(bsd => bsd.ItemId == block.ItemId);
            Assume.That(blockSave != null, "Save data must contain the block.");

            // Set the save data to reference the now-missing command
            blockSave.ActiveCommandId = command.ItemId;
            blockSave.ActiveCommandIndex = 0;

            // Should log a warning and not throw
            LogAssert.Expect(LogType.Warning, $"Command {command.ItemId} not found in block {block.BlockName}.");
            await flowchartApplier.Apply(flowchartSaveData);
        }

        [Test]
        public async Task Apply_Works_WhenCalledFromBackgroundThread()
        {
            await CommonSetupAsync();

            // Change a variable so we can verify it gets applied
            nameVar.Value = "Changed from background thread";

            // Run Apply on a background thread
            await Task.Run(async () =>
            {
                await flowchartApplier.Apply(flowchartSaveData);
            });

            // The variable should be restored to its saved value
            Assert.AreEqual(
                flowchartSaveData.SavedVars.FirstOrDefault(v => v.VarName == nameVar.Key)?.Value,
                nameVar.Value,
                "Variable was not restored when Apply was called from a background thread.");
        }

        [Test]
        public async Task ApplyMulti_AppliesToMultipleFlowcharts()
        {
            await CommonSetupAsync();

            var origFirstVarVals = GetValsOfFirstVars();
            IList<object> GetValsOfFirstVars()
            {
                IList<object> result = new List<object>();
                foreach (var elem in flowchart.Variables)
                {
                    result.Add(elem.BoxedValue);
                }
                return result;
            }

            // Create a second flowchart in the scene
            var secondFlowchartGO = new GameObject("SecondFlowchart");
            var secondFlowchart = secondFlowchartGO.AddComponent<Flowchart>();
            RegisterTestFlowchart(secondFlowchart);

            // Add a variable to the second flowchart
            string initSecondVarVal = "initial";
            var secondVar = secondFlowchart.AddNewMuscariable<string, StringMuscariable>("secondVar", initSecondVarVal);

            FlowchartSaveData secondSaveData = flowchartSaveCodec.EncodeToSave(secondFlowchart);

            // Change the variable so we can verify it gets restored
            secondVar.Value = "changed";

            // Apply both save datas
            await flowchartApplier.ApplyRange(new[] { flowchartSaveData, secondSaveData });

            // Assert both flowcharts' variables were restored
            var firstSavedVars = flowchartSaveData.SavedVars;

            var varsAfterRestoration = GetValsOfFirstVars();
            bool firstFcVarsRestored = varsAfterRestoration.SequenceEqual(origFirstVarVals);
            Assert.IsTrue(firstFcVarsRestored, "First flowchart variables were not restored.");
            Assert.AreEqual(initSecondVarVal, secondVar.Value, "Second flowchart variable was not restored.");

            // Cleanup
            UnityObject.DestroyImmediate(secondFlowchartGO);
        }

        [Test]
        public async Task Apply_IgnoresMissingVarsAndBlocks_WhenPartialSaveData()
        {
            await CommonSetupAsync();

            // Change all variables and blocks to known "wrong" values
            nameVar.Value = "WrongName";
            scoreVar.Value = -999;

            // Remove all but one variable and one block from the save data
            var originalVars = flowchartSaveData.SavedVars.ToList();
            var originalBlocks = flowchartSaveData.SavedBlocks.ToList();

            var keptVar = flowchartSaveData.SavedVars.First();
            var keptBlock = flowchartSaveData.SavedBlocks.First();

            flowchartSaveData.SavedVars = new List<VariableSaveData> { keptVar };
            flowchartSaveData.SavedBlocks = new List<BlockSaveData> { keptBlock };

            // Change the kept variable to a unique value in the save data
            keptVar.Value = "RestoredName";

            // Apply the partial save data
            await flowchartApplier.Apply(flowchartSaveData);

            // Only the kept variable should be restored
            Assert.AreEqual("RestoredName", nameVar.Value, "Kept variable was not restored.");
            Assert.AreEqual(-999, scoreVar.Value, "Non-kept variable should not be changed.");

            // Restore original save data for other tests
            flowchartSaveData.SavedVars = originalVars;
            flowchartSaveData.SavedBlocks = originalBlocks;
        }

        [Test]
        public async Task Apply_FindsFlowchartByName_WhenIdDoesNotMatch()
        {
            await CommonSetupAsync();

            // Change the flowchart's UniqueId so it no longer matches the save data
            string originalId = flowchart.UniqueId;
            typeof(Flowchart)
                .GetField("uniqueId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(flowchart, originalId + "erho8ufgiyswegfi7wfgf7o");

            // The save data still has the old ID, but the name matches
            // Change a variable so we can verify it gets restored
            nameVar.Value = "ChangedForNameFallback";

            await flowchartApplier.Apply(flowchartSaveData);

            // The variable should be restored, meaning the fallback by name worked
            Assert.AreEqual(
                flowchartSaveData.SavedVars.FirstOrDefault(v => v.VarName == nameVar.Key)?.Value,
                nameVar.Value,
                "Flowchart was not found by name fallback.");

            // Restore the original ID for other tests
            typeof(Flowchart)
                .GetField("uniqueId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(flowchart, originalId);
        }

    }
}