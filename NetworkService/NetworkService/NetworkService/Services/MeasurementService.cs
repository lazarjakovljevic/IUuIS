using System;
using System.IO;
using System.Collections.ObjectModel;
using NetworkService.Model;
using System.Linq;

namespace NetworkService.Services
{
    public class MeasurementService
    {
        #region Constants

        private const string LOG_FILE_PATH = "measurements.txt";

        #endregion

        #region Singleton Pattern

        private static MeasurementService instance;
        private static readonly object lockObject = new object();

        public static MeasurementService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                            instance = new MeasurementService();
                    }
                }
                return instance;
            }
        }

        #endregion

        #region Events

        public event Action<PowerConsumptionEntity, double> MeasurementReceived;
        public event Action<string> AlertTriggered;

        #endregion

        #region Constructor

        private MeasurementService()
        {
            EnsureLogFileExists();
        }

        #endregion

        #region Public Methods

        public void ProcessTcpMessage(string message, ObservableCollection<PowerConsumptionEntity> entities)
        {
            try
            {
                // Parse message format: "Entitet_ID:Value" -> Example: "Entitet_1:1.55"
                if (string.IsNullOrEmpty(message) || !message.Contains(":"))
                {
                    LogError($"Invalid message format: {message}");
                    return;
                }

                var parts = message.Split(':');
                if (parts.Length != 2)
                {
                    LogError($"Invalid message format: {message}");
                    return;
                }

                // Extract entity ID from "Entitet_ID"
                var entityPart = parts[0];
                if (!entityPart.StartsWith("Entitet_"))
                {
                    LogError($"Invalid entity format: {entityPart}");
                    return;
                }

                var idString = entityPart.Substring(8); // Remove "Entitet_"
                if (!int.TryParse(idString, out int entityId))
                {
                    LogError($"Invalid entity ID: {idString}");
                    return;
                }

                // Parse value
                if (!double.TryParse(parts[1], out double value))
                {
                    LogError($"Invalid value: {parts[1]}");
                    return;
                }

                // Find entity and update
                var entity = entities.FirstOrDefault(e => e.Id == entityId);
                if (entity != null)
                {
                    var oldValue = entity.CurrentValue;

                    // Update value on UI thread to ensure proper binding
                    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    {
                        entity.CurrentValue = value;
                    });

                    // Log the measurement
                    LogMeasurement(entityId, value, entity.IsValueValid);

                    // Check for alerts
                    if (!entity.IsValueValid)
                    {
                        var alertMessage = $"ALERT: Entity '{entity.Name}' (ID: {entityId}) has invalid value: {value:F2} kWh (valid range: 0.34-2.73)";
                        AlertTriggered?.Invoke(alertMessage);
                    }

                    // Notify subscribers
                    MeasurementReceived?.Invoke(entity, value);

                    Console.WriteLine($"Updated Entity {entityId}: {oldValue:F2} -> {value:F2} kWh");
                }
                else
                {
                    LogError($"Entity with ID {entityId} not found");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error processing TCP message '{message}': {ex.Message}");
            }
        }

        public void LogMeasurement(int entityId, double value, bool isValid)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var status = isValid ? "NORMAL" : "ALERT";
                var logEntry = $"[{timestamp}] Entity_ID: {entityId}, Value: {value:F2} kWh, Status: {status}";

                lock (lockObject)
                {
                    File.AppendAllText(LOG_FILE_PATH, logEntry + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        public void LogError(string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logEntry = $"[{timestamp}] ERROR: {message}";

                lock (lockObject)
                {
                    File.AppendAllText(LOG_FILE_PATH, logEntry + Environment.NewLine);
                }

                Console.WriteLine(logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing error to log file: {ex.Message}");
            }
        }

        public string[] ReadRecentMeasurements(int entityId, int count = 5)
        {
            try
            {
                if (!File.Exists(LOG_FILE_PATH))
                    return new string[0];

                var allLines = File.ReadAllLines(LOG_FILE_PATH);
                var entityLines = allLines
                    .Where(line => line.Contains($"Entity_ID: {entityId}") && !line.Contains("ERROR"))
                    .Skip(Math.Max(0, allLines.Length - count)) // Take last N elements
                    .ToArray();

                // Alternative approach - reverse, take, reverse back
                var filteredLines = allLines
                    .Where(line => line.Contains($"Entity_ID: {entityId}") && !line.Contains("ERROR"))
                    .ToList();

                if (filteredLines.Count <= count)
                    return filteredLines.ToArray();

                return filteredLines.Skip(filteredLines.Count - count).ToArray();
            }
            catch (Exception ex)
            {
                LogError($"Error reading recent measurements: {ex.Message}");
                return new string[0];
            }
        }

        #endregion

        #region Private Methods

        private void EnsureLogFileExists()
        {
            try
            {
                if (!File.Exists(LOG_FILE_PATH))
                {
                    var header = $"=== Network Service Measurements Log - Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===" + Environment.NewLine;
                    File.WriteAllText(LOG_FILE_PATH, header);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating log file: {ex.Message}");
            }
        }

        #endregion
    }
}