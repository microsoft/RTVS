CREATE PROCEDURE script
AS
BEGIN
EXEC sp_execute_external_script @language = N'R'
    , @script = N'SELECT RCode FROM RCodeTable WHERE SProcName IS script'
    , @input_data_1 = N'SELECT * FROM ABC'
    WITH RESULT SETS (([MY] NVARCHAR(max)));
END;
