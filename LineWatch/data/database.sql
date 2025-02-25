IF not exists (SELECT * FROM sys.databases WHERE name='emc_prod') BEGIN CREATE DATABASE [emc_prod]; END;
USE emc_prod;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.AddBox'))
   exec('CREATE PROCEDURE [dbo].[AddBox]')
GO
ALTER PROCEDURE [AddBox] 
	@Date DATE,
	@Time TIME,
	@labelNumber NVARCHAR(12),
	@Name NVARCHAR(3),
	@Material NVARCHAR(10),
	@Amount INT
AS
BEGIN
	INSERT INTO prod VALUES (@Date, @Time, @labelNumber, @Name, @Material, @Amount);
END;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'dbo.plc')
BEGIN
	CREATE TABLE [plc](
		[name] [nchar](3) NOT NULL,
		[ip] [nchar](15) NOT NULL,
		[port] [int] NULL,
		[printer] [nchar](10) NULL,
		[print_label] [bit] NOT NULL,
	 CONSTRAINT [PK_plc] PRIMARY KEY CLUSTERED ([name] ASC)
	 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) 
	 ON [PRIMARY]) 
	 ON [PRIMARY];
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'dbo.prod')
BEGIN
	CREATE TABLE [prod](
		[date] [date] NOT NULL,
		[time] [time](7) NOT NULL,
		[label] [nchar](12) NOT NULL,
		[line] [nchar](3) NOT NULL,
		[material] [nchar](10) NOT NULL,
		[amount] [int] NOT NULL,
	 CONSTRAINT [PK_prod] PRIMARY KEY CLUSTERED 
	([label] ASC)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) 
	ON [PRIMARY]) 
	ON [PRIMARY];
END;

ALTER DATABASE [emc_prod] SET READ_WRITE;
