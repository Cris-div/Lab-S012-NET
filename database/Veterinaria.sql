-- PostgreSQL 17. Ejecutar en una base vacía, también en Neon SQL Editor.
-- Database First: este esquema es el origen del modelo EF Core.
BEGIN;

CREATE TABLE "Propietarios" (
    "Id" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Nombre" varchar(100) NOT NULL CHECK (length(trim("Nombre")) > 0),
    "Documento" varchar(8) NOT NULL,
    "Telefono" varchar(15) NOT NULL,
    CONSTRAINT "UQ_Propietarios_Documento" UNIQUE ("Documento"),
    CONSTRAINT "CK_Propietarios_Documento" CHECK ("Documento" ~ '^[0-9]{8}$'),
    CONSTRAINT "CK_Propietarios_Telefono" CHECK ("Telefono" ~ '^[0-9+ -]{7,15}$')
);

CREATE TABLE "Mascotas" (
    "Id" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Nombre" varchar(80) NOT NULL CHECK (length(trim("Nombre")) > 0),
    "Especie" varchar(40) NOT NULL CHECK (length(trim("Especie")) > 0),
    "Edad" integer NOT NULL CHECK ("Edad" BETWEEN 0 AND 50),
    "Peso" numeric(7,2) NOT NULL CHECK ("Peso" BETWEEN 0.01 AND 1000),
    "PropietarioId" integer NOT NULL,
    CONSTRAINT "FK_Mascotas_Propietarios" FOREIGN KEY ("PropietarioId")
        REFERENCES "Propietarios" ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_Mascotas_PropietarioId" ON "Mascotas" ("PropietarioId");

INSERT INTO "Propietarios" ("Nombre", "Documento", "Telefono") VALUES
('Ana Torres', '12345678', '987654321'),
('Luis Ramos', '87654321', '912345678'),
('Carla Flores', '45678912', '956123456');

INSERT INTO "Mascotas" ("Nombre", "Especie", "Edad", "Peso", "PropietarioId") VALUES
('Luna', 'Perro', 3, 12.50, 1),
('Milo', 'Gato', 2, 4.20, 1),
('Rocky', 'Perro', 5, 22.80, 2);

COMMIT;
