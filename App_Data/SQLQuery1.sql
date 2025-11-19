CREATE TABLE Product (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) NOT NULL,
    price DECIMAL(18,2) NOT NULL,
    quantity INT NOT NULL,
    unit NVARCHAR(100) NOT NULL,
    origin NVARCHAR(255) NOT NULL
);

CREATE TABLE TravelPackage (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255),
    price DECIMAL(18,2),
    duration INT,
    description NVARCHAR(MAX),
    country NVARCHAR(100)
);
