\name{fetch_file}
\alias{fetch_file}
\title{Fetch file from remote}
\usage{
fetch_file(path)
}
\arguments{
  \item{path}{Path to the file on remote machine}
}
\description{
Fetches file from a remote machine and brings it down to the local file system. The file is saved to the local project folder.
}
\examples{
fetch_file('data.csv')
}