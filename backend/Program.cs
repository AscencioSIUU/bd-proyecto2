// See https://aka.ms/new-console-template for more information
using Npgsql;
using System.Threading;

const string CONNECTION_STRING = "Host=localhost;Port=5432;Database=proyecto_2;UserId=postgres;Password=p1aE7Eitr";
const int TASK_AMOUNT = 20;



void SimulateUserReservation(string name)
{
    NpgsqlConnection connection = new NpgsqlConnection(CONNECTION_STRING);
    connection.Open();

    NpgsqlCommand isSeatAvailable = new NpgsqlCommand("SELECT is_available FROM seat WHERE id = @seat_id", connection);
    isSeatAvailable.Parameters.AddWithValue("@seat_id", 1);

    bool isAvailable = false;
    NpgsqlDataReader reader = isSeatAvailable.ExecuteReader();
    if (reader.Read())
    {
        isAvailable = reader.GetBoolean(0);
    }
    reader.Close();
    isSeatAvailable.Dispose();

    if (!isAvailable)
    {
        Console.WriteLine("Asiento no disponible");
        connection.Close();
        connection.Dispose();
        return;
    }

    NpgsqlTransaction transaction = connection.BeginTransaction();

    try
    {
        string insertReservationQuery = "INSERT INTO reservation (event_id, user_id) VALUES (@event_id, @user_id) RETURNING id";
        NpgsqlCommand insertReservation = new NpgsqlCommand(insertReservationQuery, connection, transaction);
        insertReservation.Parameters.AddWithValue("@event_id", 1);
        insertReservation.Parameters.AddWithValue("@user_id", 1);

        object reservationIdObj = insertReservation.ExecuteScalar();
        int reservationId = (int)reservationIdObj;
        insertReservation.Dispose();

        string updateSeatQuery = "UPDATE seat SET is_available = false WHERE id = @seat_id";
        NpgsqlCommand updateSeat = new NpgsqlCommand(updateSeatQuery, connection, transaction);
        updateSeat.Parameters.AddWithValue("@seat_id", 1);
        updateSeat.ExecuteNonQuery();
        updateSeat.Dispose();

        string insertReservationSeatQuery = "INSERT INTO reservation_seats(seat_id, reservation_id) VALUES (@seat_id, @reservation_id)";
        NpgsqlCommand insertReservationSeat = new NpgsqlCommand(insertReservationSeatQuery, connection, transaction);
        insertReservationSeat.Parameters.AddWithValue("@seat_id", 1);
        insertReservationSeat.Parameters.AddWithValue("@reservation_id", reservationId);
        insertReservationSeat.ExecuteNonQuery();
        insertReservationSeat.Dispose();

        transaction.Commit();
        Console.WriteLine("Transaccion exitosa");
    }
    catch (Exception e)
    {
        transaction.Rollback();
        Console.WriteLine("Error en transaccion:" + e.Message);
    }

    transaction.Dispose();
    connection.Close();
    connection.Dispose();
}

void LogSimulation(int i)
{
    Console.WriteLine($"Start thread {i}");
    SimulateUserReservation($"thread {i}");
    Console.WriteLine($"End thread {i}");
}

Console.WriteLine("DB concurreny simulation");
Console.WriteLine("Press ENTER to start");
Console.ReadLine();

void RunSimulation()
{
    for (int i = 0; i < TASK_AMOUNT; i++)
    {
        int copy = i;
        Task.Run(() => { LogSimulation(copy); });
    }
}

SimulateUserReservation("");


Console.ReadLine();
