using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkService.Commands
{
    public class UndoManager
    {
        #region Singleton Pattern

        private static UndoManager instance;
        private static readonly object lockObject = new object();

        public static UndoManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                            instance = new UndoManager();
                    }
                }
                return instance;
            }
        }

        #endregion

        #region Fields

        private readonly Stack<IUndoableCommand> undoStack;
        private const int MAX_UNDO_STACK_SIZE = 50;

        #endregion

        #region Properties

        public bool CanUndo => undoStack.Count > 0;

        /// <summary>
        /// Gets description of the last action that can be undone
        /// </summary>
        public string LastActionDescription => undoStack.Count > 0 ? undoStack.Peek().Description : "No actions to undo";

        /// <summary>
        /// Gets the number of actions in undo stack
        /// </summary>
        public int UndoStackCount => undoStack.Count;

        #endregion

        #region Events

        public event Action UndoStackChanged;

        public event Action<IUndoableCommand> CommandExecuted;

        public event Action<IUndoableCommand> CommandUndone;

        #endregion

        #region Constructor

        private UndoManager()
        {
            undoStack = new Stack<IUndoableCommand>();
        }

        #endregion

        #region Public Methods
        public void ExecuteCommand(IUndoableCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            try
            {
                // Execute the command
                command.Execute();

                // Add to undo stack
                undoStack.Push(command);

                // Limit stack size to prevent memory issues
                LimitStackSize();

                // Notify subscribers
                UndoStackChanged?.Invoke();
                CommandExecuted?.Invoke(command);

                Console.WriteLine($"Executed: {command.Description} at {command.Timestamp:HH:mm:ss}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command '{command.Description}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Undoes the last executed command
        /// </summary>
        /// <returns>True if undo was successful, false if no commands to undo</returns>
        public bool Undo()
        {
            if (!CanUndo)
            {
                Console.WriteLine("No commands to undo");
                return false;
            }

            var command = undoStack.Pop();

            try
            {
                // Undo the command
                command.Undo();

                // Notify subscribers
                UndoStackChanged?.Invoke();
                CommandUndone?.Invoke(command);

                Console.WriteLine($"Undone: {command.Description} (executed at {command.Timestamp:HH:mm:ss})");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error undoing command '{command.Description}': {ex.Message}");

                // Put command back on stack since undo failed
                undoStack.Push(command);
                throw;
            }
        }

        /// <summary>
        /// Clears all commands from undo stack
        /// </summary>
        public void Clear()
        {
            var count = undoStack.Count;
            undoStack.Clear();

            UndoStackChanged?.Invoke();
            Console.WriteLine($"Cleared undo stack ({count} commands removed)");
        }

        /// <summary>
        /// Gets list of all commands in undo stack (for debugging/UI)
        /// </summary>
        /// <returns>List of command descriptions in chronological order</returns>
        public List<string> GetUndoHistory()
        {
            return undoStack.Select(cmd => $"{cmd.Timestamp:HH:mm:ss} - {cmd.Description}").ToList();
        }

        #endregion

        #region Private Methods
        private void LimitStackSize()
        {
            if (undoStack.Count > MAX_UNDO_STACK_SIZE)
            {
                var commands = undoStack.ToArray().Take(MAX_UNDO_STACK_SIZE).Reverse().ToArray();
                undoStack.Clear();

                foreach (var cmd in commands)
                {
                    undoStack.Push(cmd);
                }

                Console.WriteLine($"Undo stack trimmed to {MAX_UNDO_STACK_SIZE} items");
            }
        }

        #endregion

        #region Debug Methods
        public void PrintUndoStack()
        {
            Console.WriteLine($"=== Undo Stack ({undoStack.Count} items) ===");
            if (undoStack.Count == 0)
            {
                Console.WriteLine("Empty");
                return;
            }

            int index = 0;
            foreach (var command in undoStack)
            {
                Console.WriteLine($"{index}: {command.Timestamp:HH:mm:ss} - {command.Description}");
                index++;
            }
            Console.WriteLine("========================");
        }

        #endregion
    }
}