USE [C14]
GO
/****** Object:  StoredProcedure [dbo].[AppUsers_UpdateV2]    Script Date: 4/11/2016 11:58:41 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROC [dbo].[AppUsers_UpdateV2]
	 @UserId nvarchar(128)
	,@FirstName nvarchar(50)
	,@LastName nvarchar(50)
	,@UserName nvarchar(256)
	,@PhoneId int
	,@Number nvarchar(50)
	,@Extension nvarchar(50) = null
	
/*
	
DECLARE
	@UserId nvarchar(128) = '031311ef-1e6e-4f7b-9874-5ebedc4cdddd'
	,@FirstName nvarchar(50)='New Name'
	,@LastName nvarchar(50)='LastName'
	,@UserName nvarchar(256) = 'Fsdfadsafdzy'
	,@PhoneId int = 101
	,@Number nvarchar(50) ='(555)555-5555'
	,@Extension nvarchar(50) = '3'

EXEC dbo.AppUsers_SelectByUserId
@UserId

EXEC dbo.AppUsers_UpdateV2
	@UserId
	,@FirstName
	,@LastName 
	,@UserName
	,@PhoneId
	,@Number
	,@Extension 

select * from dbo.AppUsers

--select * from dbo.Phones

*/

AS

BEGIN


UPDATE dbo.AppUsers
	SET  [FirstName] = @FirstName
		,[LastName] = @LastName

	WHERE UserId = @UserId

EXEC AspNetUsers_UpdateUserName
			 @UserId
			,@UserName

EXEC dbo.Phones_Update 
			 @PhoneId 
			,@Number 
			,@Extension 

END