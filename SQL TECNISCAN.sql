-- Crear la tabla Clientes
CREATE TABLE Clientes (
    ClienteID INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE
);

-- Crear la tabla Habitaciones
CREATE TABLE Habitaciones (
    HabitacionID INT PRIMARY KEY IDENTITY(1,1),
    TipoHabitacion VARCHAR(50) NOT NULL,
    PrecioPorNoche DECIMAL(10, 2) NOT NULL
);

-- Crear la tabla Reservas
CREATE TABLE Reservas (
    ReservaID INT PRIMARY KEY IDENTITY(1,1),
    ClienteID INT,
    HabitacionID INT,
    FechaEntrada DATETIME NOT NULL,
    FechaSalida DATETIME NOT NULL,
    Total DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (ClienteID) REFERENCES Clientes(ClienteID),
    FOREIGN KEY (HabitacionID) REFERENCES Habitaciones(HabitacionID)
);

SELECT 
    c.ClienteID,
    c.Nombre,
    SUM(r.Total) AS TotalIngresos
FROM 
    Clientes c
LEFT JOIN 
    Reservas r ON c.ClienteID = r.ClienteID
GROUP BY 
    c.ClienteID, c.Nombre;


	SELECT 
    r.ReservaID,
    c.Nombre AS NombreCliente,
    h.TipoHabitacion,
    r.FechaEntrada,
    r.FechaSalida,
    r.Total
FROM 
    Reservas r
JOIN 
    Clientes c ON r.ClienteID = c.ClienteID
JOIN 
    Habitaciones h ON r.HabitacionID = h.HabitacionID
WHERE 
    r.FechaSalida > GETDATE();







SELECT * FROM Reservas
SELECT * FROM Habitaciones

CREATE PROCEDURE RegistrarReserva
    @ClienteID INT,
    @HabitacionID INT,
    @FechaEntrada DATETIME,
    @FechaSalida DATETIME,
    @Total DECIMAL OUTPUT
AS
BEGIN
    DECLARE @PrecioPorNoche DECIMAL;
    
    -- Obtener el precio por noche de la habitación
    SELECT @PrecioPorNoche = PrecioPorNoche
    FROM Habitaciones
    WHERE HabitacionID = @HabitacionID;
    
    -- Calcular el total de la reserva
    SET @Total = DATEDIFF(DAY, @FechaEntrada, @FechaSalida) * @PrecioPorNoche;
    
    -- Insertar la reserva
    INSERT INTO Reservas (ClienteID, HabitacionID, FechaEntrada, FechaSalida, Total)
    VALUES (@ClienteID, @HabitacionID, @FechaEntrada, @FechaSalida, @Total);
END;



INSERT INTO Clientes (Nombre, Email)
VALUES 
    ('Juan Pérez', 'juan.perez@example.com'),
    ('María López', 'maria.lopez@example.com'),
    ('Carlos García', 'carlos.garcia@example.com');


	INSERT INTO Habitaciones (TipoHabitacion, PrecioPorNoche)
VALUES 
    ('Individual', 50.00),
    ('Doble', 75.00),
    ('Suite', 120.00);


INSERT INTO Reservas (ClienteID, HabitacionID, FechaEntrada, FechaSalida, Total)
VALUES 
    (1, 2, '2024-10-10', '2024-10-15', 375.00);  -- Ejemplo de total calculado: 5 noches * 75.00



	INSERT INTO Reservas (ClienteID, HabitacionID, FechaEntrada, FechaSalida, Total)
VALUES 
    (1, 2, '2024-10-10', '2024-10-15', 375.00),  -- Juan Pérez en habitación doble
    (2, 1, '2024-10-12', '2024-10-14', 100.00), -- María López en habitación individual
    (3, 3, '2024-10-15', '2024-10-20', 600.00); -- Carlos García en suite
	SELECT * FROM Reservas;

	EXEC RegistrarReservas @ClienteID = 1, @HabitacionID = 2, @FechaEntrada = NULL, @FechaSalida = NULL;



	SELECT * FROM Reservas

