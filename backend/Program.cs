
// File: Program.cs
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ConcurrencySimulation
{
    class Program
    {
        private const string CONNECTION_STRING =
            "Host=localhost;Port=5432;Database=proyect2_DB;UserId=escu;Password=123456";

        // Asientos distintos para cada nivel de aislamiento
        private static readonly Dictionary<System.Data.IsolationLevel, int> SeatByIsolation =
            new()
            {
                { System.Data.IsolationLevel.ReadCommitted, 1 },
                { System.Data.IsolationLevel.RepeatableRead, 2 },
                { System.Data.IsolationLevel.Serializable, 3 }
            };

        static async Task Main(string[] args)
        {
            int[] userCounts = { 5, 10, 20, 30 };
            var resultsByUsers = new Dictionary<int, List<Result>>();

            foreach (int users in userCounts)
            {
                var results = new List<Result>();
                Console.WriteLine($"\n--- Ejecutando para {users} usuarios ---\n");

                foreach (var kv in SeatByIsolation)
                {
                    var isolation = kv.Key;
                    var seatId = kv.Value;

                    Console.WriteLine($"Preparando simulación: Nivel={isolation}, Asiento={seatId}");
                    ResetDatabase(seatId);

                    Console.WriteLine($"Ejecutando simulación con {users} usuarios y nivel {isolation}...");
                    var res = await RunSimulation(users, isolation, seatId);
                    results.Add(res);

                    Console.WriteLine($"  → Éxitos: {res.SuccessCount}, Fallos: {res.FailureCount}, Tiempo Promedio: {res.AverageTimeMs} ms\n");
                }

                resultsByUsers[users] = results;
            }

            // Imprimir tablas comparativas
            foreach (int users in userCounts)
            {
                Console.WriteLine($"\n=== Resultados para {users} usuarios ===");
                Console.WriteLine($"{"Nivel",-16} {"Exitosas",8} {"Fallidas",8} {"Tiempo(ms)",10}");
                Console.WriteLine(new string('-', 46));
                foreach (var r in resultsByUsers[users])
                {
                    Console.WriteLine($"{r.Isolation,-16} {r.SuccessCount,8} {r.FailureCount,8} {r.AverageTimeMs,10}");
                }
            }

            Console.WriteLine("\nProceso completado. Presiona ENTER para salir.");
            Console.ReadLine();
        }

        // Limpia las tablas y deja disponible el asiento indicado
        private static void ResetDatabase(int seatId)
        {
            using var conn = new NpgsqlConnection(CONNECTION_STRING);
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                // Truncar y reiniciar IDs
                using (var cmd = new NpgsqlCommand(
                    "TRUNCATE reservation_seats, reservation RESTART IDENTITY CASCADE;", conn, tx))
                {
                    cmd.ExecuteNonQuery();
                }
                // Marcar el asiento como disponible
                using (var cmd2 = new NpgsqlCommand(
                    "UPDATE seat SET is_available = TRUE WHERE id = @id;", conn, tx))
                {
                    cmd2.Parameters.AddWithValue("id", seatId);
                    cmd2.ExecuteNonQuery();
                }
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // Estructura para guardar resultados
        record Result(
            System.Data.IsolationLevel Isolation,
            int SuccessCount,
            int FailureCount,
            long AverageTimeMs
        );

        private static async Task<Result> RunSimulation(int users, System.Data.IsolationLevel isolationLevel, int seatId)
        {
            int success = 0, failure = 0;
            var times = new List<long>();
            var tasks = new List<Task>();
            var lockSuccess = new object();
            var lockFailure = new object();

            for (int i = 1; i <= users; i++)
            {
                int userId = i;
                tasks.Add(Task.Run(() =>
                {
                    var sw = Stopwatch.StartNew();
                    bool ok = SimulateOnce(userId, isolationLevel, seatId);
                    sw.Stop();

                    if (ok)
                    {
                        lock (lockSuccess) { success++; times.Add(sw.ElapsedMilliseconds); }
                    }
                    else
                    {
                        lock (lockFailure) { failure++; }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            long avgTime = times.Count > 0 ? (long)times.Average() : 0;
            return new Result(isolationLevel, success, failure, avgTime);
        }

        // Simula una sola transacción; retorna true si commit, false si rollback o indisponible
        private static bool SimulateOnce(int userId, System.Data.IsolationLevel isolationLevel, int seatId)
        {
            try
            {
                using var conn = new NpgsqlConnection(CONNECTION_STRING);
                conn.Open();

                using var tx = conn.BeginTransaction(isolationLevel);
                try
                {
                    // 1) Selección con lock: FOR UPDATE
                    bool isAvailable;
                    using (var check = new NpgsqlCommand(
                        "SELECT is_available FROM seat WHERE id = @id FOR UPDATE", conn, tx))
                    {
                        check.Parameters.AddWithValue("id", seatId);
                        var val = check.ExecuteScalar();
                        isAvailable = val != null && (bool)val;
                    }

                    if (!isAvailable)
                    {
                        tx.Rollback();
                        return false;
                    }

                    // 2) Insertar reserva y capturar ID
                    int resId;
                    using (var ins = new NpgsqlCommand(
                        "INSERT INTO reservation(event_id,user_id) VALUES(1,@u) RETURNING id",
                        conn, tx))
                    {
                        ins.Parameters.AddWithValue("u", userId);
                        resId = (int)ins.ExecuteScalar();
                    }

                    // 3) Actualizar asiento a no disponible
                    using (var upd = new NpgsqlCommand(
                        "UPDATE seat SET is_available = false WHERE id = @id",
                        conn, tx))
                    {
                        upd.Parameters.AddWithValue("id", seatId);
                        upd.ExecuteNonQuery();
                    }

                    // 4) Insertar relación reservation_seats
                    using (var rel = new NpgsqlCommand(
                        "INSERT INTO reservation_seats(seat_id,reservation_id) VALUES(@s,@r)",
                        conn, tx))
                    {
                        rel.Parameters.AddWithValue("s", seatId);
                        rel.Parameters.AddWithValue("r", resId);
                        rel.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return true;
                }
                catch (PostgresException pg) when (pg.SqlState == "40001")
                {
                    tx.Rollback();
                    return false;
                }
                catch
                {
                    tx.Rollback();
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

