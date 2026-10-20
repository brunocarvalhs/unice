using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unice
{
    /// <summary>
    /// The map editor's problem list: runs <see cref="MapValidation.Validate"/> over <see cref="MapEditorWindow.Working"/> and shows
    /// what's wrong, clicking a problem selects the item it's about. Re-validates only when <see cref="MapEditorWindow.Revision"/>
    /// changed since the last draw (validating rebuilds the whole navigation graph — too slow to redo every repaint), and never
    /// while <see cref="EditorApplication.isPlaying"/> (the live match owns <see cref="Map"/>'s state then). Every validation run
    /// mutates that shared state (<see cref="MapValidation.Validate"/> calls <see cref="Map.Load"/>), so <see cref="ValidateAndRestore"/>
    /// always reloads the house right after, leaving <see cref="Map"/> at its documented rest state.
    /// </summary>
    sealed class MapEditorValidationPanel
    {
        const float MaxListHeight = 160, RowHeight = 20;

        List<MapProblem> cachedProblems = new List<MapProblem>();
        int cachedRevision = -1;
        Vector2 scrollPosition;
        GUIStyle problemButtonStyle;

        public void Draw(MapEditorWindow window)
        {
            EditorGUILayout.LabelField("Problemas", EditorStyles.boldLabel);

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Validação pausada durante o Play.", MessageType.Info);
                return;
            }

            var problems = ProblemsFor(window);
            if (problems.Count == 0) { EditorGUILayout.HelpBox("Nenhum problema.", MessageType.Info); return; }

            problemButtonStyle ??= new GUIStyle(EditorStyles.miniButton) { alignment = TextAnchor.MiddleLeft, wordWrap = true };
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(Mathf.Min(MaxListHeight, problems.Count * RowHeight + 4)));
            foreach (var problem in problems)
                if (GUILayout.Button(problem.Message, problemButtonStyle)) SelectTarget(window, problem);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>The current problems, re-validating only when <see cref="MapEditorWindow.Revision"/> moved on since the last call.</summary>
        List<MapProblem> ProblemsFor(MapEditorWindow window)
        {
            if (window.Revision == cachedRevision) return cachedProblems;
            cachedProblems = ValidateAndRestore(window.Working);
            cachedRevision = window.Revision;
            return cachedProblems;
        }

        /// <summary>
        /// Validates <paramref name="map"/> and immediately reloads the house, so the shared <see cref="Map"/> state <see cref="MapValidation.Validate"/>
        /// mutates is never left pointed at an editor's in-memory copy. Always fresh (no cache) — safe to call right before a save.
        /// </summary>
        public static List<MapProblem> ValidateAndRestore(MapDefinition map)
        {
            var problems = MapValidation.Validate(map);
            Map.Load(MapHouse.Build());
            return problems;
        }

        void SelectTarget(MapEditorWindow window, MapProblem problem)
        {
            switch (problem.Target)
            {
                case MapProblemTarget.Room: window.Selection = new MapEditorSelection(MapEditorSelectionKind.Room, problem.Index); break;
                case MapProblemTarget.Furniture: window.Selection = new MapEditorSelection(MapEditorSelectionKind.Furniture, problem.Index); break;
                case MapProblemTarget.Spawn: window.Selection = new MapEditorSelection(MapEditorSelectionKind.Spawn, -1); break;
                case MapProblemTarget.Entry: window.Selection = new MapEditorSelection(MapEditorSelectionKind.Entry, -1); break;
                // MapProblemTarget.Map isn't tied to one item: nothing to select.
            }
            window.Repaint();
        }
    }
}
