IF OBJECT_ID ( 'SplitString', 'TF' ) IS NOT NULL 
    DROP FUNCTION SplitString
GO

CREATE FUNCTION SplitString
( 
    @string NVARCHAR(MAX), 
    @delimiter CHAR(1) 
) 
RETURNS @Output TABLE(value NVARCHAR(MAX)) 
BEGIN 
    DECLARE @start INT, @end INT

    SELECT @start = 1, @end = CHARINDEX(@delimiter, @string) 
    WHILE @start < LEN(@string) + 1 BEGIN 
        IF @end = 0  
            SET @end = LEN(@string) + 1
       
        INSERT INTO @Output (value)  
        VALUES(SUBSTRING(@string, @start, @end - @start)) 
        SET @start = @end + 1
        SET @end = CHARINDEX(@delimiter, @string, @start)
        
    END 
    RETURN 
END