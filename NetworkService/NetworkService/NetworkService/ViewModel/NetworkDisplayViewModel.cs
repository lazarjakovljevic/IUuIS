using System.Collections.ObjectModel;
using System.Linq;
using NetworkService.MVVM;
using NetworkService.Model;
using NetworkService.Views;

namespace NetworkService.ViewModel
{
    public class NetworkDisplayViewModel : BindableBase
    {
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

        #region Constructor

        public NetworkDisplayViewModel()
        {
            LoadGroupedEntities();

            // Subscribe to changes in shared entities
            if (SharedEntities != null)
            {
                SharedEntities.CollectionChanged += OnSharedEntitiesChanged;
            }
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
        }

        #endregion
    }
}