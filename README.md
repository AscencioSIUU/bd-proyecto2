# Proyecto 2 – ACID y Concurrencia

## Título y Descripción

Este proyecto desarrolla un sistema que simula reservas concurrentes en un evento, utilizando un lenguaje de programación que permite el manejo de concurrencia [hilos, procesos paralelos, etc.](cite: 1). El objetivo principal es comprender el manejo de transacciones, bloqueos y concurrencia en un entorno práctico[cite: 2, 3, 4].

## Estructura del Repositorio

```bash
.
├── backend
│   ├── backend.csproj
│   ├── bin
│   ├── obj
│   └── Program.cs
└── bd
    ├── data.sql
    └── ddl.sql

```

## Requisitos

- Cualquier lenguaje de programación que permita concurrencia (hilos, procesos paralelos), en este caso en específio se usara C#.
- PostgreSQL.
- .NET SDK 9.0 (si se ejecuta el backend localmente).

## Configuración de la Base de Datos Local

1. Instalar PostgreSQL.
2. Crear una base de datos llamada `proyect2_DB`.
3. Ejecutar el script `bd/ddl.sql` para crear las tablas
4. Ejecutar el script `bd/data.sql` para insertar datos de prueba


## Ejecución Local

1. Asegurarse de tener instalado el .NET SDK 9.0.
2. Navegar al directorio `backend`.
3. En el Program.cs cambiar el CONNECTION_STRING, con las credenciales de tu preferencia
4. Restaurar las dependencias:

   ```bash
   dotnet restore
   ```

5. Ejecutar la aplicación:

   ```bash
   dotnet build
   ```

   ```bash
   dotnet run
   ```

## Resultados

Los resultados de las pruebas de concurrencia se mostraran en un formato parecido al siguiente:

| Usuarios Concurrentes | Nivel de Aislamiento | Reservas Exitosas | Reservas Fallidas | Tiempo Promedio |
| :-------------------- | :------------------- | :---------------- | :---------------- | :-------------- |
| 5                     | READ COMMITTED       | 4                 | 1                 | 120 ms          |
| 10                    | REPEATABLE READ      | 8                 | 2                 | 150 ms          |
| 20                    | SERIALIZABLE         | 15                | 5                 | 300 ms          |
| 30                    | SERIALIZABLE         | 22                | 8                 | 500 ms          |

## Entregables

- Diagrama Entidad-Relación (ER).
- Script SQL para la creación de la base de datos (`ddl.sql`).
- Script SQL para la carga de datos de prueba (`data.sql`).
- Código fuente del programa [enlace al repositorio de GitHub].
- Manual de uso para ejecutar la simulación (este readme).
- Informe con los resultados de las pruebas.
- Informe en PDF con análisis y reflexiones.

## Autores

- Esteban Enrique Cárcamo Urízar
- Hugo Daniel Barillas
- Ernesto David Ascencio
