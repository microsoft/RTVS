\title{Miscellaneous Mathematical Functions}\name{MathFun}\alias{abs}\alias{sqrt}\keyword{math}\description{
  \code{abs(x)} computes the absolute value of x, \code{sqrt(x)} computes the
  (principal) square root of x, \eqn{\sqrt{x}}.% Details for complex x are below

  The naming follows the standard for computer languages such as C or Fortran.
}\usage{
abs(x)
sqrt(x)
}\arguments{
  \item{x}{a numeric or \code{\link{complex}} vector or array.}
}\details{
  These are \link{internal generic} \link{primitive} functions: methods
  can be defined for them individually or via the
  \code{\link[=S3groupGeneric]{Math}} group generic.  For complex
  arguments (and the default method), \code{z}, \code{abs(z) ==
  \link{Mod}(z)} and \code{sqrt(z) == z^0.5}.

  \code{abs(x)} returns an \code{\link{integer}} vector when \code{x} is
  \code{integer} or \code{\link{logical}}.
}\section{S4 methods}{
  Both are S4 generic and members of the
  \code{\link[=S4groupGeneric]{Math}} group generic.
}\references{
  Becker, R. A., Chambers, J. M. and Wilks, A. R. (1988)
  \emph{The New S Language}.
  Wadsworth & Brooks/Cole.
}\seealso{
  \code{\link{Arithmetic}} for simple, \code{\link{log}} for logarithmic,
  \code{\link{sin}} for trigonometric, and \code{\link{Special}} for
  special mathematical functions.

  \sQuote{\link{plotmath}} for the use of \code{sqrt} in plot annotation.
}\examples{
require(stats) # for spline
require(graphics)
xx <- -9:9
plot(xx, sqrt(abs(xx)),  col = "red")
lines(spline(xx, sqrt(abs(xx)), n=101), col = "pink")


\name{Foo}
\alias{Foo}
%- Also NEED an '\alias' for EACH other topic documented here.
\title{
%%  ~~function to do ... ~~
}
\description{
%%  ~~ A concise (1-5 lines) description of what the function does. ~~
}
\usage{
Foo(x)
}
%- maybe also 'usage' for other objects documented here.
\arguments{
  \item{x}{
%%     ~~Describe \code{x} here~~
}
}
\details{
%%  ~~ If necessary, more details than the description above ~~
}
\value{
%%  ~Describe the value returned
%%  If it is a LIST, use
%%  \item{comp1 }{Description of 'comp1'}
%%  \item{comp2 }{Description of 'comp2'}
%% ...
}
\references{
%% ~put references to the literature/web site here ~
}
\author{
%%  ~~who you are~~
}
\note{
%%  ~~further notes~~
}

%% ~Make other sections like Warning with \section{Warning }{....} ~

\seealso{
%% ~~objects to See Also as \code{\link{help}}, ~~~
}
\examples{
##---- Should be DIRECTLY executable !! ----
##-- ==>  Define data, use random,
##--	or do  help(data=index)  for the standard data sets.

## The function is currently defined as
function (x) 
{
  }
}
% Add one or more standard keywords, see file 'KEYWORDS' in the
% R documentation directory.
\keyword{ ~kwd1 }
\keyword{ ~kwd2 }% __ONLY ONE__ keyword per line
