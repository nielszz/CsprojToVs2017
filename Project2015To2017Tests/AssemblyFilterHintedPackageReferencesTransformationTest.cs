using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using Project2015To2017.Definition;
using Project2015To2017.Reading;
using Project2015To2017.Transforms;

namespace Project2015To2017Tests
{
	[TestClass]
	public class AssemblyFilterHintedPackageReferencesTransformationTest
	{
		[TestMethod]
		public void HandlesNoPackagesConfig()
		{
			var project = new Project();

			var transformation = new AssemblyFilterHintedPackageReferencesTransformation();
			transformation.Transform(project);
		}

		[TestMethod]
		public void DedupeReferencesFromPackages()
		{
			var project = new Project
			{
				AssemblyReferences = new List<AssemblyReference>
				{
					new AssemblyReference {
						Include = "Newtonsoft.Json, Version=10.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed, processorArchitecture=MSIL",
						HintPath = @"..\packages\Newtonsoft.Json.10.0.2\lib\net45\Newtonsoft.Json.dll"
					},
					new AssemblyReference {

						Include = "System.Data.DataSetExtensions"
					},
					new AssemblyReference {

						Include = "Owin",
						HintPath = @"..\packages\Owin.1.0\lib\net40\Owin.dll"
					}
				},
				PackageReferences = new[]
				{
					new PackageReference {
						Id = "Newtonsoft.Json",
						Version = "1.0.0"
					}
				},
				FilePath = new FileInfo(@".\dummy.csproj")
			};

			var transformation = new AssemblyFilterHintedPackageReferencesTransformation();

			transformation.Transform(project);

			Assert.AreEqual(2, project.AssemblyReferences.Count);
		}

		[TestMethod]
		public void DedupeReferencesFromPackagesAlternativePackagesFolder()
		{
			var projFile = @"TestFiles\AltNugetConfig\ProjectFolder\net46console.testcsproj";

			var project = new ProjectReader().Read(projFile);

			var transformation = new AssemblyFilterHintedPackageReferencesTransformation();

			//Then attempt to clear any referencing the nuget packages folder
			transformation.Transform(project);

			//The only one left which points to another folder
			Assert.AreEqual(1, project.AssemblyReferences.Count);
			Assert.IsTrue(project.AssemblyReferences[0].Include.StartsWith("Owin"));
		}
	}
}
