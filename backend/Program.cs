// See https://aka.ms/new-console-template for more information
using Npgsql;
using System.Threading;

const string CONNECTION_STRING = "Host=localhost;Port=5432;Database=testing;UserId=postgres;Password=p1aE7Eitr";
const int TASK_AMOUNT = 20;



void CreateDatabaseConnection(string name){
    using NpgsqlConnection connection = new NpgsqlConnection(CONNECTION_STRING);
    connection.Open();

    using NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO customer(name) VALUES ($1);", connection);
    cmd.Parameters.AddWithValue(name);

    using NpgsqlDataReader reader = cmd.ExecuteReader();
    cmd.ExecuteNonQueryAsync();
}

void LogThread(int i){
    Console.WriteLine($"Start thread {i}");
    CreateDatabaseConnection($"thread {i}");
    Console.WriteLine($"End thread {i}");
}



Console.WriteLine("DB concurreny simulation");
Console.WriteLine("Press ENTER to start");
Console.ReadLine();

for(int i = 0; i < TASK_AMOUNT; i++){
    int copy = i;
    Task.Run( () => { LogThread(copy); });
}


Console.ReadLine();
