// Assets/Scripts/Commands/CommandDispatcher.cs
using System.Collections.Generic;
using UnityEngine;

// Singleton to handle command execution
public class CommandDispatcher : MonoBehaviour
{
    private static CommandDispatcher instance;

    public static CommandDispatcher Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("CommandDispatcher");
                instance = obj.AddComponent<CommandDispatcher>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private Queue<ICommand> commandQueue = new Queue<ICommand>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
    }

    // Queue commands for later execution
    public void QueueCommand(ICommand command)
    {
        commandQueue.Enqueue(command);
    }

    // Execute all queued commands
    public void ExecuteQueuedCommands()
    {
        while (commandQueue.Count > 0)
        {
            ICommand command = commandQueue.Dequeue();
            ExecuteCommand(command);
        }
    }

    private void Update()
    {
        // Option to execute queued commands each frame
        // ExecuteQueuedCommands();
    }
}