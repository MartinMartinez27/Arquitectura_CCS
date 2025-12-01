-- Crear base de datos si no existe
IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'CCS_VehicleTracking')
BEGIN
    CREATE DATABASE CCS_VehicleTracking;
    PRINT 'Base de datos CCS_VehicleTracking creada en Docker';
END
ELSE
BEGIN
    PRINT 'La base de datos CCS_VehicleTracking ya existe en Docker';
END
GO

USE CCS_VehicleTracking;
GO

-- Crear tablas (script completo)
USE [CCS_VehicleTracking]
GO
/****** Object:  Table [dbo].[EmergencySignals]    Script Date: 30/11/2025 4:03:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmergencySignals](
	[EmergencyId] [nvarchar](50) NOT NULL,
	[VehicleId] [nvarchar](50) NOT NULL,
	[EmergencyType] [int] NOT NULL,
	[Source] [nvarchar](50) NOT NULL,
	[Latitude] [float] NOT NULL,
	[Longitude] [float] NOT NULL,
	[Description] [nvarchar](500) NOT NULL,
	[AdditionalData] [nvarchar](max) NULL,
	[IsResolved] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[ResolvedAt] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[EmergencyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notifications]    Script Date: 30/11/2025 4:03:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notifications](
	[NotificationId] [nvarchar](50) NOT NULL,
	[VehicleId] [nvarchar](50) NOT NULL,
	[RuleId] [nvarchar](50) NOT NULL,
	[ActionId] [nvarchar](50) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Recipient] [nvarchar](500) NOT NULL,
	[Message] [nvarchar](max) NOT NULL,
	[IsSent] [bit] NOT NULL,
	[ErrorMessage] [nvarchar](1000) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[SentAt] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[NotificationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RuleActions]    Script Date: 30/11/2025 4:03:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RuleActions](
	[ActionId] [nvarchar](50) NOT NULL,
	[RuleId] [nvarchar](50) NOT NULL,
	[ActionType] [int] NOT NULL,
	[Target] [nvarchar](500) NOT NULL,
	[MessageTemplate] [nvarchar](1000) NOT NULL,
	[Parameters] [nvarchar](max) NULL,
	[DelaySeconds] [int] NOT NULL,
	[IsEnabled] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ActionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Rules]    Script Date: 30/11/2025 4:03:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rules](
	[RuleId] [nvarchar](50) NOT NULL,
	[VehicleId] [nvarchar](50) NULL,
	[Name] [nvarchar](200) NOT NULL,
	[RuleType] [int] NOT NULL,
	[Conditions] [nvarchar](max) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Priority] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[RuleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Vehicles]    Script Date: 30/11/2025 4:03:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vehicles](
	[VehicleId] [nvarchar](50) NOT NULL,
	[LicensePlate] [nvarchar](20) NOT NULL,
	[VehicleType] [int] NOT NULL,
	[OwnerId] [nvarchar](100) NOT NULL,
	[Model] [nvarchar](100) NOT NULL,
	[Brand] [nvarchar](100) NOT NULL,
	[Year] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[VehicleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[LicensePlate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VehicleTelemetries]    Script Date: 30/11/2025 4:03:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VehicleTelemetries](
	[TelemetryId] [nvarchar](50) NOT NULL,
	[VehicleId] [nvarchar](50) NOT NULL,
	[VehicleType] [int] NOT NULL,
	[Latitude] [float] NOT NULL,
	[Longitude] [float] NOT NULL,
	[Speed] [float] NOT NULL,
	[Direction] [float] NOT NULL,
	[IsMoving] [bit] NOT NULL,
	[EngineOn] [bit] NOT NULL,
	[FuelLevel] [float] NOT NULL,
	[CargoTemperature] [float] NULL,
	[CargoStatus] [nvarchar](100) NULL,
	[IsPlannedStop] [bit] NULL,
	[Timestamp] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TelemetryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[EmergencySignals] ADD  DEFAULT ((0)) FOR [IsResolved]
GO
ALTER TABLE [dbo].[EmergencySignals] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT ((0)) FOR [IsSent]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RuleActions] ADD  DEFAULT ((0)) FOR [DelaySeconds]
GO
ALTER TABLE [dbo].[RuleActions] ADD  DEFAULT ((1)) FOR [IsEnabled]
GO
ALTER TABLE [dbo].[Rules] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Rules] ADD  DEFAULT ((1)) FOR [Priority]
GO
ALTER TABLE [dbo].[Rules] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Vehicles] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Vehicles] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[EmergencySignals]  WITH CHECK ADD FOREIGN KEY([VehicleId])
REFERENCES [dbo].[Vehicles] ([VehicleId])
GO
ALTER TABLE [dbo].[RuleActions]  WITH CHECK ADD FOREIGN KEY([RuleId])
REFERENCES [dbo].[Rules] ([RuleId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Rules]  WITH CHECK ADD FOREIGN KEY([VehicleId])
REFERENCES [dbo].[Vehicles] ([VehicleId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[VehicleTelemetries]  WITH CHECK ADD FOREIGN KEY([VehicleId])
REFERENCES [dbo].[Vehicles] ([VehicleId])
GO
INSERT INTO Vehicles (VehicleId, LicensePlate, VehicleType, OwnerId, Model, Brand, Year) VALUES
('TRUCK001', 'ABC123', 1, 'OWNER001', 'FH16', 'Volvo', 2023),
('CAR001', 'DEF456', 2, 'OWNER002', 'Corolla', 'Toyota', 2022),
('MOTO001', 'GHI789', 3, 'OWNER003', 'Ninja', 'Kawasaki', 2023);

INSERT INTO Rules (RuleId, VehicleId, Name, RuleType, Conditions) VALUES
('RULE001', 'TRUCK001', 'Detención no planeada camión', 1, '{"max_stop_time_minutes": 5, "allowed_locations": []}'),
('RULE002', NULL, 'Emergencia botón pánico', 4, '{"emergency_types": [1]}'),
('RULE003', 'CAR001', 'Movimiento en horario no permitido', 6, '{"allowed_hours": {"start": "06:00", "end": "22:00"}}');

INSERT INTO RuleActions (ActionId, RuleId, ActionType, Target, MessageTemplate, DelaySeconds) VALUES
('ACTION001', 'RULE001', 1, 'owner@company.com', 'El vehículo {VehicleId} se ha detenido inesperadamente en {Location}', 0),
('ACTION002', 'RULE002', 2, 'police_authority', 'Emergencia reportada desde vehículo {VehicleId} en {Location}', 0),
('ACTION003', 'RULE002', 3, 'rescue_service', 'Se requiere asistencia para vehículo {VehicleId}', 0);