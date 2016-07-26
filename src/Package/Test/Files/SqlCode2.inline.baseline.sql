CREATE PROCEDURE sqlcode2
AS
BEGIN
EXEC sp_execute_external_script @language = N'R'
    , @script = N'
a <- b
'
    , @input_data_1 = N'SELECT * FROM A'
    WITH RESULT SETS (([Column] NVARCHAR(max)));
END;
