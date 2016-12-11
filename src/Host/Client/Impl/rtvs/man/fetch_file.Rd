\title{Fetches file from remote machine}
\name{fetch_file}
\alias{fetch_file}
\usage{
fetch_file(remotePath, localPath = NULL, silent = FALSE)
}
\arguments{
  \item{remotePath}{Path to the file on remote machine.}
  \item{localPath}{Path to the file on local machine. If not provided, random name is generated. If directory is not provided, file is placed in the doanloads folder.}
  \item{silent}{If TRUE, method does not output local file name to the console. Default is FALSE.}
}
\description{
\code{fetch_file} is used to download file from a remote machine. If local path is not provided, the file is saved to the local downloads folder.
}
\examples{
fetch_file('data.csv')
fetch_file('data.csv', '~/data.csv')
}