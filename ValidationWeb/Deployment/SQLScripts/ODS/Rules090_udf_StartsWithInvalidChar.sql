IF EXISTS (SELECT * 
		FROM INFORMATION_SCHEMA.ROUTINES
		WHERE ROUTINE_NAME = 'StartsWithInvalidChar'
			AND ROUTINE_SCHEMA = 'dbo')
	DROP FUNCTION [dbo].StartsWithInvalidChar
GO

CREATE FUNCTION dbo.StartsWithInvalidChar (@String VARCHAR(200))

RETURNS BIT 

BEGIN

DECLARE @InvalidChar AS BIT

SELECT @InvalidChar = 
	   CASE
		WHEN @String IS NULL THEN 0
		WHEN (ASCII(LEFT(@String,1)) BETWEEN 65 and 90) THEN 0
		WHEN (ASCII(LEFT(@String,1)) BETWEEN 97 and 122) THEN 0
		ELSE 1
	   END
RETURN @InvalidChar;

END;