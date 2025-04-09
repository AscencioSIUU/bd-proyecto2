// See https://aka.ms/new-console-template for more information
using Npgsql;
using System;
using System.Threading.Tasks;

static class Program
{
    // Leemos la cadena de conexión de la variable de entorno
    static readonly string CONNECTION_STRING =
        Environment.GetEnvironmentVariable("CONNECTION_STRING")
        ?? throw new Exception("No CONNECTION_STRING provided");

    // Número de hilos que vamos a lanzar
    const int TASK_AMOUNT = 20;

    static void Main()
    {
        Console.WriteLine("DB concurrency simulation");
        Console.WriteLine("Press ENTER to start");
        Console.ReadLine();

        // Lanzamos TASK_AMOUNT tareas en paralelo
        for (int i = 0; i < TASK_AMOUNT; i++)
        {
            int copy = i; // captura segura del índice
            Task.Run(() => LogThread(copy));
        }

        // Evitamos que la aplicación termine inmediatamente
        Console.WriteLine("Presiona ENTER para salir");
        Console.ReadLine();
    }

    // Método que hace el INSERT en la base de datos
    static void CreateDatabaseConnection(string name)
    {
        using var connection = new NpgsqlConnection(CONNECTION_STRING);
        connection.Open();

        using var cmd = new NpgsqlCommand(
            "INSERT INTO customer(name) VALUES ($1);",
            connection);
        cmd.Parameters.AddWithValue(name);

        // Ejecuta el INSERT de forma síncrona
        cmd.ExecuteNonQuery();
    }

    // Método que se ejecuta en cada hilo
    static void LogThread(int i)
    {
        Console.WriteLine($"Start thread {i}");
        CreateDatabaseConnection($"thread {i}");
        Console.WriteLine($"End thread {i}");
    }
}

