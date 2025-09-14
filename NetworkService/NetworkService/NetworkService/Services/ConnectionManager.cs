using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using NetworkService.Model;

namespace NetworkService.Services
{
    public class ConnectionManager
    {
        #region Fields

        private readonly ObservableCollection<Connection> connections;
        private readonly Canvas lineCanvas; 
        private readonly Dictionary<Canvas, PowerConsumptionEntity> canvasEntityMap;

        #endregion

        #region Properties

        public ObservableCollection<Connection> Connections => connections;

        public Canvas LineCanvas => lineCanvas;

        #endregion

        #region Events

        public event Action<Connection> ConnectionAdded;

        public event Action<Connection> ConnectionRemoved;

        #endregion

        #region Constructor

        public ConnectionManager(Canvas lineCanvas, Dictionary<Canvas, PowerConsumptionEntity> canvasEntityMap)
        {
            this.connections = new ObservableCollection<Connection>();
            this.lineCanvas = lineCanvas ?? throw new ArgumentNullException(nameof(lineCanvas));
            this.canvasEntityMap = canvasEntityMap ?? throw new ArgumentNullException(nameof(canvasEntityMap));
        }

        #endregion

        #region Public Methods

        public void CreateAutomaticConnections()
        {
            var entitiesOnGrid = canvasEntityMap.Where(x => x.Value != null).ToList();

            if (entitiesOnGrid.Count < 2)
                return;

            for (int i = 0; i < entitiesOnGrid.Count; i++)
            {
                for (int j = i + 1; j < entitiesOnGrid.Count; j++)
                {
                    var from = entitiesOnGrid[i];
                    var to = entitiesOnGrid[j];

                    CreateConnection(from.Value, from.Key, to.Value, to.Key);
                }
            }
        }
        public bool CreateConnection(PowerConsumptionEntity fromEntity, Canvas fromCanvas,
                                   PowerConsumptionEntity toEntity, Canvas toCanvas)
        {
            if (fromEntity == null || toEntity == null || fromCanvas == null || toCanvas == null)
                return false;


            if (fromEntity.Id == toEntity.Id)
                return false;

            if (ConnectionExists(fromEntity, toEntity))
                return false;

            var connection = new Connection(fromEntity, toEntity, fromCanvas, toCanvas);

            lineCanvas.Children.Add(connection.VisualLine);

            connections.Add(connection);

            ConnectionAdded?.Invoke(connection);

            Console.WriteLine($"Created connection: {fromEntity.Name} -> {toEntity.Name}");
            return true;
        }
        public void RemoveConnectionsForEntity(PowerConsumptionEntity entity)
        {
            if (entity == null)
                return;

            var connectionsToRemove = connections
                .Where(c => c.FromEntity?.Id == entity.Id || c.ToEntity?.Id == entity.Id)
                .ToList();

            foreach (var connection in connectionsToRemove)
            {
                RemoveConnection(connection);
            }

            Console.WriteLine($"Removed {connectionsToRemove.Count} connections for entity {entity.Name}");
        }
        public bool RemoveConnection(Connection connection)
        {
            if (connection == null)
                return false;
  
            if (connection.VisualLine != null && lineCanvas.Children.Contains(connection.VisualLine))
            {
                lineCanvas.Children.Remove(connection.VisualLine);
            }

            bool removed = connections.Remove(connection);

            if (removed)
            {
                ConnectionRemoved?.Invoke(connection);
                Console.WriteLine($"Removed connection: {connection.FromEntity?.Name} -> {connection.ToEntity?.Name}");
            }

            return removed;
        }
        public void UpdateAllLinePositions()
        {
            foreach (var connection in connections.Where(c => c.IsValid))
            {
                connection.UpdateLinePosition();
            }
        }
        public void UpdateLinePositionsForEntity(PowerConsumptionEntity entity)
        {
            if (entity == null)
                return;

            var entityConnections = connections
                .Where(c => c.FromEntity?.Id == entity.Id || c.ToEntity?.Id == entity.Id)
                .Where(c => c.IsValid);

            foreach (var connection in entityConnections)
            {
                connection.UpdateLinePosition();
            }
        }

        public void UpdateAllLineColors()
        {
            foreach (var connection in connections.Where(c => c.IsValid))
            {
                connection.UpdateLineColor();
            }
        }

        public void UpdateEntityCanvas(PowerConsumptionEntity entity, Canvas newCanvas)
        {
            if (entity == null || newCanvas == null)
                return;

            var entityConnections = connections
                .Where(c => c.FromEntity?.Id == entity.Id || c.ToEntity?.Id == entity.Id);

            foreach (var connection in entityConnections)
            {
                if (connection.FromEntity?.Id == entity.Id)
                {
                    connection.FromCanvas = newCanvas;
                }

                if (connection.ToEntity?.Id == entity.Id)
                {
                    connection.ToCanvas = newCanvas;
                }

                connection.UpdateLinePosition();
            }
        }

        public void ClearAllConnections()
        {
            var linesToRemove = connections.Select(c => c.VisualLine).Where(l => l != null).ToList();
            foreach (var line in linesToRemove)
            {
                if (lineCanvas.Children.Contains(line))
                {
                    lineCanvas.Children.Remove(line);
                }
            }

            connections.Clear();

            Console.WriteLine("Cleared all connections");
        }

        public int GetConnectionCountForEntity(PowerConsumptionEntity entity)
        {
            if (entity == null)
                return 0;

            return connections.Count(c => c.FromEntity?.Id == entity.Id || c.ToEntity?.Id == entity.Id);
        }
        public List<PowerConsumptionEntity> GetConnectedEntities(PowerConsumptionEntity entity)
        {
            if (entity == null)
                return new List<PowerConsumptionEntity>();

            var connectedEntities = new List<PowerConsumptionEntity>();

            foreach (var connection in connections.Where(c => c.IsValid))
            {
                if (connection.FromEntity?.Id == entity.Id && connection.ToEntity != null)
                {
                    connectedEntities.Add(connection.ToEntity);
                }
                else if (connection.ToEntity?.Id == entity.Id && connection.FromEntity != null)
                {
                    connectedEntities.Add(connection.FromEntity);
                }
            }

            return connectedEntities.Distinct().ToList();
        }

        #endregion

        #region Private Methods

        private bool ConnectionExists(PowerConsumptionEntity entity1, PowerConsumptionEntity entity2)
        {
            return connections.Any(c =>
                (c.FromEntity?.Id == entity1.Id && c.ToEntity?.Id == entity2.Id) ||
                (c.FromEntity?.Id == entity2.Id && c.ToEntity?.Id == entity1.Id));
        }

        #endregion
    }
}