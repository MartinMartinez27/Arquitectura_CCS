# Arquitectura CCS – Event-Driven Telemetry Platform

Este repositorio implementa la arquitectura base del sistema de monitoreo vehicular de CCS (Compañía Colombiana de Seguimiento de Vehículos).
La solución está diseñada bajo un enfoque event-driven, permitiendo alta disponibilidad, resiliencia y escalabilidad horizontal mediante colas, particiones y workers paralelos.

## 🚀 Tecnologías utilizadas

- **.NET 9 Web API**
- **Docker & Docker Compose**
- **Kafka + Zookeeper**
- **Arquitectura por capas** (Domain, Application, Infrastructure, API)
- **GitHub Actions** para CI/CD
- **Cobertura de pruebas unitarias** automática

## 🧱 Arquitectura General
La solución sigue un flujo orientado a eventos:
API Telemetry & Emergency: recibe telemetría y emergencias desde vehículos.
Kafka Cluster: almacena, balancea y distribuye eventos usando el vehicleId como clave de particionado para mantener orden por vehículo.
Background Worker: consume los mensajes de Kafka, valida la información y la persiste.
SQL Server / Storage Layer: almacena la telemetría procesada.
Este enfoque permite:
Escalar horizontalmente el API y los workers
Garantizar orden por vehículo
Mantener alta disponibilidad ante fallas
Lograr consistencia eventual en el sistema

## 🐳 Ejecución con Docker Compose
Incluye todos los servicios necesarios:
API de Telemetría y Emergencias
Worker de procesamiento
Kafka + Zookeeper
SQL Server (si el compose lo incluye)

# 1. Clonar el repositorio
- git clone https://github.com/MartinMartinez27/Arquitectura_CCS.git
- cd Arquitectura_CCS

# 2. Construir y ejecutar los contenedores
- docker compose up --build
- docker-compose -f docker-compose.yml up -d
- o en segundo plano: docker compose up -d

Docker levantará:
- API Telemetry/Emergencies
- Kafka + Zookeeper
- Workers y servicios internos definidos en el docker-compose.yml

# 3. Detener los contenedores
- docker compose down

# 4. Limpiar volúmenes (opcional)
- docker compose down -v

## 🧪 Pruebas cURL (PowerShell)
1. Enviar Telemetría Vehicular
curl -Method POST http://localhost:5000/api/telemetry/vehicle `
   -Headers @{ "Content-Type" = "application/json" } `
   -Body '{
     "vehicleId":"TRUCK001",
     "vehicleType":1,
     "latitude":4.710989,
     "longitude":-74.072092,
     "speed":65.5,
     "direction":45.0,
     "isMoving":true,
     "engineOn":true,
     "fuelLevel":75.0,
     "cargoTemperature":18.5,
     "cargoStatus":"Normal",
     "isPlannedStop":false,
     "timestamp":"2024-01-15T10:30:00Z"
   }'

2. Enviar Alerta de Emergencia
curl -Method POST http://localhost:5000/api/telemetry/emergency `
   -Headers @{ "Content-Type" = "application/json" } `
   -Body '{
     "vehicleId":"TRUCK001",
     "emergencyType":1,
     "source":"panic_button",
     "latitude":4.710989,
     "longitude":-74.072092,
     "description":"TEST - Sistema Dockerizado Funcionando",
     "additionalData":"{\"test\": \"docker_success\"}"
   }'

## 📈 Flujo tras enviar los cURL

- La API recibe la telemetría/emergencia
- Publica un evento en Kafka
- El Worker consume el mensaje
- Procesa la información
- Persiste en la base de datos
- Esto deja la arquitectura lista para escalar mediante:
-- Múltiples instancias del API
-- Workers paralelos
-- Particiones adicionales en Kafka

## 🟩 GitHub Actions — Pruebas unitarias con Coverage
El repositorio incluye un workflow en GitHub Actions que:
- Compila el proyecto
- Ejecuta las pruebas unitarias
- Genera reporte de cobertura
- Publica los resultados directamente en la pestaña Actions del repositorio
- Esto garantiza calidad continua del código y validación automática en cada push o pull request.

🛠️ Notas finales
Si usás Kafka de manera local, asegurate de que los puertos no estén ocupados por otros procesos.

El docker-compose.yml puede ampliarse para agregar métricas, dashboards y más workers según la demanda.
