CREATE PROCEDURE [a b]
AS
BEGIN
EXEC sp_execute_external_script @language = N'R'
    , @script = N'_RCODE_'
    , @input_data_1 = N'_INPUT_QUERY_'
    WITH RESULT SETS (([MY] NVARCHAR(max)));
END;
