CREATE PROCEDURE script
AS
BEGIN
EXEC sp_execute_external_script @language = N'R'
    , @script = N'SELECT RCode FROM RCodeTable WHERE SProcName IS script'
    , @input_data_1 = N''
    WITH RESULT SETS (([Column] NVARCHAR(max)));
END;
