using System;

namespace NetworkService.Commands
{
    public interface IUndoableCommand
    {
        void Execute();
        void Undo();
        string Description { get; }
        DateTime Timestamp { get; }
    }
}