using NetworkService.MVVM;

namespace NetworkService.Model
{
    public class PowerConsumptionEntity : BindableBase
    {
        #region Fields
        private int id;
        private string name;
        private EntityType type;
        private double currentValue;
        #endregion

        #region Properties
        public int Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        public EntityType Type
        {
            get { return type; }
            set { SetProperty(ref type, value); }
        }

        public double CurrentValue
        {
            get { return currentValue; }
            set
            {
                SetProperty(ref currentValue, value);

                OnPropertyChanged(nameof(IsValueValid));
                OnPropertyChanged(nameof(ValueStatus));
            }
        }

        public bool IsValueValid
        {
            get { return CurrentValue >= 0.34 && CurrentValue <= 2.73; }
        }

        public string ValueStatus
        {
            get { return IsValueValid ? "Normal" : "Alert"; }
        }
        #endregion

        #region Constructors
        public PowerConsumptionEntity()
        {
            CurrentValue = 1.5; // Default value
        }

        public PowerConsumptionEntity(int id, string name, EntityType type)
        {
            Id = id;
            Name = name;
            Type = type;
            CurrentValue = 1.5; // Default value
        }
        #endregion

    }
}
