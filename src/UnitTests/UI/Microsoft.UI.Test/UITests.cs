using System;
using System.IO;
using System.Linq;
using EnvDTE80;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools.TestUtilities.UI;

namespace Microsoft.UI.Test {
	[TestClass]
	public class UITests {
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
				Assert.AreEqual("2 -> a# R Script", document.Text);
			}
		}

		[TestMethod, Priority(1)]
		[HostType("VSTestHost"), TestCategory("Installed")]
		public void VerifyTemplateDirectories() {
			var languageName = RConstants.TemplateLanguageName;
			using (var app = new VisualStudioApp()) {
				var sln = (Solution2)app.Dte.Solution;

				foreach (var templateName in new[] {
					RConstants.ProjectTemplate_EmptyProject,
				}) {
					var templatePath = sln.GetProjectTemplate(templateName, languageName);
					Assert.IsTrue(
						File.Exists(templatePath) || Directory.Exists(templatePath),
						string.Format("Cannot find template '{0}' for language '{1}'", templateName, languageName)
					);
					Console.WriteLine("Found {0} at {1}", templateName, templatePath);
				}
			}
		}
	}
}
