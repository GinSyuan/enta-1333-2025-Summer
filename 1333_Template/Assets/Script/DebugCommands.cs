/// <summary>
/// Developer debug commands for testing.
/// Key Usage: Attach to a scene object; allows keyboard shortcuts to trigger debug actions.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using IngameDebugConsole;
using UnityEngine;


public class DebugCommands : MonoBehaviour
{
/// <summary>
    /// OnEnable - Perform this action
    /// </summary>
    private void OnEnable()
    {
        DebugLogConsole.AddCommand("HelloWorld", "Hello", HelloWorld);

    }

/// <summary>
    /// OnDisable - Perform this action
    /// </summary>
    private void OnDisable()
    {
        DebugLogConsole.RemoveCommand("HelloWorld");
    }

/// <summary>
    /// HelloWorld - Perform this action
    /// </summary>
    private void HelloWorld() 
    {
        Debug.Log("Hehehe");
    }
}
