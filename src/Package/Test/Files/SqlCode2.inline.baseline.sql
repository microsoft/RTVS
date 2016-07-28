CREATE PROCEDURE [a b]
AS
BEGIN
EXEC sp_execute_external_script @language = N'R'
    , @script = N'
# comment
x <- 1
'
    , @input_data_1 = N'-- comment
SELECT * FROM ABC
-- comment'
    WITH RESULT SETS (([MY] NVARCHAR(max)));
END;
