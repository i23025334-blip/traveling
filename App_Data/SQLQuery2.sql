-- Drop the Users table if it exists
DROP TABLE IF EXISTS Users;

-- Create the Users table
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(255) NULL,
    Passwd NVARCHAR(255) NOT NULL,
    IsVerified BIT NOT NULL DEFAULT 0,
    IsAdmin BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Verify table creation
SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users';
SELECT * FROM Users;

-- Insert a user into the Users table
-- INSERT INTO Users (Username, Email, Passwd, IsVerified, IsAdmin)
-- VALUES ('admin', 'admin@example.com', 'admin', 1, 1);

