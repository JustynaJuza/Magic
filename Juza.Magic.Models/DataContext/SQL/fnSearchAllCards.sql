USE MagicDB
select * from Cards
IF OBJECT_ID ( 'SearchAllCards', 'TF' ) IS NOT NULL 
    DROP PROCEDURE SearchAllCards
GO
CREATE FUNCTION SearchAllCards(
	@searchExpression NVARCHAR(50),	
	@selectedColumns NVARCHAR(500)
	)
RETURNS @Cards TABLE
(
	Id nvarchar(128),
	SetId nvarchar(128),
	Name nvarchar(MAX),
	Rarity int,
	Mana int
)
AS
BEGIN
	DECLARE @Rarity TABLE
	(
	  Id int, 
	  Name nvarchar(25)
	)
	INSERT INTO @Rarity
	VALUES (1, 'Common'), (2, 'Uncommon'), (3, 'Rare'), (4, 'Mythic Rare')

	IF (@searchExpression IS NOT NULL)
	BEGIN

	SET @searchExpression = '%' + @searchExpression + '%'

	SET NOCOUNT ON

	DECLARE @sqlQuery VARCHAR(MAX)
	
	INSERT INTO #Search
	SELECT SetId, Name, Rarity, Mana FROM Cards
	WHERE SetId LIKE @searchExpression
	OR Name LIKE @searchExpression
	OR (CASE Rarity 
		WHEN 1 THEN 'COMMON' LIKE @searchExpression 
		END)
				
	
			INSERT INTO #Search EXECUTE(@sqlQUery)
		END
	END

	SELECT DISTINCT * FROM #Search
END
ELSE	
	SELECT * FROM Cards
RETURN
GO