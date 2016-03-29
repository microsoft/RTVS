// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.PackageManager.Implementation;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Components.Test.PackageManager {
    public class RPackageWebParserTest {
        private readonly MethodInfo _testMethod;
        private readonly TestFilesFixture _testFiles;

        public RPackageWebParserTest(TestMethodFixture testMethod, TestFilesFixture testFiles) {
            _testMethod = testMethod.MethodInfo;
            _testFiles = testFiles;
        }

        [Test]
        [Category.PackageManager]
        public async void GetPackageInfo1() {
            var path = _testFiles.GetDestinationPath(Path.Combine("Parser", "ddpcr.html"));
            var actual = await RPackageWebParser.RetrievePackageInfo(new Uri(path, UriKind.Absolute));

            var expected = new RPackage() {
                Package = "ddpcr",
                Title = "Analysis and Visualization of Droplet Digital PCR in R and on the Web",
                Description = "An interface to explore, analyze, and visualize droplet digital PCR (ddPCR) data in R. This is the first non-proprietary software for analyzing two-channel ddPCR data. An interactive tool was also created and is available online to facilitate this analysis for anyone who is not comfortable with using R.",
                Version = "1.1.2",
                Author = "Dean Attali [aut, cre]",
                Depends = "R (≥ 3.1.0)",
                Published = "2016-03-17",
                License = "MIT + file LICENSE",
                Imports = "DT (≥ 0.1), dplyr (≥ 0.4.0), ggplot2 (≥ 1.0.1.9003), lazyeval (≥ 0.1.10), magrittr (≥ 1.5), mixtools (≥ 1.0.2), plyr (≥ 1.8.1), readr (≥ 0.1.0), shiny (≥ 0.11.0), shinyjs (≥ 0.4.0)",
                Suggests = "ggExtra (≥ 0.3.0), graphics, grid (≥ 3.2.2), gridExtra (≥ 2.0.0), knitr (≥ 1.7), rmarkdown, stats, testthat (≥ 0.9.1), utils",
                Maintainer = "Dean Attali  <daattali at gmail.com>",
                URL = "https://github.com/daattali/ddpcr",
                BugReports = "https://github.com/daattali/ddpcr/issues",
                NeedsCompilation = "no",
            };

            actual.ShouldBeEquivalentTo(expected);
        }

        [Test]
        [Category.PackageManager]
        public async void GetPackageInfo2() {
            var path = _testFiles.GetDestinationPath(Path.Combine("Parser", "dplyr.html"));
            var actual = await RPackageWebParser.RetrievePackageInfo(new Uri(path, UriKind.Absolute));

            var expected = new RPackage() {
                Package = "dplyr",
                Title = "A Grammar of Data Manipulation",
                Description = "A fast, consistent tool for working with data frame like objects, both in memory and out of memory.",
                Version = "0.4.3",
                Author = "Hadley Wickham [aut, cre], Romain Francois [aut], RStudio [cph]",
                Depends = "R (≥ 3.1.2)",
                Published = "2015-09-01",
                License = "MIT + file LICENSE",
                Imports = "assertthat, utils, R6, Rcpp, magrittr, lazyeval (≥ 0.1.10), DBI (≥ 0.3)",
                Suggests = "RSQLite (≥ 1.0.0), RMySQL, RPostgreSQL, data.table, testthat, knitr, microbenchmark, ggplot2, mgcv, Lahman (≥ 3.0-1), nycflights13, methods",
                Maintainer = "Hadley Wickham  <hadley at rstudio.com>",
                URL = "https://github.com/hadley/dplyr",
                BugReports = "https://github.com/hadley/dplyr/issues",
                LinkingTo = "Rcpp (≥ 0.12.0), BH (≥ 1.58.0-1)",
                NeedsCompilation = "yes",
            };

            actual.ShouldBeEquivalentTo(expected);
        }

        [Test]
        [Category.PackageManager]
        public void GetPackageInfoNotExist() {
            var path = _testFiles.GetDestinationPath(Path.Combine("Parser", "notexist.html"));

            Func<Task> f = async () => await RPackageWebParser.RetrievePackageInfo(new Uri(path, UriKind.Absolute));
            f.ShouldThrow<RPackageInfoRetrievalException>();
        }
    }
}
