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
                // Notify related properties
                OnPropertyChanged(nameof(IsValueValid));
                OnPropertyChanged(nameof(ValueStatus));
            }
        }

        // Computed properties
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
            CurrentValue = 1.5; // Safe default
        }

        public PowerConsumptionEntity(int id, string name, EntityType type)
        {
            Id = id;
            Name = name;
            Type = type;
            CurrentValue = 1.5; // Safe default
        }
        #endregion

        #region Static helpers
        // Static validation helper
        public static bool IsValidValue(double value)
        {
            return value >= 0.34 && value <= 2.73;
        }
        #endregion

        #region Equality
        // Object equality 
        public override bool Equals(object obj)
        {
            if (obj is PowerConsumptionEntity other)
                return Id == other.Id;
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        #endregion

        #region Overrides
        // ToString for display
        public override string ToString()
        {
            return $"{Name} (ID: {Id})";
        }
        #endregion
    }
}
