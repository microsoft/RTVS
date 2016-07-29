# @InputDataSet: input data frame, result of SQL query execution
# @OutputDataSet: data frame to pass back to SQL

# Test code
# library(RODBC)
# dbConnection <- 'Driver={SQL Server};Server=SERVER;Database=DATABASE;Trusted_Connection=yes'
# channel <- odbcDriverConnect(dbConnection)
# InputDataSet <- sqlQuery(channel, 'SQL QUERY')
# odbcClose(channel)

OutputDataSet <- InputDataSet
