using System;
using System.IO;
using System.Linq;
using EnvDTE80;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools.TestUtilities.UI;
using FluentAssertions;
using System.Diagnostics;

namespace Microsoft.VSIntegrationTests {
	[TestClass]
	public class Tests {
		[TestMethod, Priority(1)]
		[HostType("VSTestHost"), TestCategory("Installed")]
		public void CreateNewScript() {
			using (var app = new VisualStudioApp()) {
				var project = app.CreateProject(
					RConstants.TemplateLanguageName, RConstants.ProjectTemplate_EmptyProject,
					System.IO.Path.GetTempPath(), "RTestProject");
				app.OpenSolutionExplorer().SelectProject(project);
				using (var newItem = NewItemDialog.FromDte(app)) {
					AutomationWrapper.Select(newItem.ProjectTypes.FindItem("R Script"));
					newItem.FileName = "my-script.r";
					newItem.OK();
				}

				var document = app.GetDocument("my-script.r");
				document.SetFocus();
				document.Type("2 -> a");
				document.Text.Should().Be("2 -> a# R Script");
			}
		}

		[TestMethod, Priority(1)]
		[HostType("VSTestHost"), TestCategory("Installed")]
		public void VerifyTemplateDirectories() {
			var languageName = RConstants.TemplateLanguageName;
			using (var app = new VisualStudioApp()) {
				var sln = (Solution2)app.Dte.Solution;
				var paths = (new[] { RConstants.ProjectTemplate_EmptyProject })
					.Select(n => sln.GetProjectTemplate(n, languageName));
				var existingPaths = paths.Where(templatePath => File.Exists(templatePath) || Directory.Exists(templatePath));
				existingPaths.Should().BeEquivalentTo(paths);
			}
		}
	}
}
