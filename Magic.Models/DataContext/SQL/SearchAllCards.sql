USE MagicDB

IF OBJECT_ID ( 'SearchAllCards', 'P' ) IS NOT NULL 
    DROP PROCEDURE SearchAllCards
GO
CREATE PROCEDURE SearchAllCards
	@searchExpression NVARCHAR(50),	
	@selectedColumns NVARCHAR(500)
AS
	IF (@searchExpression IS NOT NULL)
	BEGIN
	DECLARE @sqlQuery VARCHAR(MAX), @columnName nvarchar(128)

	SET NOCOUNT ON
	-- CREATE TEMP TABLE
	SELECT * INTO #Search FROM Cards WHERE 1 = 2

	SET @columnName = ''
	SET @searchExpression = '%' + @searchExpression + '%'

	WHILE (@columnName IS NOT NULL)
	BEGIN
		SET @columnName = (
			SELECT MIN(sys.columns.name)
			FROM sys.columns
			INNER JOIN dbo.SplitString(@selectedColumns, ',') as split on split.value = sys.columns.name
			WHERE sys.columns.object_id = object_id('Cards')
			AND sys.columns.name > @columnName
		)
	
		IF @columnName IS NOT NULL
		BEGIN			
			SET @sqlQuery = 
				'SELECT * ' + --+ QUOTENAME(@tableName) + '.' + @columnName + 
				'FROM Cards WHERE ' + @columnName + ' LIKE ' + QUOTENAME(@searchExpression, '''')
	
			INSERT INTO #Search EXECUTE(@sqlQUery)
		END
	END

	SELECT DISTINCT * FROM #Search
END
ELSE	
	SELECT * FROM Cards
GO