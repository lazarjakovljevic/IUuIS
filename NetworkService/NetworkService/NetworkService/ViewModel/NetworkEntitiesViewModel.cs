using NetworkService.Commands;
using NetworkService.Controls;
using NetworkService.Model;
using NetworkService.MVVM;
using NetworkService.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        public Action ScrollToTopAction { get; set; }

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

        public ObservableCollection<EntityType> AvailableTypes { get; set; }
        public ObservableCollection<EntityType> FilterTypes { get; set; }

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
                string filteredValue = FilterNumericInput(value);

                if (value != filteredValue)
                {
                    TriggerFilterIdError();
                }
                else
                {

                    HasFilterIdError = false;
                }

                SetProperty(ref filterIdValue, filteredValue);
                ApplyFilters();
            }
        }

        private bool isLessThanSelected = true;
        public bool IsLessThanSelected
        {
            get { return isLessThanSelected; }
            set
            {
                if (SetProperty(ref isLessThanSelected, value) && value)
                {

                    isGreaterThanSelected = false;
                    isEqualSelected = false;
                    OnPropertyChanged(nameof(IsGreaterThanSelected));
                    OnPropertyChanged(nameof(IsEqualSelected));

                    ApplyFilters();
                }
            }
        }

        private bool isGreaterThanSelected;
        public bool IsGreaterThanSelected
        {
            get { return isGreaterThanSelected; }
            set
            {
                if (SetProperty(ref isGreaterThanSelected, value) && value)
                {

                    isLessThanSelected = false;
                    isEqualSelected = false;
                    OnPropertyChanged(nameof(IsLessThanSelected));
                    OnPropertyChanged(nameof(IsEqualSelected));

                    ApplyFilters();
                }
            }
        }

        private bool isEqualSelected;
        public bool IsEqualSelected
        {
            get { return isEqualSelected; }
            set
            {
                if (SetProperty(ref isEqualSelected, value) && value)
                {

                    isLessThanSelected = false;
                    isGreaterThanSelected = false;
                    OnPropertyChanged(nameof(IsLessThanSelected));
                    OnPropertyChanged(nameof(IsGreaterThanSelected));

                    ApplyFilters();
                }
            }
        }

        private bool hasFilterIdError = false;
        public bool HasFilterIdError
        {
            get { return hasFilterIdError; }
            set { SetProperty(ref hasFilterIdError, value); }
        }

        #endregion

        #region Virtual Keyboard Integration

        private VirtualKeyboardService keyboardService;
        public void InitializeVirtualKeyboard()
        {
            try
            {
                keyboardService = VirtualKeyboardService.Instance;
                keyboardService.TextInput += OnGlobalKeyboardInput;
                keyboardService.KeyboardVisibilityChanged += OnGlobalKeyboardVisibilityChanged;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing virtual keyboard in NetworkEntitiesViewModel: {ex.Message}");
            }
        }

        private void OnGlobalKeyboardInput(object sender, VirtualKeyEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case VirtualKeyAction.Enter:
                        if (ValidateFilterIdInput(FilterIdValue))
                        {
                            OnScrollToTopRequested();
                        }
                        break;
                    case VirtualKeyAction.Character:
                        break;
                    case VirtualKeyAction.Backspace:
                        break;
                    case VirtualKeyAction.Shift:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling global keyboard input: {ex.Message}");
            }
        }

        #endregion

        #region Scrolling Support

        public Action ScrollToBottomAction { get; set; }
        public Action<double> ScrollToVerticalOffsetAction { get; set; }
        public Action<Thickness> SetScrollViewerPaddingAction { get; set; }

        private void OnScrollToTopRequested()
        {
            try
            {
                ScrollToTopAction?.Invoke();
                keyboardService.HideKeyboard();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scrolling to top: {ex.Message}");
            }
        }

        private void OnScrollToBottomRequested()
        {
            try
            {
                ScrollToBottomAction?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scrolling to bottom: {ex.Message}");
            }
        }

        private void SetScrollViewerPadding(Thickness padding)
        {
            try
            {
                SetScrollViewerPaddingAction?.Invoke(padding);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting scroll viewer padding: {ex.Message}");
            }
        }

        private void ScrollTextBoxIntoView(System.Windows.Controls.TextBox textBox)
        {
            try
            {
                if (textBox != null)
                {
                    textBox.BringIntoView();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scrolling TextBox into view: {ex.Message}");
            }
        }

        private void OnGlobalKeyboardVisibilityChanged(object sender, KeyboardVisibilityEventArgs e)
        {
            try
            {
                if (e.IsVisible)
                {
                    var activeTextBox = keyboardService?.ActiveTextBox;
                    if (activeTextBox != null)
                    {
                        if (IsFilterTextBox(activeTextBox))
                        {
                            var middlePosition = 0.75;
                            ScrollToVerticalOffsetAction?.Invoke(middlePosition);
                        }
                        else
                        {
                            SetScrollViewerPadding(new Thickness(0, 0, 0, 275));
                            OnScrollToBottomRequested();
                        }

                        ScrollTextBoxIntoView(activeTextBox);
                    }
                }
                else
                {
                    SetScrollViewerPadding(new Thickness(0));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling keyboard visibility change: {ex.Message}");
            }
        }

        private bool IsFilterTextBox(TextBox textBox)
        {
            if (textBox == null) return false;

            try
            {
                var binding = textBox.GetBindingExpression(TextBox.TextProperty);
                if (binding?.ParentBinding?.Path?.Path != null)
                {
                    var path = binding.ParentBinding.Path.Path;
                    return path.Contains("Filter");
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Constructor
        private NetworkEntitiesViewModel()
        {
            InitializeCollections();
            InitializeCommands();
            SetupFiltering();
            InitializeVirtualKeyboard();
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

        private string FilterNumericInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string result = "";
            foreach (char c in input)
            {
                if (char.IsDigit(c))
                {
                    result += c;
                }
            }
            return result;
        }

        public void TriggerFilterIdError()
        {
            try
            {
                HasFilterIdError = true;

                Task.Run(async () =>
                {
                    await Task.Delay(200);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        HasFilterIdError = false;
                    });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error triggering filter ID error: {ex.Message}");
            }
        }

        public bool IsTextAllowed(string text)
        {
            return text.All(char.IsDigit);
        }

        public bool ValidateFilterIdInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (!IsTextAllowed(input))
            {
                TriggerFilterIdError();
                return false;
            }

            return true;
        }

        private void ValidateId()
        {
            if (string.IsNullOrWhiteSpace(NewEntityId))
            {
                IdValidationMessage = "";
                return;
            }

            if (!int.TryParse(NewEntityId, out int id))
            {
                IdValidationMessage = "ID must be a number";
                return;
            }

            if (id < 0)
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
                NameValidationMessage = "";
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
                TypeValidationMessage = "";
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
                if (string.IsNullOrWhiteSpace(NewEntityId))
                {
                    IdValidationMessage = "ID is required";
                }

                if (string.IsNullOrWhiteSpace(NewEntityName))
                {
                    NameValidationMessage = "Name is required";
                }

                if (NewEntityType == null)
                {
                    TypeValidationMessage = "Type is required";
                }

                if (!CanAddEntity())
                {
                    return;
                }

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

                OnScrollToTopRequested();
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

            if (SelectedFilterType != null && !SelectedFilterType.Name.Equals("All Types"))
            {
                if (!entity.Type.Equals(SelectedFilterType))
                    return false;
            }

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