CREATE PROCEDURE #DropConstraintsAffectingTable
( 
    @tableName NVARCHAR(MAX)
)
AS
BEGIN
	CREATE TABLE #Constraints
	( 
	PKTABLE_QUALIFIER sysname,
	PKTABLE_OWNER sysname,
	PKTABLE_NAME sysname,
	PKCOLUMN_NAME sysname,
	FKTABLE_QUALIFIER sysname,
	FKTABLE_OWNER sysname,
	FKTABLE_NAME sysname,
	FKCOLUMN_NAME sysname,
	KEY_SEQ smallint,
	UPDATE_RULE smallint,
	DELETE_RULE smallint,
	FK_NAME sysname,
	PK_NAME sysname,
	DEFERRABILITY smallint
	)
	INSERT #Constraints EXEC sp_fkeys @tableName

	DECLARE @referencingTableQualifier VARCHAR(MAX),
			@referencingTableOwner VARCHAR(MAX),
			@referencingTableName VARCHAR(MAX),
			@foreignKeyConstraint VARCHAR(MAX)	

	WHILE EXISTS (SELECT * FROM #Constraints)
	BEGIN
		SELECT TOP 1 
			@referencingTableQualifier = FKTABLE_QUALIFIER,
			@referencingTableOwner = FKTABLE_OWNER,
			@referencingTableName = FKTABLE_NAME, 
			@foreignKeyConstraint = FK_NAME
        FROM #Constraints
		
		EXEC
		(
			'ALTER TABLE [' + 
			@referencingTableQualifier + '].[' + @referencingTableOwner + '].[' + @referencingTableName + 
			'] DROP CONSTRAINT ' + @foreignKeyConstraint
		)

		DELETE #Constraints 
		WHERE FKTABLE_QUALIFIER = @referencingTableQualifier
		AND FKTABLE_OWNER = @referencingTableOwner
		AND FKTABLE_NAME = @referencingTableName 
		AND FK_NAME = @foreignKeyConstraint
	END
DROP TABLE #Constraints
END
GO

EXEC #DropConstraintsAffectingTable 'Ratings'
GO

DROP PROC #DropConstraintsAffectingTable
GO

DROP TABLE Ratings
GO

--EXEC sp_fkeys 'Ratings'