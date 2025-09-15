using System;
using System.Collections.ObjectModel;
using NetworkService.Model;

namespace NetworkService.Commands
{
    public class DeleteEntityCommand : IUndoableCommand
    {
        private readonly ObservableCollection<PowerConsumptionEntity> entities;
        private readonly PowerConsumptionEntity entity;
        private readonly int originalIndex;

        public string Description { get; }
        public DateTime Timestamp { get; }

        public DeleteEntityCommand(ObservableCollection<PowerConsumptionEntity> entities, PowerConsumptionEntity entity)
        {
            this.entities = entities;
            this.entity = entity;
            this.originalIndex = entities.IndexOf(entity);
            Description = $"Delete entity '{entity.Name}' (ID: {entity.Id})";
            Timestamp = DateTime.Now;
        }

        public void Execute()
        {
            entities.Remove(entity);
        }

        public void Undo()
        {
            if (originalIndex >= 0 && originalIndex <= entities.Count)
            {
                entities.Insert(originalIndex, entity);
            }
            else
            {
                entities.Add(entity);
            }
        }
    }
}