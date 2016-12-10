\title{Fetches file from remote machine}
\name{fetch_file}
\alias{fetch_file}
\usage{
fetch_file(path)
}
\arguments{
  \item{path}{Path to the file on remote machine}
}
\description{
\code{fetch_file} is used to download file from a remote machine. The file is saved to the local downloads folder.
}
\examples{
fetch_file('data.csv')
}