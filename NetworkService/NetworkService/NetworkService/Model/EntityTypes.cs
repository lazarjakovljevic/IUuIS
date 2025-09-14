using NetworkService.MVVM;

namespace NetworkService.Model
{
    public class EntityType : BindableBase
    {
        #region Fields
        private string name;
        private string imagePath;
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        public string ImagePath
        {
            get { return imagePath; }
            set { SetProperty(ref imagePath, value); }
        }
        #endregion

        #region Constructors
        public EntityType()
        {
        }

        public EntityType(string name, string imagePath)
        {
            Name = name;
            ImagePath = imagePath;
        }
        #endregion

        #region Static predefined types
        public static EntityType IntervalMeter => new EntityType("Interval Meter", "/Resources/Images/brojiloIntervala.png");
        public static EntityType SmartMeter => new EntityType("Smart Meter", "/Resources/Images/pametnoBrojilo.png");
        #endregion

        #region Helpers
        // Helper for ComboBox binding
        public static EntityType[] GetAllTypes()
        {
            return new EntityType[] { IntervalMeter, SmartMeter };
        }
        #endregion

        #region Equality
        // Object equality 
        public override bool Equals(object obj)
        {
            if (obj is EntityType other)
                return Name?.Equals(other.Name) == true;
            return false;
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }
        #endregion

        #region Overrides
        // ToString for display
        public override string ToString()
        {
            return Name ?? string.Empty;
        }
        #endregion
    }
}
