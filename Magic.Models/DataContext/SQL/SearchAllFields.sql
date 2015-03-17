--USE MagicDB

----SELECT name AS object_name 
----  ,SCHEMA_NAME(schema_id) AS schema_name,
----  [type]
----  ,type_desc
----  ,create_date
----  ,modify_date
----FROM sys.objects where type_desc = 'USER_TABLE'


--IF OBJECT_ID ( 'SearchAllFields', 'P' ) IS NOT NULL 
--    DROP PROCEDURE SearchAllFields
--GO
--CREATE PROCEDURE SearchAllFields
--	@tableName NVARCHAR(128),
--	@searchExpression NVARCHAR(50)
--AS
--	DECLARE @sqlQuery VARCHAR(MAX), @columnName nvarchar(128)
--	-- CREATE TEMP TABLE
--	SET NOCOUNT ON
--	SELECT * INTO #Search FROM @tableName WHERE 1=2

--	--IF OBJECT_ID(QUOTENAME(@tableName), 'U') IS NOT NULL
--	--	SELECT name from sys.columns where object_id = object_id(QUOTENAME(@tableName)) --COLUMNPROPERTY(OBJECT_ID(QUOTENAME(@tableName), 'U'),  )

--	SET @columnName = ''

--	IF OBJECT_ID(QUOTENAME(@tableName), 'U') IS NOT NULL
--	BEGIN
--		SET @searchExpression = '%' + @searchExpression + '%'

--		WHILE (@columnName IS NOT NULL)
--		BEGIN
--			SET @columnName = (
--				SELECT MIN(sys.columns.name)
--				FROM sys.columns
--				JOIN sys.types ON sys.columns.system_type_id = sys.types.system_type_id
--				WHERE sys.columns.object_id = object_id(QUOTENAME('ChatRooms'))
--				AND sys.types.name IN ('char', 'varchar', 'nchar', 'nvarchar', 'int', 'decimal')
--				AND sys.columns.name > @columnName
--			)

--			IF @columnName IS NOT NULL
--			BEGIN			
--				SET @sqlQuery = 
--					'SELECT * ' + --+ QUOTENAME(@tableName) + '.' + @columnName + 
--					'FROM ' + QUOTENAME(@tableName) + 
--					' WHERE ' + @columnName + ' LIKE ' + QUOTENAME(@searchExpression, '''')

--				INSERT INTO #Search EXECUTE(@sqlQUery)
--			END
--		END
--	END

--	SELECT DISTINCT * FROM #Search
--	DROP TABLE #Search
--GO

USE MagicDB

	DECLARE @sqlQuery VARCHAR(MAX), @columnName nvarchar(128), @tableName NVARCHAR(128), @searchExpression NVARCHAR(50)

	SET @tableName = 'ChatRooms'
	SET @searchExpression = ''

	SET NOCOUNT ON
	-- CREATE TEMP TABLE
	SET @sqlQuery = 
		'SELECT * INTO #Search FROM ' + QUOTENAME(@tableName) + 
		' WHERE 1 = 2'
	EXECUTE(@sqlQUery)
	
	SELECT DISTINCT * FROM #Search

	SET @columnName = ''
	IF OBJECT_ID(QUOTENAME(@tableName), 'U') IS NOT NULL
	BEGIN
		SET @searchExpression = '%' + @searchExpression + '%'

		WHILE (@columnName IS NOT NULL)
		BEGIN
			SET @columnName = (
				SELECT MIN(sys.columns.name)
				FROM sys.columns
				JOIN sys.types ON sys.columns.system_type_id = sys.types.system_type_id
				WHERE sys.columns.object_id = object_id(QUOTENAME('ChatRooms'))
				AND sys.types.name IN ('char', 'varchar', 'nchar', 'nvarchar', 'int', 'decimal')
				AND sys.columns.name > @columnName
			)

			IF @columnName IS NOT NULL
			BEGIN			
				SET @sqlQuery = 
					'SELECT * ' + --+ QUOTENAME(@tableName) + '.' + @columnName + 
					'FROM ' + QUOTENAME(@tableName) + 
					' WHERE ' + @columnName + ' LIKE ' + QUOTENAME(@searchExpression, '''')

				INSERT INTO #Search EXECUTE(@sqlQUery)
			END
		END
	END

	SELECT DISTINCT * FROM #Search
	DROP TABLE #Search
GO