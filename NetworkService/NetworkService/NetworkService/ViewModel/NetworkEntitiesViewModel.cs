using NetworkService.Commands;
using NetworkService.Model;
using NetworkService.MVVM;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : BindableBase
    {
        #region Singleton Pattern

        private static NetworkEntitiesViewModel instance;
        public static NetworkEntitiesViewModel Instance
        {
            get
            {
                if (instance == null)
                    instance = new NetworkEntitiesViewModel();
                return instance;
            }
        }

        #endregion

        #region Properties

        // Collections
        private ObservableCollection<PowerConsumptionEntity> entities;
        public ObservableCollection<PowerConsumptionEntity> Entities
        {
            get { return entities; }
            set { SetProperty(ref entities, value); }
        }

        private ICollectionView filteredEntities;
        public ICollectionView FilteredEntities
        {
            get { return filteredEntities; }
            set { SetProperty(ref filteredEntities, value); }
        }

        // Available types for ComboBox
        public ObservableCollection<EntityType> AvailableTypes { get; set; }
        public ObservableCollection<EntityType> FilterTypes { get; set; }

        // Selected entity for deletion
        private PowerConsumptionEntity selectedEntity;
        public PowerConsumptionEntity SelectedEntity
        {
            get { return selectedEntity; }
            set
            {
                SetProperty(ref selectedEntity, value);
                DeleteEntityCommand.RaiseCanExecuteChanged();
            }
        }

        // Form fields for new entity
        private string newEntityId;
        public string NewEntityId
        {
            get { return newEntityId; }
            set
            {
                SetProperty(ref newEntityId, value);
                ValidateId();
                AddEntityCommand.RaiseCanExecuteChanged();
            }
        }

        private string newEntityName;
        public string NewEntityName
        {
            get { return newEntityName; }
            set
            {
                SetProperty(ref newEntityName, value);
                ValidateName();
                AddEntityCommand.RaiseCanExecuteChanged();
            }
        }

        private EntityType newEntityType;
        public EntityType NewEntityType
        {
            get { return newEntityType; }
            set
            {
                SetProperty(ref newEntityType, value);
                ValidateType();
                AddEntityCommand.RaiseCanExecuteChanged();
            }
        }

        // Validation messages
        private string idValidationMessage;
        public string IdValidationMessage
        {
            get { return idValidationMessage; }
            set { SetProperty(ref idValidationMessage, value); }
        }

        private string nameValidationMessage;
        public string NameValidationMessage
        {
            get { return nameValidationMessage; }
            set { SetProperty(ref nameValidationMessage, value); }
        }

        private string typeValidationMessage;
        public string TypeValidationMessage
        {
            get { return typeValidationMessage; }
            set { SetProperty(ref typeValidationMessage, value); }
        }

        // Filter properties
        private EntityType selectedFilterType;
        public EntityType SelectedFilterType
        {
            get { return selectedFilterType; }
            set
            {
                SetProperty(ref selectedFilterType, value);
                ApplyFilters();
            }
        }

        private string filterIdValue;
        public string FilterIdValue
        {
            get { return filterIdValue; }
            set
            {
                SetProperty(ref filterIdValue, value);
                ApplyFilters();
            }
        }

        private bool isLessThanSelected = true;
        public bool IsLessThanSelected
        {
            get { return isLessThanSelected; }
            set
            {
                SetProperty(ref isLessThanSelected, value);
                if (value) ApplyFilters();
            }
        }

        private bool isGreaterThanSelected;
        public bool IsGreaterThanSelected
        {
            get { return isGreaterThanSelected; }
            set
            {
                SetProperty(ref isGreaterThanSelected, value);
                if (value) ApplyFilters();
            }
        }

        private bool isEqualSelected;
        public bool IsEqualSelected
        {
            get { return isEqualSelected; }
            set
            {
                SetProperty(ref isEqualSelected, value);
                if (value) ApplyFilters();
            }
        }

        #endregion

        #region Constructor
        private NetworkEntitiesViewModel()
        {
            InitializeCollections();
            InitializeCommands();
            SetupFiltering();
        }
        #endregion

        #region Commands

        public MyICommand AddEntityCommand { get; private set; }
        public MyICommand DeleteEntityCommand { get; private set; }
        public MyICommand ClearFiltersCommand { get; private set; }

        #endregion

        #region Initialization

        private void InitializeCollections()
        {
            Entities = MainWindowViewModel.SharedEntities;

            AvailableTypes = new ObservableCollection<EntityType>
            {
                EntityType.IntervalMeter,
                EntityType.SmartMeter
            };

            FilterTypes = new ObservableCollection<EntityType>
            {
                new EntityType("All Types", ""),
                EntityType.IntervalMeter,
                EntityType.SmartMeter
            };
        }

        private void InitializeCommands()
        {
            AddEntityCommand = new MyICommand(OnAddEntity, CanAddEntity);
            DeleteEntityCommand = new MyICommand(OnDeleteEntity, CanDeleteEntity);
            ClearFiltersCommand = new MyICommand(OnClearFilters);
        }


        private void SetupFiltering()
        {
            FilteredEntities = CollectionViewSource.GetDefaultView(Entities);
            FilteredEntities.Filter = EntityFilter;
            SelectedFilterType = FilterTypes.First(); // "All Types"
        }

        #endregion

        #region Validation

        private void ValidateId()
        {
            if (string.IsNullOrWhiteSpace(NewEntityId))
            {
                IdValidationMessage = "ID is required";
                return;
            }

            if (!int.TryParse(NewEntityId, out int id))
            {
                IdValidationMessage = "ID must be a number";
                return;
            }

            if (id <= 0)
            {
                IdValidationMessage = "ID must be positive";
                return;
            }

            if (Entities.Any(e => e.Id == id))
            {
                IdValidationMessage = "ID already exists";
                return;
            }

            IdValidationMessage = "";
        }

        private void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(NewEntityName))
            {
                NameValidationMessage = "Name is required";
                return;
            }

            if (NewEntityName.Length < 3)
            {
                NameValidationMessage = "Name must be at least 3 characters";
                return;
            }

            NameValidationMessage = "";
        }

        private void ValidateType()
        {
            if (NewEntityType == null)
            {
                TypeValidationMessage = "Type is required";
                return;
            }

            TypeValidationMessage = "";
        }

        #endregion

        #region Command Implementations

        private bool CanAddEntity()
        {
            return string.IsNullOrEmpty(IdValidationMessage) &&
                   string.IsNullOrEmpty(NameValidationMessage) &&
                   string.IsNullOrEmpty(TypeValidationMessage) &&
                   !string.IsNullOrWhiteSpace(NewEntityId) &&
                   !string.IsNullOrWhiteSpace(NewEntityName) &&
                   NewEntityType != null;
        }

        private void OnAddEntity()
        {
            try
            {
                var newEntity = new PowerConsumptionEntity(
                    int.Parse(NewEntityId),
                    NewEntityName.Trim(),
                    NewEntityType
                );

                var addCommand = new AddEntityCommand(Entities, newEntity);
                UndoManager.Instance.ExecuteCommand(addCommand);

                NewEntityId = "";
                NewEntityName = "";
                NewEntityType = null;

                MessageBox.Show("Entity added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding entity: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDeleteEntity()
        {
            return SelectedEntity != null;
        }

        private void OnDeleteEntity()
        {
            if (SelectedEntity == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedEntity.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Use UndoManager instead of direct remove
                var deleteCommand = new DeleteEntityCommand(Entities, SelectedEntity);
                UndoManager.Instance.ExecuteCommand(deleteCommand);

                MessageBox.Show("Entity deleted successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnClearFilters()
        {
            SelectedFilterType = FilterTypes.First(); // "All Types"
            FilterIdValue = "";
            IsLessThanSelected = true;
            IsGreaterThanSelected = false;
            IsEqualSelected = false;
        }

        #endregion

        #region Filtering

        private bool EntityFilter(object item)
        {
            var entity = item as PowerConsumptionEntity;
            if (entity == null) return false;

            // Filter by type
            if (SelectedFilterType != null && !SelectedFilterType.Name.Equals("All Types"))
            {
                if (!entity.Type.Equals(SelectedFilterType))
                    return false;
            }

            // Filter by ID
            if (!string.IsNullOrWhiteSpace(FilterIdValue) && int.TryParse(FilterIdValue, out int filterValue))
            {
                if (IsLessThanSelected && entity.Id >= filterValue) return false;
                if (IsGreaterThanSelected && entity.Id <= filterValue) return false;
                if (IsEqualSelected && entity.Id != filterValue) return false;
            }

            return true;
        }

        private void ApplyFilters()
        {
            if (FilteredEntities != null)
            {
                FilteredEntities.Refresh();
            }
        }

        #endregion
    }
}