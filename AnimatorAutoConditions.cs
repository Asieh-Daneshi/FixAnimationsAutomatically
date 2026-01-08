using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
// to automatically index transitions coming out of idle posture to "Raise" then next "Lower" and then next, going back to idle posture to "Idle"
public class AnimatorAutoConditions
{
    private const string SOURCE_STATE_NAME = "Female-HandsUp_018_FBX";

    [MenuItem("Tools/Animator/Auto-Assign Raise Lower Idle Conditions")]
    static void AutoAssignConditions()
    {
        AnimatorController controller =
            Selection.activeObject as AnimatorController;

        if (controller == null)
        {
            Debug.LogError("Select an Animator Controller.");
            return;
        }

        foreach (var layer in controller.layers)
        {
            AnimatorStateMachine sm = layer.stateMachine;

            AnimatorState sourceState = FindState(sm, SOURCE_STATE_NAME);
            if (sourceState == null)
                continue;

            var raiseTransitions = sourceState.transitions;

            for (int i = 0; i < raiseTransitions.Length; i++)
            {
                int indexValue = i + 1;

                // -------- RAISE --------
                AnimatorStateTransition raiseTransition = raiseTransitions[i];
                AnimatorState raiseState = raiseTransition.destinationState;
                if (raiseState == null)
                    continue;

                ClearConditions(raiseTransition);
                raiseTransition.AddCondition(
                    AnimatorConditionMode.Equals,
                    indexValue,
                    "Raise"
                );

                // -------- LOWER --------
                foreach (var lowerTransition in raiseState.transitions)
                {
                    AnimatorState lowerState = lowerTransition.destinationState;
                    if (lowerState == null)
                        continue;

                    ClearConditions(lowerTransition);
                    lowerTransition.AddCondition(
                        AnimatorConditionMode.Equals,
                        indexValue,
                        "Lower"
                    );

                    // -------- IDLE (from LOWER back to HandsUp) --------
                    foreach (var idleTransition in lowerState.transitions)
                    {
                        if (idleTransition.destinationState != sourceState)
                            continue;

                        ClearConditions(idleTransition);
                        idleTransition.AddCondition(
                            AnimatorConditionMode.Equals,
                            indexValue,
                            "Idle"
                        );
                    }
                }

                Debug.Log(
                    $"✔ Raise={indexValue}, Lower={indexValue}, Idle={indexValue}"
                );
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Animator conditions correctly reassigned.");
    }

    static AnimatorState FindState(AnimatorStateMachine sm, string name)
    {
        foreach (var state in sm.states)
        {
            if (state.state.name == name)
                return state.state;
        }

        foreach (var sub in sm.stateMachines)
        {
            var found = FindState(sub.stateMachine, name);
            if (found != null)
                return found;
        }

        return null;
    }

    static void ClearConditions(AnimatorStateTransition transition)
    {
        while (transition.conditions.Length > 0)
        {
            transition.RemoveCondition(transition.conditions[0]);
        }
    }
}
