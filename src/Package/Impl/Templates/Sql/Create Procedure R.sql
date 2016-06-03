CREATE PROCEDURE $SchemaQualifiedObjectName$
AS
BEGIN  
 EXEC sp_execute_external_script  
      @language = N'R'  
     , @script = N'  
          # Place R code here
     '  
     , @input_data_1 = N''  
     , @input_data_1_name = N''  
     , @output_data_1_name = N''  
END;
