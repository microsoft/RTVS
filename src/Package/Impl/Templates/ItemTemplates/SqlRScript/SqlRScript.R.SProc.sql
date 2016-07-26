-- This is a template for the stored procedure in R
--
-- _PROCEDURENAME_ will be replaced by the generated procedure name
-- _RCODE_will be replaced with the R script
-- _INPUT_QUERY_ will receive SQL query from the RScript.R.sql file
--
CREATE PROCEDURE _PROCEDURENAME_
AS
BEGIN
EXEC sp_execute_external_script @language = N'R'
    , @script = N'_RCODE_'
    , @input_data_1 = N'_INPUT_QUERY_'
--- Please edit this line and enter  code that will handle the output data frame.
    WITH RESULT SETS (([MYNEWCOLUMN] NVARCHAR(max)));
END;
