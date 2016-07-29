CREATE PROCEDURE ProcName
AS
BEGIN
EXEC sp_execute_external_script @language = N'R'
    , @script = N'
x <- 1
'
    , @input_data_1 = N'SELECT * FROM ABC'
    WITH RESULT SETS (([MY] NVARCHAR(max)));
END;
