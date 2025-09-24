using NetworkService.MVVM;
using System;

namespace NetworkService.Model
{
    public class Measurement : BindableBase
    {
        #region Fields
        public DateTime Timestamp { get; set; }
        public int EntityId { get; set; }
        public double Value { get; set; }
        public bool IsValid { get; set; }
        public string Status { get; set; }
        #endregion

        #region Constructor
        public Measurement(DateTime timestamp, int entityId, double value, bool isValid, string status)
        {
            Timestamp = timestamp;
            EntityId = entityId;
            Value = value;
            IsValid = isValid;
            Status = status;
        }

        #endregion
    }
}