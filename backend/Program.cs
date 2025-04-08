using System;

class Program
{

     static async Task Main(string[] args)
     {
          Console.WriteLine("Simulador de concurrencia con reservas");
          Console.WriteLine("Nivel de aislamiento:\n 1: Read Committed\n 2: Repeatable Read\n 3: Serializable\n");
          var isolationInput = Console.ReadLine();
          Console.WriteLine("Ingresa la cantidad de concurrencia: (5, 10, 20, 30) \n");
          int concurrenceInput = int.Parse(Console.ReadLine() ?? "5");
     }
}