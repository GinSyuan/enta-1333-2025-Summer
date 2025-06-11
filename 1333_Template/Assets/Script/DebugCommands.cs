using System.Collections;
using System.Collections.Generic;
using IngameDebugConsole;
using UnityEngine;


public class DebugCommands : MonoBehaviour
{
    private void OnEnable()
    {
        DebugLogConsole.AddCommand("HelloWorld", "Hello", HelloWorld);

    }

    private void OnDisable()
    {
        DebugLogConsole.RemoveCommand("HelloWorld");
    }

    private void HelloWorld() 
    {
        Debug.Log("Hehehe");
    }
}
