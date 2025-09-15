using System;
using System.Collections.ObjectModel;
using NetworkService.Model;

namespace NetworkService.Commands
{
    public class AddEntityCommand : IUndoableCommand
    {
        private readonly ObservableCollection<PowerConsumptionEntity> entities;
        private readonly PowerConsumptionEntity entity;

        public string Description { get; }
        public DateTime Timestamp { get; }

        public AddEntityCommand(ObservableCollection<PowerConsumptionEntity> entities, PowerConsumptionEntity entity)
        {
            this.entities = entities;
            this.entity = entity;
            Description = $"Add entity '{entity.Name}' (ID: {entity.Id})";
            Timestamp = DateTime.Now;
        }

        public void Execute()
        {
            entities.Add(entity);
        }

        public void Undo()
        {
            entities.Remove(entity);
        }
    }
}