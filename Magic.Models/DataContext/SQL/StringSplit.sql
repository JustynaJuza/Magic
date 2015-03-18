--USE MagicDB

--CREATE function [dbo].[f_split]
--(
--@param nvarchar(max), 
--@delimiter char(1)
--)
--returns @t table (val nvarchar(max), seq int)
--as
--begin
--set @param += @delimiter

--;with a as
--(
--select cast(1 as bigint) f, charindex(@delimiter, @param) t, 1 seq
--union all
--select t + 1, charindex(@delimiter, @param, t + 1), seq + 1
--from a
--where charindex(@delimiter, @param, t + 1) > 0
--)
--insert @t
--select substring(@param, f, t - f), seq from a
--option (maxrecursion 0)
--return
--end

CREATE FUNCTION dbo.SplitString
( 
    @string NVARCHAR(MAX), 
    @delimiter CHAR(1) 
) 
RETURNS @output TABLE(splitData NVARCHAR(MAX)) 
BEGIN 
    DECLARE @start INT, @end INT 
    SELECT @start = 1, @end = CHARINDEX(@delimiter, @string) 
    WHILE @start < LEN(@string) + 1 BEGIN 
        IF @end = 0  
            SET @end = LEN(@string) + 1
       
        INSERT INTO @output (splitdata)  
        VALUES(SUBSTRING(@string, @start, @end - @start)) 
        SET @start = @end + 1 
        SET @end = CHARINDEX(@delimiter, @string, @start)
        
    END 
    RETURN 
END