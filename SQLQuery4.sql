USE [C14]
GO
/****** Object:  StoredProcedure [dbo].[Employer_Update]    Script Date: 3/16/2016 2:23:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROC [dbo].[Employer_Update]	

	 @Id int 			
	, @EmployerName nvarchar(200)
	, @Email nvarchar(100) = null
	, @CompanyUrl nvarchar(300) = null
	, @JobTitle nvarchar(100) 
	, @WorkType smallint
	, @CurrentJobStatus bit = null
	, @StartDate datetime 
	, @EndDate datetime = null 
	
/*
Declare 
	  @Id int = 6			
	, @EmployerName nvarchar(200) = 'THIS IS THE UPDATE'
	, @Email nvarchar(100) = 'update@yahoo.com'
	, @CompanyUrl nvarchar(300) = 'www.update.com'
	, @JobTitle nvarchar(100) = 'YEEEEE'
	, @WorkType smallint = 1
	, @CurrentJobStatus bit = 1
	, @StartDate datetime = '01/01/2000'
	, @EndDate datetime = null 

EXEC Employer_Update

	  @Id
	, @EmployerName
	, @Email 
	, @CompanyUrl
	, @JobTitle
	, @WorkType
	, @CurrentJobStatus
	, @StartDate
	, @EndDate

select * from dbo.Employers
where Id = @Id
  
*/

AS
BEGIN


UPDATE [dbo].[Employers]
   SET [EmployerName] = @EmployerName
      ,[Email] = @Email
      ,[CompanyUrl] = @CompanyUrl
      ,[Position] = @JobTitle
      ,[WorkType] = @WorkType
      ,[CurrentJobStatus] = @CurrentJobStatus
      ,[StartDate] = @StartDate
      ,[EndDate] = @EndDate

	  Where Id = @Id
END
