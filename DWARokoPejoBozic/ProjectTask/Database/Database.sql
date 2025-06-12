-- E-Commerce Project Database Schema
CREATE DATABASE ECommerceDB

USE ECommerceDB
-- Create Country table
CREATE TABLE Country (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL
);

-- Create Tag table
CREATE TABLE Tag (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL
);

-- Create Product table
CREATE TABLE Product (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(10,2) NOT NULL,
    ImageUrl NVARCHAR(300),
    CountryId INT NOT NULL,
    FOREIGN KEY (CountryId) REFERENCES Country(Id)
);

-- Create ProductTag table (many-to-many bridge)
CREATE TABLE ProductTag (
    ProductId INT NOT NULL,
    TagId INT NOT NULL,
    PRIMARY KEY (ProductId, TagId),
    FOREIGN KEY (ProductId) REFERENCES Product(Id),
    FOREIGN KEY (TagId) REFERENCES Tag(Id)
);

-- Create User table
CREATE TABLE [User] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'User'
);

-- Create Order table (User M-to-N entity)
CREATE TABLE [Order] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    OrderDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CountryId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES [User](Id),
    FOREIGN KEY (ProductId) REFERENCES Product(Id),
    FOREIGN KEY (CountryId) REFERENCES Country(Id)
);

CREATE TABLE Log (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Level NVARCHAR(20) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL
);
