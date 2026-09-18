USE TbtbChallenge;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Patients)
BEGIN
    DECLARE @inicioMes DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);

    INSERT INTO dbo.Gestors (Name, Email)
    VALUES
        (N'Laura Gómez', N'laura.gomez@programa.local'),
        (N'Andrés Peña', N'andres.pena@programa.local'),
        (N'María José Salazar', N'maria.salazar@programa.local'),
        (N'Carlos Restrepo', N'carlos.restrepo@programa.local'),
        (N'Diana Milena Torres', N'diana.torres@programa.local'),
        (N'Jorge Luis Paredes', N'jorge.paredes@programa.local');

    INSERT INTO dbo.Patients (Name, DocumentType, DocumentNumber, Phone, Email, City, TreatmentStartDate)
    VALUES
        (N'María Fernanda Rojas', N'CC', N'CC-1001', N'+573105550011', N'maria.fernanda.rojas@ejemplo.co', N'Bogotá', DATEADD(MONTH, -3, GETDATE())),
        (N'Diego Alejandro Suárez', N'CC', N'CC-1002', N'+573105550012', N'diego.suarez@ejemplo.co', N'Bogotá', DATEADD(MONTH, -5, GETDATE())),
        (N'Ana Lucía Ortega', N'CC', N'CC-1003', N'+573105550013', N'ana.ortega@ejemplo.co', N'Bogotá', DATEADD(MONTH, -1, GETDATE())),
        (N'Santiago Gaviria', N'CC', N'CC-1004', N'+573105550014', N'santiago.gaviria@ejemplo.co', N'Medellín', DATEADD(MONTH, -4, GETDATE())),
        (N'Valentina Restrepo', N'CC', N'CC-1005', N'+573105550015', N'valentina.restrepo@ejemplo.co', N'Medellín', DATEADD(MONTH, -2, GETDATE())),
        (N'Juan Pablo Céspedes', N'CC', N'CC-1006', N'+573105550016', N'juan.cespedes@ejemplo.co', N'Cali', DATEADD(MONTH, -6, GETDATE())),
        (N'Daniela Caicedo', N'CC', N'CC-1007', N'+573105550017', N'daniela.caicedo@ejemplo.co', N'Cali', DATEADD(MONTH, -2, GETDATE())),
        (N'Rosa Elena Quispe', N'CC', N'CC-1008', N'+519325550011', N'rosa.quispe@ejemplo.pe', N'Lima', DATEADD(MONTH, -3, GETDATE())),
        (N'Pedro Pablo Ramírez', N'CC', N'CC-1009', N'+519325550012', N'pedro.ramirez@ejemplo.pe', N'Lima', DATEADD(MONTH, -5, GETDATE())),
        (N'Laura Milagros Huamán', N'CC', N'CC-1010', N'+519325550013', N'laura.huaman@ejemplo.pe', N'Lima', DATEADD(MONTH, -1, GETDATE())),
        (N'Andrés Sebastián Mora', N'CC', N'CC-1011', N'+593965550011', N'andres.mora@ejemplo.ec', N'Quito', DATEADD(MONTH, -4, GETDATE())),
        (N'Camila Jaramillo', N'CC', N'CC-1012', N'+593965550012', N'camila.jaramillo@ejemplo.ec', N'Quito', DATEADD(MONTH, -1, GETDATE())),
        (N'Miguel Ángel Cando', N'CC', N'CC-1013', N'+593965550013', N'miguel.cando@ejemplo.ec', N'Quito', DATEADD(MONTH, -6, GETDATE())),
        (N'José Daniel Vera', N'CC', N'CC-1014', N'+593965550014', N'jose.vera@ejemplo.ec', N'Guayaquil', DATEADD(MONTH, -3, GETDATE())),
        (N'Paula Estefanía Cruz', N'CC', N'CC-1015', N'+593965550015', N'paula.cruz@ejemplo.ec', N'Guayaquil', DATEADD(MONTH, -2, GETDATE())),
        (N'Kevin Alexander Zambrano', N'CC', N'CC-1016', N'+593965550016', N'kevin.zambrano@ejemplo.ec', N'Guayaquil', DATEADD(MONTH, -5, GETDATE()));

    INSERT INTO dbo.Contacts (PatientId, GestorId, ContactDate, Channel, Result, Notes)
    SELECT p.Id, g.Id,
           DATEADD(DAY, (fila.Dia % DAY(GETDATE())), @inicioMes),
           fila.Canal, fila.Resultado, fila.Nota
    FROM (VALUES
        (N'CC-1001', N'Laura Gómez', 1, N'llamada', N'contestado', N'Primera llamada de seguimiento'),
        (N'CC-1002', N'Laura Gómez', 2, N'whatsapp', N'contestado', NULL),
        (N'CC-1003', N'Laura Gómez', 2, N'llamada', N'buzón', N'Se marcó por error; el paciente respondió'),
        (N'CC-1004', N'Andrés Peña', 3, N'correo', N'contestado', NULL),
        (N'CC-1005', N'Andrés Peña', 4, N'llamada', N'no contesta', N'Se reintenta la próxima semana'),
        (N'CC-1006', N'Andrés Peña', 5, N'llamada', N'contestado', N'Duda sobre la entrega del medicamento'),
        (N'CC-1007', N'María José Salazar', 6, N'whatsapp', N'reagendado', N'Prefiere que la contacten por la mañana'),
        (N'CC-1007', N'María José Salazar', 8, N'llamada', N'contestado', N'Seguimiento del reagendamiento'),
        (N'CC-1008', N'María José Salazar', 9, N'llamada', N'número equivocado', NULL),
        (N'CC-1008', N'María José Salazar', 10, N'llamada', N'contestado', N'Número actualizado en el registro'),
        (N'CC-1009', N'Carlos Restrepo', 11, N'correo', N'contestado', NULL),
        (N'CC-1010', N'Carlos Restrepo', 12, N'llamada', N'buzón', NULL),
        (N'CC-1011', N'Carlos Restrepo', 13, N'whatsapp', N'contestado', N'Confirmó la próxima cita'),
        (N'CC-1011', N'Diana Milena Torres', 15, N'llamada', N'contestado', NULL),
        (N'CC-1012', N'Diana Milena Torres', 14, N'correo', N'no contesta', N'Sin respuesta al correo'),
        (N'CC-1013', N'Diana Milena Torres', 15, N'llamada', N'contestado', NULL),
        (N'CC-1014', N'Diana Milena Torres', 16, N'whatsapp', N'buzón', NULL),
        (N'CC-1014', N'Jorge Luis Paredes', 17, N'llamada', N'contestado', N'Reportó efecto secundario leve'),
        (N'CC-1015', N'Jorge Luis Paredes', 18, N'llamada', N'no contesta', NULL),
        (N'CC-1016', N'Jorge Luis Paredes', 19, N'whatsapp', N'contestado', NULL),
        (N'CC-1016', N'Laura Gómez', 19, N'llamada', N'reagendado', N'Queda para la próxima semana'),
        (N'CC-1001', N'Andrés Peña', 20, N'llamada', N'contestado', NULL),
        (N'CC-1002', N'María José Salazar', 21, N'correo', N'contestado', NULL),
        (N'CC-1004', N'Carlos Restrepo', 22, N'llamada', N'número equivocado', N'El paciente corrigió el número por WhatsApp'),
        (N'CC-1005', N'Jorge Luis Paredes', 23, N'llamada', N'contestado', NULL),
        (N'CC-1006', N'Diana Milena Torres', 24, N'whatsapp', N'contestado', N'Consulta sobre horario del centro'),
        (N'CC-1009', N'Andrés Peña', 25, N'llamada', N'no contesta', NULL),
        (N'CC-1010', N'María José Salazar', 25, N'whatsapp', N'contestado', NULL),
        (N'CC-1012', N'Jorge Luis Paredes', 26, N'llamada', N'buzón', NULL),
        (N'CC-1013', N'Laura Gómez', 26, N'llamada', N'contestado', N'Respuesta a lo registrado por error')
    ) AS fila (Documento, Gestor, Dia, Canal, Resultado, Nota)
    JOIN dbo.Patients p ON p.DocumentNumber = fila.Documento
    JOIN dbo.Gestors g ON g.Name = fila.Gestor;
END
ELSE
BEGIN
    PRINT N'Seed: la base ya contiene datos; no se inserta nada.';
END
GO
