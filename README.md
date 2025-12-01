Arquitectura CCS – Event-Driven Telemetry Platform
Este repositorio implementa la arquitectura base para el sistema de monitoreo vehicular de CCS (Compañía Colombiana de Seguimiento de Vehículos).
La solución está diseñada bajo un estilo orientado a eventos, permitiendo alta disponibilidad, resiliencia y escalabilidad horizontal mediante colas, particiones y workers paralelos.


🚀 Tecnologías utilizadas
.NET 9 Web API
Docker & Docker Compose
Kafka + Zookeeper
Arquitectura por capas (Domain, Application, Infrastructure, API)

🧱 Arquitectura General
La solución sigue un flujo event-driven:
API Telemetry & Emergency: Recibe telemetría y eventos de emergencia desde los vehículos.
Kafka Cluster: Se encarga de almacenar, distribuir y balancear los eventos por particiones, usando vehicleId como clave de particionado.
Worker de Procesamiento (Background Worker): Consume los mensajes desde Kafka, los valida, transforma y persiste en la base de datos.
SQL Server / Storage Layer: Guarda la telemetría procesada y los eventos de emergencia.
Este enfoque permite:
- Escalar horizontalmente el API y los workers
- Garantizar orden por vehículo
- Mantener alta disponibilidad frente a fallos
- Asegurar consistencia eventual en el sistema

🐳 Ejecución con Docker Compose
La solución incluye todos los servicios necesarios:
API de Telemetría y Emergencias
Worker de procesamiento
Kafka + Zookeeper
SQL Server (si está incluido en el compose)
1. Clonar el repositorio
git clone https://github.com/MartinMartinez27/Arquitectura_CCS.git
cd Arquitectura_CCS

2. Construir y ejecutar los contenedores
docker compose up --build
docker compose up -d

Docker levantará:
API Telemetry/Emergencies
Kafka + Zookeeper
Servicios internos definidos en el docker-compose.yml

3. Detener los contenedores
docker compose down

4. Limpiar volúmenes (opcional)
docker compose down -v

🧪 Pruebas cURL (PowerShell)
Aquí tenés los comandos completos para probar los endpoints principales.

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

📈 ¿Qué sucede al enviar los cURL?

La API recibe la telemetría/emergencia
Publica un evento en Kafka
El Worker consume el evento
Procesa la información
Persiste en la base de datos
La arquitectura queda lista para escalar por demanda:
    instancias del API
    workers paralelos
    particiones en Kafka

🛠️ Notas finales
Si usas Kafka, asegurate de que los puertos no estén siendo usados por otros procesos.
