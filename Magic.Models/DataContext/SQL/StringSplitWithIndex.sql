IF OBJECT_ID ( 'StringSplitWithIndex', 'TF') IS NOT NULL 
	DROP FUNCTION StringSplitWithIndex
GO
 
SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS OFF
GO
 
CREATE  FUNCTION StringSplitWithIndex(@text varchar(8000), @delimiter varchar(20) = ' ')
RETURNS @Output TABLE
(    
  position int IDENTITY PRIMARY KEY,
  value nvarchar(8000)   
)
AS
BEGIN
 
DECLARE @index int
SET @index = -1 
 
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
	END
	RETURN
END
GO
 
SET QUOTED_IDENTIFIER OFF
GO 
SET ANSI_NULLS ON
GO