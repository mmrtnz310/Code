USE [C14]
GO
/****** Object:  StoredProcedure [dbo].[AppUsers_SelectByUserId]    Script Date: 3/16/2016 2:24:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROC [dbo].[AppUsers_SelectByUserId]

@UserId nvarchar(128)

AS
BEGIN

/*
    
DECLARE
@UserId nvarchar(128) = 'a4a87658-7d2a-461a-86e0-67a47faf20e2'
   
EXEC dbo.AppUsers_SelectByUserId
@UserId
  

*/

SELECT   AU.Id
		,AU.FirstName
		,AU.LastName
		,AU.DateLastInteracted
		,ANU.Email
		,ANU.UserName
		,AU.UserId
		,P.Number
		,P.Extension
		,P.Id as phoneId
		
		FROM Appusers as AU 

		inner join ASPnetUsers as ANU
		ON AU.UserId = ANU.Id

		inner Join UserPhones as UP 
		ON UP.UserId = AU.UserId

		inner join Phones as P
		ON UP.PhoneId = P.Id
		  
		WHERE Au.UserId = @UserId

END