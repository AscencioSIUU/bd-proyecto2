using Npgsql;
using System.Threading;

const string CONNECTION_STRING = "Host=localhost;Port=5432;Database=proyecto_2;UserId=postgres;Password=p1aE7Eitr";
const int USERS = 20;
const int RACE_CONDITIONS_SEAT = 1;

static void SimulateUserReservation(int user_id)
{
    NpgsqlConnection connection = new NpgsqlConnection(CONNECTION_STRING);
    connection.Open();

    NpgsqlCommand isSeatAvailable = new NpgsqlCommand("SELECT is_available FROM seat WHERE id = @seat_id", connection);
    isSeatAvailable.Parameters.AddWithValue("@seat_id", RACE_CONDITIONS_SEAT);

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
        insertReservation.Parameters.AddWithValue("@user_id", user_id);

        object reservationIdObj = insertReservation.ExecuteScalar();
        int reservationId = (int)reservationIdObj;
        insertReservation.Dispose();

        string updateSeatQuery = "UPDATE seat SET is_available = false WHERE id = @seat_id";
        NpgsqlCommand updateSeat = new NpgsqlCommand(updateSeatQuery, connection, transaction);
        updateSeat.Parameters.AddWithValue("@seat_id", RACE_CONDITIONS_SEAT);
        updateSeat.ExecuteNonQuery();
        updateSeat.Dispose();

        string insertReservationSeatQuery = "INSERT INTO reservation_seats(seat_id, reservation_id) VALUES (@seat_id, @reservation_id)";
        NpgsqlCommand insertReservationSeat = new NpgsqlCommand(insertReservationSeatQuery, connection, transaction);
        insertReservationSeat.Parameters.AddWithValue("@seat_id", RACE_CONDITIONS_SEAT);
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

static void LogSimulation(int i)
{
    Console.WriteLine($"Start thread {i}");
    SimulateUserReservation(i);
    Console.WriteLine($"End thread {i}");
}

Console.WriteLine("DB concurreny simulation");
Console.WriteLine("Press ENTER to start");
Console.ReadLine();

static async Task RunSimulation()
{
    List<Task> taskList = new List<Task>();
        
    for (int i = 0; i < USERS; i++)
    {
        int copy = i;
        Task task = Task.Run(() => { LogSimulation(copy); });
        taskList.Add(task);
    }

    await Task.WhenAll(taskList);
}
var watch = System.Diagnostics.Stopwatch.StartNew();

await RunSimulation();

watch.Stop();
var elapsedMs = watch.ElapsedMilliseconds;

Console.WriteLine($"{elapsedMs} ");
Console.ReadLine();


