/*
    Crea la base de datos si no existe. Los demas scripts numerados asumen que ya existe
    (empiezan con "USE AelbryDb;"), asi que este debe ser el primero en aplicarse.
*/

IF DB_ID('AelbryDb') IS NULL
BEGIN
    CREATE DATABASE AelbryDb;
END
GO
