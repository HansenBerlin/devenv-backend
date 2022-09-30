Create database UsersDb;
use UsersDb;

Create Table "User" 
( 
    Id INT IDENTITY(1,1) 
        NOT NULL, 
    [Name] NVARCHAR(MAX), 
    [Mail] VARCHAR(100),
    [Age] INT
)