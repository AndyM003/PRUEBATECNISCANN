using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

public class Program
{
    private static string connectionString = "Server=KIRA\\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"; // conexion BDD

    public static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("Gestión de Reservas del Hotel");
            Console.WriteLine("1. Listar habitaciones disponibles");
            Console.WriteLine("2. Registrar nueva reserva");
            Console.WriteLine("3. Consultar reservas de un cliente");
            Console.WriteLine("4. Salir");
            Console.Write("Selecciona una opción: ");
            var opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    ListarHabitaciones();
                    break;
                case "2":
                    RegistrarReserva();
                    break;
                case "3":
                    ConsultarReservasCliente();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Opción no válida. Intenta de nuevo.");
                    break;
            }
        }
    }
    //Funcion para ver habitaciones disponibles
    private static void ListarHabitaciones()
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Habitaciones";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine("\nHabitaciones disponibles:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"ID: {reader["HabitacionID"]}, Tipo: {reader["TipoHabitacion"]}, Precio por Noche: {reader["PrecioPorNoche"]:C}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al listar habitaciones: {ex.Message}");
        }
        Console.WriteLine();
    }
    // Funcion para registrar la reserva
    private static void RegistrarReserva()
    {
        Console.Write("Ingrese el ClienteID: ");
        string clienteIDInput = Console.ReadLine();

        if (string.IsNullOrEmpty(clienteIDInput) || !int.TryParse(clienteIDInput, out int clienteID))
        {
            Console.WriteLine("ClienteID no válido.");
            return;
        }

        if (!ClienteExiste(clienteID))
        {
            Console.WriteLine("Cliente no encontrado.");
            return;
        }

        Console.Write("Ingrese el HabitacionID: ");
        string habitacionIDInput = Console.ReadLine();

        if (string.IsNullOrEmpty(habitacionIDInput) || !int.TryParse(habitacionIDInput, out int habitacionID))
        {
            Console.WriteLine("HabitacionID no válido.");
            return;
        }

        if (!HabitacionExiste(habitacionID))
        {
            Console.WriteLine("Habitación no encontrada.");
            return;
        }

        Console.Write("Ingrese la fecha de entrada (yyyy-mm-dd): ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime fechaEntrada))
        {
            Console.WriteLine("Fecha de entrada no válida.");
            return;
        }

        Console.Write("Ingrese la fecha de salida (yyyy-mm-dd): ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime fechaSalida))
        {
            Console.WriteLine("Fecha de salida no válida.");
            return;
        }

        if (fechaEntrada >= fechaSalida)
        {
            Console.WriteLine("La fecha de entrada debe ser anterior a la fecha de salida.");
            return;
        }

        try
        {
            decimal total = CalcularTotalReserva(habitacionID, fechaEntrada, fechaSalida);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Reservas (ClienteID, HabitacionID, FechaEntrada, FechaSalida, Total) VALUES (@ClienteID, @HabitacionID, @FechaEntrada, @FechaSalida, @Total)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClienteID", clienteID);
                    command.Parameters.AddWithValue("@HabitacionID", habitacionID);
                    command.Parameters.AddWithValue("@FechaEntrada", fechaEntrada);
                    command.Parameters.AddWithValue("@FechaSalida", fechaSalida);
                    command.Parameters.AddWithValue("@Total", total);
                    command.ExecuteNonQuery();
                }
            }

            Console.WriteLine($"Reserva registrada con éxito. Total: {total:C}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al registrar reserva: {ex.Message}");
        }
    }
    //Funcion para registrar Reservas
    private static void ConsultarReservasCliente()
    {
        Console.Write("Ingrese el ClienteID: ");
        if (!int.TryParse(Console.ReadLine(), out int clienteID))
        {
            Console.WriteLine("ClienteID no válido.");
            return;
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Reservas WHERE ClienteID = @ClienteID AND FechaSalida > GETDATE()";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClienteID", clienteID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            Console.WriteLine("No hay reservas activas para este cliente.");
                            return;
                        }

                        Console.WriteLine("\nReservas activas del cliente:");
                        while (reader.Read())
                        {
                            Console.WriteLine($"ReservaID: {reader["ReservaID"]}, HabitacionID: {reader["HabitacionID"]}, Fecha Entrada: {((DateTime)reader["FechaEntrada"]).ToShortDateString()}, Fecha Salida: {((DateTime)reader["FechaSalida"]).ToShortDateString()}, Total: {reader["Total"]:C}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al consultar reservas: {ex.Message}");
        }
        Console.WriteLine();
    }
    // Funcion para calcular el total de reservas
    private static decimal CalcularTotalReserva(int habitacionID, DateTime fechaEntrada, DateTime fechaSalida)
    {
        decimal precioPorNoche;

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT PrecioPorNoche FROM Habitaciones WHERE HabitacionID = @HabitacionID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HabitacionID", habitacionID);
                    precioPorNoche = (decimal)command.ExecuteScalar();
                }
            }

            int noches = (fechaSalida - fechaEntrada).Days;
            return noches * precioPorNoche;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al calcular total: {ex.Message}");
            return 0;
        }
    }
    //Funcion para comprobar si el cliente existe
    private static bool ClienteExiste(int clienteID)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Clientes WHERE ClienteID = @ClienteID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClienteID", clienteID);
                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al verificar cliente: {ex.Message}");
            return false;
        }
    }
    //Funcion para comprobar si la habitacion existe
    private static bool HabitacionExiste(int habitacionID)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Habitaciones WHERE HabitacionID = @HabitacionID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HabitacionID", habitacionID);
                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al verificar habitación: {ex.Message}");
            return false;
        }
    }
}
