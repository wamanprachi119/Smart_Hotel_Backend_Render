-- ============================================================
-- Smart Hotel — MySQL Setup Script
-- Run this ONCE in MySQL Workbench or MySQL CLI
-- before starting the backend
-- ============================================================

-- Step 1: Create the database
CREATE DATABASE IF NOT EXISTS smart_hotel
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

-- Step 2: Make sure user 'root' can access it
-- (If you use a different user, change 'root'@'localhost' below)
GRANT ALL PRIVILEGES ON smart_hotel.* TO 'root'@'localhost';
FLUSH PRIVILEGES;

-- Step 3: Select database
USE smart_hotel;

-- Tables are auto-created by the backend (EnsureCreated).
-- Just run Steps 1-2 above and then start the backend.

SELECT 'Database ready! Now run: dotnet run' AS status;
