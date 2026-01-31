using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class AnimatorSetup : EditorWindow
{
    private AnimatorController controller;
    private AnimationClip idleClip;
    private AnimationClip walkClip;
    private AnimationClip interactClip;

    [MenuItem("Tools/Setup Player Animator")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorSetup>("Animator Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Player Animator Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        controller = (AnimatorController)EditorGUILayout.ObjectField(
            "Animator Controller", 
            controller, 
            typeof(AnimatorController), 
            false
        );

        idleClip = (AnimationClip)EditorGUILayout.ObjectField(
            "Idle Animation", 
            idleClip, 
            typeof(AnimationClip), 
            false
        );

        walkClip = (AnimationClip)EditorGUILayout.ObjectField(
            "Walk Animation", 
            walkClip, 
            typeof(AnimationClip), 
            false
        );

        interactClip = (AnimationClip)EditorGUILayout.ObjectField(
            "Interact Animation", 
            interactClip, 
            typeof(AnimationClip), 
            false
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Setup Animator Controller"))
        {
            SetupAnimator();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This will create:\n" +
            "- Speed parameter (float)\n" +
            "- Interact trigger\n" +
            "- Idle state (default)\n" +
            "- Walk state\n" +
            "- Interact state (optional)\n" +
            "- Transitions between states",
            MessageType.Info
        );
    }

    private void SetupAnimator()
    {
        if (controller == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign an Animator Controller", "OK");
            return;
        }

        if (idleClip == null || walkClip == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign both Idle and Walk animation clips", "OK");
            return;
        }

        // Clear existing parameters
        while (controller.parameters.Length > 0)
        {
            controller.RemoveParameter(0);
        }

        // Add parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Interact", AnimatorControllerParameterType.Trigger);

        // Get the base layer state machine
        var rootStateMachine = controller.layers[0].stateMachine;

        // Clear existing states
        foreach (var state in rootStateMachine.states)
        {
            rootStateMachine.RemoveState(state.state);
        }

        // Create Idle state
        var idleState = rootStateMachine.AddState("Idle", new Vector3(300, 100, 0));
        idleState.motion = idleClip;

        // Create Walk state
        var walkState = rootStateMachine.AddState("Walk", new Vector3(300, 200, 0));
        walkState.motion = walkClip;

        // Set Idle as default state
        rootStateMachine.defaultState = idleState;

        // Create transitions between Idle and Walk
        // Idle to Walk (when Speed > 0.1)
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.hasExitTime = false;
        idleToWalk.exitTime = 0.0f;
        idleToWalk.duration = 0.1f;
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        // Walk to Idle (when Speed < 0.1)
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.hasExitTime = false;
        walkToIdle.exitTime = 0.0f;
        walkToIdle.duration = 0.1f;
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        // Create Interact state if clip is provided
        if (interactClip != null)
        {
            var interactState = rootStateMachine.AddState("Interact", new Vector3(500, 150, 0));
            interactState.motion = interactClip;

            // Idle to Interact (when Interact trigger)
            var idleToInteract = idleState.AddTransition(interactState);
            idleToInteract.hasExitTime = false;
            idleToInteract.duration = 0.1f;
            idleToInteract.AddCondition(AnimatorConditionMode.If, 0, "Interact");

            // Walk to Interact (when Interact trigger)
            var walkToInteract = walkState.AddTransition(interactState);
            walkToInteract.hasExitTime = false;
            walkToInteract.duration = 0.1f;
            walkToInteract.AddCondition(AnimatorConditionMode.If, 0, "Interact");

            // Interact to Idle (when animation finishes)
            var interactToIdle = interactState.AddTransition(idleState);
            interactToIdle.hasExitTime = true;
            interactToIdle.exitTime = 0.9f;
            interactToIdle.duration = 0.1f;
        }

        // Save the changes
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        string message = "Animator Controller has been set up successfully!\n\n" +
                        "States created:\n" +
                        "- Idle (default)\n" +
                        "- Walk\n";
        
        if (interactClip != null)
        {
            message += "- Interact\n";
        }

        message += "\nParameters:\n" +
                   "- Speed (float): Speed > 0.1 → Walk, Speed < 0.1 → Idle\n" +
                   "- Interact (trigger): Triggers interact animation";

        EditorUtility.DisplayDialog("Success", message, "OK");
    }
}
