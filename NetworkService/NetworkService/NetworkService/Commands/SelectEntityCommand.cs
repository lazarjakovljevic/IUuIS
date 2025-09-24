using System;
using NetworkService.Model;
using NetworkService.ViewModel;

namespace NetworkService.Commands
{
    public class SelectEntityCommand : IUndoableCommand
    {
        private readonly PowerConsumptionEntity previousEntity;
        private readonly PowerConsumptionEntity newEntity;

        public string Description { get; }
        public DateTime Timestamp { get; }

        public SelectEntityCommand(PowerConsumptionEntity previousEntity, PowerConsumptionEntity newEntity)
        {
            this.previousEntity = previousEntity;
            this.newEntity = newEntity;

            var prevName = previousEntity?.Name ?? "None";
            var newName = newEntity?.Name ?? "None";
            Description = $"Select entity '{newName}' (was '{prevName}')";
            Timestamp = DateTime.Now;
        }

        public void Execute()
        {
            MeasurementGraphViewModel.Instance.SelectedEntity = newEntity;
        }

        public void Undo()
        {
            MeasurementGraphViewModel.Instance.SetSelectedEntitySilently(previousEntity);
        }
    }
}