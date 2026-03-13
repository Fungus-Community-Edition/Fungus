using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace AtMycelia.Amanita.VScripting.EditorUtils.FcWindow
{
    internal sealed class FcwLifecycleCoordinator
    {
        private readonly FcwFlowchartStateService _flowchartStateService;
        private readonly FcwPlayModeFocusService _playModeFocusService;

        public FcwLifecycleCoordinator(
            FcwFlowchartStateService flowchartStateService,
            FcwPlayModeFocusService playModeFocusService)
        {
            _flowchartStateService = flowchartStateService;
            _playModeFocusService = playModeFocusService;
        }

        public void HandleSceneLoaded(
            Scene scene,
            LoadSceneMode mode,
            Func<Flowchart> activeFlowchartGetter,
            FcwGraphicsRenderer graphicsRenderer)
        {
            ResetActiveFlowchartSelections(activeFlowchartGetter);
            graphicsRenderer?.RefreshNow();
        }

        public void HandleSceneClosed(
            Scene scene,
            Func<Flowchart> activeFlowchartGetter,
            FcwGraphicsRenderer graphicsRenderer)
        {
            ResetActiveFlowchartSelections(activeFlowchartGetter);
            graphicsRenderer?.RefreshNow();
        }

        public void HandleSceneOpened(
            Scene scene,
            Func<Flowchart> activeFlowchartGetter,
            FlowchartContext context,
            VisualElement rootVisualElement,
            MissingFlowchartOverlay missingOverlay,
            FcwGraphicsRenderer graphicsRenderer)
        {
            EnsureFlowchartForScene(
                context,
                rootVisualElement,
                missingOverlay,
                graphicsRenderer);

            ResetActiveFlowchartSelections(activeFlowchartGetter);
            graphicsRenderer?.RefreshNow();
        }

        private void ResetActiveFlowchartSelections(Func<Flowchart> activeFlowchartGetter)
        {
            if (activeFlowchartGetter == null)
            {
                return;
            }

            _flowchartStateService.ResetSelections(activeFlowchartGetter());
        }

        private void EnsureFlowchartForScene(
            FlowchartContext context,
            VisualElement rootVisualElement,
            MissingFlowchartOverlay missingOverlay,
            FcwGraphicsRenderer graphicsRenderer)
        {
            if (context == null)
            {
                return; // UI not built yet; CreateGUI will initialize.
            }

            if (context.Flowchart != null)
            {
                return; // Still valid.
            }

            Debug.Log("Seeking new flowchart for scene...");

            Flowchart lastFocusedInPlayMode;
            _playModeFocusService.TryResolveLastFocused(_flowchartStateService, out lastFocusedInPlayMode);

            bool usedPlayModeFlowchart;
            Flowchart resolved = _flowchartStateService.ResolveFlowchartForScene(context.Flowchart, lastFocusedInPlayMode, out usedPlayModeFlowchart);
            if (resolved == null)
            {
                if (missingOverlay != null)
                {
                    missingOverlay.Show(rootVisualElement);
                }
                return;
            }

            if (usedPlayModeFlowchart)
            {
                Debug.Log("Found last-focused flowchart from play mode.");
            }

            missingOverlay?.Hide();
            Flowchart previous = context.Flowchart;
            context.Flowchart = resolved;
            graphicsRenderer?.RefreshNow();
            if (!ReferenceEquals(previous, resolved))
            {
                FlowchartWindowSignals.ChangedFlowchart(previous, resolved);
            }
        }
    }
}