using NetworkService.Model;
using NetworkService.Views;
using System;
using System.Windows.Controls;

namespace NetworkService.Commands
{
    public class MoveEntityCommand : IUndoableCommand
    {
        private readonly Canvas fromCanvas;
        private readonly Canvas toCanvas;
        private readonly PowerConsumptionEntity entity;

        public string Description { get; }
        public DateTime Timestamp { get; }

        public MoveEntityCommand(PowerConsumptionEntity entity, Canvas fromCanvas, Canvas toCanvas)
        {
            this.entity = entity;
            this.fromCanvas = fromCanvas;
            this.toCanvas = toCanvas;

            var fromName = fromCanvas?.Name ?? "TreeView";
            var toName = toCanvas?.Name ?? "TreeView";
            Description = $"Move '{entity.Name}' from {fromName} to {toName}";
            Timestamp = DateTime.Now;
        }

        public void Execute()
        {
            // Move is already done in drag&drop, this is just for tracking
        }

        public void Undo()
        {
            var networkView = NetworkDisplayView.Instance;
            if (networkView == null)
            {
                Console.WriteLine("NetworkDisplayView instance not available for undo");
                return;
            }

            if (fromCanvas == null)
            {
                // Was moved from TreeView to Canvas - return to TreeView
                networkView.RemoveEntityFromCanvasUndo(toCanvas, true);
            }
            else if (toCanvas == null)
            {
                // Was moved from Canvas to TreeView - return to Canvas
                networkView.PlaceEntityOnCanvasUndo(fromCanvas, entity);
            }
            else
            {
                // Was moved from Canvas to Canvas - move back
                networkView.RemoveEntityFromCanvasUndo(toCanvas, false);
                networkView.PlaceEntityOnCanvasUndo(fromCanvas, entity);
            }
        }
    }
}