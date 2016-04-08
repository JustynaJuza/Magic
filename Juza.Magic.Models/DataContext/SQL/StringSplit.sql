IF OBJECT_ID ( 'StringSplit', 'TF') IS NOT NULL 
	DROP FUNCTION StringSplit
GO
 
CREATE  FUNCTION StringSplit(@text varchar(8000), @delimiter varchar(20) = ',')
RETURNS @Output TABLE
(
	value nvarchar(8000)   
)
AS
BEGIN 
	DECLARE @index int

	WHILE (LEN(@text) > 0) 
	BEGIN
		SET @index = CHARINDEX(@delimiter , @text)  
		IF (@index = 0) AND (LEN(@text) > 0)  
		BEGIN  
			INSERT INTO @Output VALUES (@text)
			BREAK  
		END 
 
		IF (@index > 1)  
		BEGIN  
			INSERT INTO @Output VALUES (LEFT(@text, @index - 1))   
			SET @text = RIGHT(@text, (LEN(@text) - @index))  
		END 
		ELSE
			SET @text = RIGHT(@text, (LEN(@text) - @index))
		--SET @index = CHARINDEX(@delimiter , @text)
		--INSERT INTO @Output VALUES (LEFT(@text, @index - 1))   
		--SET @text = RIGHT(@text, (LEN(@text) - @index))
	END
RETURN
END
GO