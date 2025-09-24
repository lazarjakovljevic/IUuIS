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

        public string LastActionDescription => undoStack.Count > 0 ? undoStack.Peek().Description : "No actions to undo";

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
                command.Execute();

                undoStack.Push(command);

                LimitStackSize();

                UndoStackChanged?.Invoke();
                CommandExecuted?.Invoke(command);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command '{command.Description}': {ex.Message}");
                throw;
            }
        }

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
                command.Undo();

                UndoStackChanged?.Invoke();
                CommandUndone?.Invoke(command);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error undoing command '{command.Description}': {ex.Message}");

                undoStack.Push(command);
                throw;
            }
        }

        public void Clear()
        {
            var count = undoStack.Count;
            undoStack.Clear();

            UndoStackChanged?.Invoke();
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