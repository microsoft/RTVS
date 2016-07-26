CREATE PROCEDURE script
AS
BEGIN
EXEC sp_execute_external_script
        @language = N'R'
        , @script = N'
x <- 1
'
        , @input_data_1 = N''
        , @input_data_1_name = N''
        , @output_data_1_name = N''
END;
