using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using NetworkService.MVVM;
using NetworkService.Model;
using NetworkService.Views;

namespace NetworkService.ViewModel
{
    public class NetworkDisplayViewModel : BindableBase
    {
        #region Singleton Pattern

        private static NetworkDisplayViewModel instance;
        public static NetworkDisplayViewModel Instance
        {
            get
            {
                if (instance == null)
                    instance = new NetworkDisplayViewModel();
                return instance;
            }
        }

        // Private constructor for singleton
        private NetworkDisplayViewModel()
        {
            LoadGroupedEntities();

            // Subscribe to changes in shared entities
            if (SharedEntities != null)
            {
                SharedEntities.CollectionChanged += OnSharedEntitiesChanged;
            }
        }

        #endregion

        #region Properties

        private ObservableCollection<EntityGroup> groupedEntities;
        public ObservableCollection<EntityGroup> GroupedEntities
        {
            get { return groupedEntities; }
            set { SetProperty(ref groupedEntities, value); }
        }

        // Reference to shared entities
        public ObservableCollection<PowerConsumptionEntity> SharedEntities
        {
            get { return MainWindowViewModel.SharedEntities; }
        }

        #endregion

        #region State Persistence

        private Dictionary<string, PowerConsumptionEntity> savedCanvasState;

        public Dictionary<string, PowerConsumptionEntity> SavedCanvasState
        {
            get { return savedCanvasState ?? (savedCanvasState = new Dictionary<string, PowerConsumptionEntity>()); }
        }

        /// <summary>
        /// Čuva trenutno stanje Canvas-a
        /// </summary>
        public void SaveCanvasState(Dictionary<Canvas, PowerConsumptionEntity> canvasEntityMap)
        {
            SavedCanvasState.Clear();
            foreach (var kvp in canvasEntityMap)
            {
                if (kvp.Value != null)
                {
                    SavedCanvasState[kvp.Key.Name] = kvp.Value;
                }
            }
            Console.WriteLine($"Saved state for {SavedCanvasState.Count} entities");
        }

        /// <summary>
        /// Vraća sačuvano stanje Canvas-a
        /// </summary>
        public void RestoreCanvasState(Dictionary<Canvas, PowerConsumptionEntity> allCanvases,
                                      Action<Canvas, PowerConsumptionEntity> placeEntityCallback)
        {
            foreach (var kvp in SavedCanvasState)
            {
                var canvas = allCanvases.Keys.FirstOrDefault(c => c.Name == kvp.Key);
                if (canvas != null && kvp.Value != null)
                {
                    // Check if entity still exists in shared collection
                    var currentEntity = SharedEntities.FirstOrDefault(e => e.Id == kvp.Value.Id);
                    if (currentEntity != null)
                    {
                        placeEntityCallback(canvas, currentEntity);
                    }
                }
            }
            Console.WriteLine($"Restored state for {SavedCanvasState.Count} entities");
        }

        #endregion

        #region Methods

        private void LoadGroupedEntities()
        {
            GroupedEntities = new ObservableCollection<EntityGroup>();

            if (SharedEntities != null)
            {
                // Group entities by type
                var smartMeters = SharedEntities.Where(e => e.Type.Name.Contains("Smart")).ToList();
                var intervalMeters = SharedEntities.Where(e => e.Type.Name.Contains("Interval")).ToList();

                if (smartMeters.Any())
                {
                    var smartGroup = new EntityGroup("Smart Meters");
                    foreach (var meter in smartMeters)
                        smartGroup.Entities.Add(meter);
                    GroupedEntities.Add(smartGroup);
                }

                if (intervalMeters.Any())
                {
                    var intervalGroup = new EntityGroup("Interval Meters");
                    foreach (var meter in intervalMeters)
                        intervalGroup.Entities.Add(meter);
                    GroupedEntities.Add(intervalGroup);
                }
            }
        }

        private void OnSharedEntitiesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Refresh groupings when entities change
            LoadGroupedEntities();
        }

        public void RefreshGroupings()
        {
            LoadGroupedEntities();
        }

        public void RemoveEntityFromTree(PowerConsumptionEntity entity)
        {
            // Find and remove entity from appropriate group
            foreach (var group in GroupedEntities)
            {
                if (group.Entities.Contains(entity))
                {
                    group.Entities.Remove(entity);
                    // Refresh group count
                    OnPropertyChanged(nameof(GroupedEntities));
                    break;
                }
            }
        }

        public void AddEntityToTree(PowerConsumptionEntity entity)
        {
            // Find appropriate group and add entity back
            var groupName = entity.Type.Name.Contains("Smart") ? "Smart Meters" : "Interval Meters";
            var group = GroupedEntities.FirstOrDefault(g => g.TypeName == groupName);

            if (group != null)
            {
                group.Entities.Add(entity);
                // Refresh group count
                OnPropertyChanged(nameof(GroupedEntities));
            }
            else
            {
                // If group doesn't exist, refresh all groupings
                LoadGroupedEntities();
            }
        }

        #endregion
    }
}