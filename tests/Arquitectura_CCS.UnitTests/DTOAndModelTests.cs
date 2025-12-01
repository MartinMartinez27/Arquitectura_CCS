using Arquitectura_CCS.Common.DTOs;
using Arquitectura_CCS.Common.Models;
using Arquitectura_CCS.Common.Enums;
using System;
using Xunit;

namespace Arquitectura_CCS.UnitTests.Common.Models
{
    public class DTOAndModelTests
    {
        [Fact]
        public void TelemetryData_DefaultValues_ShouldBeInitializedCorrectly()
        {
            var telemetry = new TelemetryData();

            Assert.NotNull(telemetry.TelemetryId);
            Assert.Equal(string.Empty, telemetry.TelemetryId);
            Assert.Equal(string.Empty, telemetry.VehicleId);
            Assert.False(telemetry.IsMoving);
            Assert.False(telemetry.EngineOn);
            Assert.Null(telemetry.CargoTemperature);
            Assert.Null(telemetry.CargoStatus);
            Assert.Null(telemetry.IsPlannedStop);
            Assert.Equal(default(DateTime), telemetry.Timestamp);
        }

        [Fact]
        public void EmergencyData_DefaultValues_ShouldBeInitializedCorrectly()
        {
            var emergency = new EmergencyData();

            Assert.NotNull(emergency.EmergencyId);
            Assert.Equal(string.Empty, emergency.EmergencyId);
            Assert.Equal(string.Empty, emergency.VehicleId);
            Assert.Equal(string.Empty, emergency.Source);
            Assert.Equal(string.Empty, emergency.Description);
            Assert.Null(emergency.AdditionalData);
            Assert.Equal(default(DateTime), emergency.CreatedAt);
            Assert.Equal(string.Empty, emergency.Priority);
        }

        [Fact]
        public void EmergencyAlert_DefaultValues_ShouldBeInitializedCorrectly()
        {
            var alert = new EmergencyAlert();

            Assert.NotNull(alert.AlertId);
            Assert.False(string.IsNullOrWhiteSpace(alert.AlertId));
            Assert.Equal(1, alert.Severity);
            Assert.False(alert.IsResolved);
            Assert.True(alert.Timestamp <= DateTime.UtcNow);
        }

        [Fact]
        public void EmergencySignal_DefaultValues_ShouldBeInitializedCorrectly()
        {
            var signal = new EmergencySignal();

            Assert.NotNull(signal.EmergencyId);
            Assert.False(string.IsNullOrWhiteSpace(signal.EmergencyId));
            Assert.Equal(string.Empty, signal.VehicleId);
            Assert.Equal(EmergencyType.None, signal.EmergencyType);
            Assert.Equal(string.Empty, signal.Source);
            Assert.Equal(string.Empty, signal.Description);
            Assert.Null(signal.AdditionalData);
            Assert.False(signal.IsResolved);
            Assert.True(signal.CreatedAt <= DateTime.UtcNow);
            Assert.Null(signal.ResolvedAt);
            Assert.Null(signal.Vehicle);
        }

        [Fact]
        public void TelemetryData_PropertySetAndGet_ShouldWork()
        {
            var telemetry = new TelemetryData
            {
                TelemetryId = "TID123",
                VehicleId = "VID123",
                VehicleType = 2,
                Latitude = 1.1,
                Longitude = 2.2,
                Speed = 30.5,
                Direction = 180,
                IsMoving = true,
                EngineOn = true,
                FuelLevel = 50,
                CargoTemperature = 5,
                CargoStatus = "Cold",
                IsPlannedStop = true,
                Timestamp = DateTime.UtcNow
            };

            Assert.Equal("TID123", telemetry.TelemetryId);
            Assert.Equal("VID123", telemetry.VehicleId);
            Assert.Equal(2, telemetry.VehicleType);
            Assert.Equal(1.1, telemetry.Latitude);
            Assert.Equal(2.2, telemetry.Longitude);
            Assert.Equal(30.5, telemetry.Speed);
            Assert.Equal(180, telemetry.Direction);
            Assert.True(telemetry.IsMoving);
            Assert.True(telemetry.EngineOn);
            Assert.Equal(50, telemetry.FuelLevel);
            Assert.Equal(5, telemetry.CargoTemperature);
            Assert.Equal("Cold", telemetry.CargoStatus);
            Assert.True(telemetry.IsPlannedStop);
            Assert.True(telemetry.Timestamp <= DateTime.UtcNow);
        }

        [Fact]
        public void VehicleTelemetryRequest_DefaultValues_ShouldBeInitializedCorrectly()
        {
            var telemetry = new VehicleTelemetryRequest();

            Assert.Equal(string.Empty, telemetry.VehicleId);
            Assert.Equal(0, (int)telemetry.VehicleType);
            Assert.Equal(0, telemetry.Latitude);
            Assert.Equal(0, telemetry.Longitude);
            Assert.Equal(0, telemetry.Speed);
            Assert.Equal(0, telemetry.Direction);
            Assert.False(telemetry.IsMoving);
            Assert.False(telemetry.EngineOn);
            Assert.Equal(0, telemetry.FuelLevel);
            Assert.Null(telemetry.CargoTemperature);
            Assert.Null(telemetry.CargoStatus);
            Assert.Null(telemetry.IsPlannedStop);
            Assert.True(telemetry.Timestamp <= DateTime.UtcNow);
        }

        [Fact]
        public void RuleAction_DefaultValues_ShouldBeInitializedCorrectly()
        {
            var action = new RuleAction();

            Assert.NotNull(action.ActionId);
            Assert.Equal(string.Empty, action.RuleId);
            Assert.Equal(ActionType.None, action.ActionType);
            Assert.Equal(string.Empty, action.Target);
            Assert.Equal(string.Empty, action.MessageTemplate);
            Assert.Null(action.Parameters);
            Assert.Equal(0, action.DelaySeconds);
            Assert.True(action.IsEnabled);

            // Ahora no será null
            Assert.NotNull(action.Rule);
            Assert.NotNull(action.Rule.RuleId);
            Assert.Equal(string.Empty, action.Rule.Name);
            Assert.True(action.Rule.CreatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public void Rule_DefaultValues_ShouldBeInitializedCorrectly()
        {
            var rule = new Rule();

            Assert.NotNull(rule.RuleId);
            Assert.Null(rule.VehicleId);
            Assert.Equal(string.Empty, rule.Name);
            Assert.Equal(0, (int)rule.RuleType);
            Assert.Equal(string.Empty, rule.Conditions);
            Assert.True(rule.IsActive);
            Assert.Equal(1, rule.Priority);
            Assert.True(rule.CreatedAt <= DateTime.UtcNow);
            Assert.Null(rule.UpdatedAt);
            Assert.NotNull(rule.Actions);
            Assert.Null(rule.Vehicle);
        }

        [Fact]
        public void Notification_DefaultValues_ShouldBeInitializedCorrectly()
        {
            var notification = new Notification();

            Assert.NotNull(notification.NotificationId);
            Assert.Equal(string.Empty, notification.VehicleId);
            Assert.Equal(string.Empty, notification.RuleId);
            Assert.Equal(string.Empty, notification.ActionId);
            Assert.Equal(string.Empty, notification.Type);
            Assert.Equal(string.Empty, notification.Recipient);
            Assert.Equal(string.Empty, notification.Message);
            Assert.False(notification.IsSent);
            Assert.Null(notification.ErrorMessage);
            Assert.True(notification.CreatedAt <= DateTime.UtcNow);
            Assert.Null(notification.SentAt);
        }

        [Fact]
        public void EmergencySignalRequest_DefaultValues_ShouldBeInitializedCorrectly()
        {
            var signal = new EmergencySignalRequest();

            Assert.Equal(string.Empty, signal.VehicleId);
            Assert.Equal(EmergencyType.None, signal.EmergencyType);
            Assert.Equal("mobile_app", signal.Source);
            Assert.Equal(0, signal.Latitude);
            Assert.Equal(0, signal.Longitude);
            Assert.Equal(string.Empty, signal.Description);
            Assert.Null(signal.AdditionalData);
        }
    }
}
