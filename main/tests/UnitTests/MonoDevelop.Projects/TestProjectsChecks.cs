// TestProjectsChecks.cs
//
// Author:
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (c) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnitTests;
using MonoDevelop.Projects.Serialization;
using MonoDevelop.Core;
using CSharpBinding;

namespace MonoDevelop.Projects
{
	public class TestProjectsChecks
	{
		public static void CheckBasicMdConsoleProject (Solution sol)
		{
			// Check projects
			
			Assert.AreEqual (1, sol.Items.Count);
			Assert.IsTrue (sol.Items [0] is DotNetProject, "Project is DotNetProject");
			DotNetProject project = (DotNetProject) sol.Items [0];
			Assert.AreEqual ("csharp-console-mdp", project.Name);
			
			// Check files
			
			Assert.AreEqual (2, project.Files.Count, "File count");
			
			ProjectFile file = project.GetProjectFile (Path.Combine (project.BaseDirectory, "Main.cs"));
			Assert.AreEqual ("Main.cs", Path.GetFileName (file.Name));
			Assert.AreEqual (BuildAction.Compile, file.BuildAction);
			
			file = project.GetProjectFile (Path.Combine (project.BaseDirectory, "AssemblyInfo.cs"));
			Assert.AreEqual ("AssemblyInfo.cs", Path.GetFileName (file.Name));
			Assert.AreEqual (BuildAction.Compile, file.BuildAction);
			
			// References
			
			Assert.AreEqual (1, project.References.Count);
			ProjectReference pr = project.References [0];
			Assert.AreEqual (ReferenceType.Gac, pr.ReferenceType);
			Assert.AreEqual ("System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", pr.Reference);
			
			// Configurations
			
			Assert.AreEqual (2, sol.Configurations.Count, "Configuration count");
			
			SolutionConfiguration sc = sol.Configurations [0];
			Assert.AreEqual ("Debug", sc.Id);
			SolutionConfigurationEntry sce = sc.GetEntryForItem (project);
			Assert.IsTrue (sce.Build);
			Assert.AreEqual (project, sce.Item);
			Assert.AreEqual ("Debug", sce.ItemConfiguration);
			
			sc = sol.Configurations [1];
			Assert.AreEqual ("Release", sc.Id);
			sce = sc.GetEntryForItem (project);
			Assert.IsTrue (sce.Build);
			Assert.AreEqual (project, sce.Item);
			Assert.AreEqual ("Release2", sce.ItemConfiguration);
		}

		public static void CheckBasicVsConsoleProject (Solution sol)
		{
			// Check projects
			
			Assert.AreEqual (1, sol.Items.Count);
			DotNetProject project = (DotNetProject) sol.Items [0];
			
			// Check files
			
			Assert.AreEqual (2, project.Files.Count);
			
			ProjectFile file = project.GetProjectFile (Path.Combine (project.BaseDirectory, "Program.cs"));
			Assert.AreEqual ("Program.cs", Path.GetFileName (file.Name));
			Assert.AreEqual (BuildAction.Compile, file.BuildAction);
			
			file = project.GetProjectFile (Path.Combine (project.BaseDirectory, Path.Combine ("Properties", "AssemblyInfo.cs")));
			Assert.AreEqual ("AssemblyInfo.cs", Path.GetFileName (file.Name));
			Assert.AreEqual (BuildAction.Compile, file.BuildAction);
			
			// References
			
			Assert.AreEqual (3, project.References.Count);
			
			ProjectReference pr = project.References [0];
			Assert.AreEqual (ReferenceType.Gac, pr.ReferenceType);
			Assert.AreEqual ("System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", pr.Reference);
			
			pr = project.References [1];
			Assert.AreEqual (ReferenceType.Gac, pr.ReferenceType);
			Assert.AreEqual ("System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", pr.Reference);
			
			pr = project.References [2];
			Assert.AreEqual (ReferenceType.Gac, pr.ReferenceType);
			Assert.AreEqual ("System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", pr.Reference);
			
			// Configurations
			
			Assert.AreEqual (2, sol.Configurations.Count, "Configuration count");
			
			SolutionConfiguration sc = sol.Configurations [0];
			Assert.AreEqual ("Debug", sc.Name);
			SolutionConfigurationEntry sce = sc.GetEntryForItem (project);
			Assert.IsTrue (sce.Build);
			Assert.AreEqual (project, sce.Item);
			Assert.AreEqual ("Debug", sce.ItemConfiguration);
			
			sc = sol.Configurations [1];
			Assert.AreEqual ("Release", sc.Name);
			sce = sc.GetEntryForItem (project);
			Assert.IsTrue (sce.Build);
			Assert.AreEqual (project, sce.Item);
			Assert.AreEqual ("Release", sce.ItemConfiguration);
		}
		
		public static Solution CreateConsoleSolution (string hint)
		{
			string dir = Util.CreateTmpDir (hint);
			
			Solution sol = new Solution ();
			SolutionConfiguration scDebug = sol.AddConfiguration ("Debug", true);
			
			DotNetProject project = new DotNetProject ("C#");
			sol.RootFolder.Items.Add (project);
			
			InitializeProject (dir, project, "TestProject");
			project.Files.Add (new ProjectFile (Path.Combine (dir, "Main.cs")));
			project.Files.Add (new ProjectFile (Path.Combine (dir, "Resource.xml"), BuildAction.EmbedAsResource));
			project.Files.Add (new ProjectFile (Path.Combine (dir, "Excluded.xml"), BuildAction.Exclude));
			project.Files.Add (new ProjectFile (Path.Combine (dir, "Copy.xml"), BuildAction.FileCopy));
			project.Files.Add (new ProjectFile (Path.Combine (dir, "Nothing.xml"), BuildAction.Nothing));
			project.References.Add (new ProjectReference (ReferenceType.Gac, "System"));
			project.References.Add (new ProjectReference (ReferenceType.Gac, "System.Data"));
			project.References.Add (new ProjectReference (ReferenceType.Gac, "System.Xml"));
			
			SolutionConfiguration scRelease = sol.AddConfiguration ("Release", true);
			
			string file = Path.Combine (dir, "TestSolution.sln");
			sol.FileName = file;
			
			Assert.AreEqual (2, sol.Configurations.Count);
			Assert.AreEqual (1, scDebug.Configurations.Count);
			Assert.AreEqual (1, scRelease.Configurations.Count);
			Assert.AreEqual (2, project.Configurations.Count);
			
			return sol;
		}
		
		public static void CheckConsoleProject (Solution sol)
		{
			Assert.AreEqual (1, sol.Items.Count);
			Assert.AreEqual (2, sol.Configurations.Count);
			Assert.AreEqual ("Debug", sol.Configurations [0].Id);
			Assert.AreEqual ("Release", sol.Configurations [1].Id);
			
			DotNetProject p = (DotNetProject) sol.Items [0];
			Assert.AreEqual (5, p.Files.Count);
			Assert.IsTrue (p.Files.GetFile (Path.Combine (p.BaseDirectory, "Main.cs")) != null);
			Assert.IsTrue (p.Files.GetFile (Path.Combine (p.BaseDirectory, "Resource.xml")) != null);
			Assert.IsTrue (p.Files.GetFile (Path.Combine (p.BaseDirectory, "Excluded.xml")) != null);
			Assert.IsTrue (p.Files.GetFile (Path.Combine (p.BaseDirectory, "Copy.xml")) != null);
			Assert.IsTrue (p.Files.GetFile (Path.Combine (p.BaseDirectory, "Nothing.xml")) != null);
			
			Assert.AreEqual (3, p.References.Count);
		}
		
		public static DotNetProject CreateProject (string dir, string lang, string name)
		{
			DotNetProject project = new DotNetProject (lang);
			InitializeProject (dir, project, name);
			return project;
		}
		
		public static void InitializeProject (string dir, DotNetProject project, string name)
		{
			project.DefaultNamespace = name;
			
			DotNetProjectConfiguration pcDebug = project.AddNewConfiguration ("Debug") as DotNetProjectConfiguration;
			CSharpCompilerParameters csparamsDebug = (CSharpCompilerParameters) pcDebug.CompilationParameters;
			pcDebug.OutputDirectory = Path.Combine (dir, "bin/Debug");
			pcDebug.OutputAssembly = name;
			pcDebug.DebugMode = true;
			csparamsDebug.DefineSymbols = "DEBUG;TRACE";
			csparamsDebug.Optimize = false;
			
			DotNetProjectConfiguration pcRelease = project.AddNewConfiguration ("Release") as DotNetProjectConfiguration;
			CSharpCompilerParameters csparamsRelease = (CSharpCompilerParameters) pcRelease.CompilationParameters;
			pcRelease.OutputDirectory = Path.Combine (dir, "bin/Release");
			pcRelease.OutputAssembly = name;
			csparamsRelease.DefineSymbols = "TRACE";
			csparamsRelease.Optimize = true;
			
			string pfile = Path.Combine (dir, name);
			project.FileName = pfile;
		}
		
		public static Solution CreateProjectWithFolders (string hint)
		{
			string dir = Util.CreateTmpDir (hint);
			Directory.CreateDirectory (Util.Combine (dir, "console-project"));
			Directory.CreateDirectory (Util.Combine (dir, "nested-solution1"));
			Directory.CreateDirectory (Util.Combine (dir, "nested-solution1", "library1"));
			Directory.CreateDirectory (Util.Combine (dir, "nested-solution1", "library2"));
			Directory.CreateDirectory (Util.Combine (dir, "nested-solution2"));
			Directory.CreateDirectory (Util.Combine (dir, "nested-solution2", "console-project2"));
			Directory.CreateDirectory (Util.Combine (dir, "nested-solution2", "nested-solution3"));
			Directory.CreateDirectory (Util.Combine (dir, "nested-solution2", "nested-solution3", "library3"));
			Directory.CreateDirectory (Util.Combine (dir, "nested-solution2", "nested-solution3", "library4"));
			
			Solution sol = new Solution ();
			sol.FileName = Path.Combine (dir, "nested-solutions-mdp");
			SolutionConfiguration scDebug = sol.AddConfiguration ("Debug", true);
			SolutionConfiguration scRelease = sol.AddConfiguration ("Release", true);
			
			DotNetProject project1 = CreateProject (Util.Combine (dir, "console-project"), "C#", "console-project");
			project1.Files.Add (new ProjectFile (Path.Combine (project1.BaseDirectory, "Main.cs")));
			project1.Files.Add (new ProjectFile (Path.Combine (project1.BaseDirectory, "AssemblyInfo.cs")));
			sol.RootFolder.Items.Add (project1);
			
			// nested-solution1
			
			SolutionFolder folder1 = new SolutionFolder ();
			sol.RootFolder.Items.Add (folder1);
			folder1.Name = "nested-solution1";
			
			DotNetProject projectLib1 = CreateProject (Util.Combine (dir, "nested-solution1", "library1"), "C#", "library1");
			projectLib1.References.Add (new ProjectReference (ReferenceType.Gac, "System"));
			projectLib1.Files.Add (new ProjectFile (Path.Combine (projectLib1.BaseDirectory, "MyClass.cs")));
			projectLib1.Files.Add (new ProjectFile (Path.Combine (projectLib1.BaseDirectory, "AssemblyInfo.cs")));
			projectLib1.CompileTarget = CompileTarget.Library;
			folder1.Items.Add (projectLib1);
			
			DotNetProject projectLib2 = CreateProject (Util.Combine (dir, "nested-solution1", "library2"), "C#", "library2");
			projectLib2.References.Add (new ProjectReference (ReferenceType.Gac, "System"));
			projectLib2.Files.Add (new ProjectFile (Path.Combine (projectLib2.BaseDirectory, "MyClass.cs")));
			projectLib2.Files.Add (new ProjectFile (Path.Combine (projectLib2.BaseDirectory, "AssemblyInfo.cs")));
			projectLib2.CompileTarget = CompileTarget.Library;
			folder1.Items.Add (projectLib2);
			
			// nested-solution2

			SolutionFolder folder2 = new SolutionFolder ();
			folder2.Name = "nested-solution2";
			sol.RootFolder.Items.Add (folder2);
			
			DotNetProject project2 = CreateProject (Util.Combine (dir, "nested-solution2", "console-project2"), "C#", "console-project2");
			project2.Files.Add (new ProjectFile (Path.Combine (project2.BaseDirectory, "Main.cs")));
			project2.Files.Add (new ProjectFile (Path.Combine (project2.BaseDirectory, "AssemblyInfo.cs")));
			project2.References.Add (new ProjectReference (ReferenceType.Gac, "System"));
			
			// nested-solution3

			SolutionFolder folder3 = new SolutionFolder ();
			folder3.Name = "nested-solution3";
			
			DotNetProject projectLib3 = CreateProject (Util.Combine (dir, "nested-solution2", "nested-solution3", "library3"), "C#", "library3");
			projectLib3.References.Add (new ProjectReference (ReferenceType.Gac, "System"));
			projectLib3.Files.Add (new ProjectFile (Path.Combine (projectLib3.BaseDirectory, "MyClass.cs")));
			projectLib3.Files.Add (new ProjectFile (Path.Combine (projectLib3.BaseDirectory, "AssemblyInfo.cs")));
			projectLib3.CompileTarget = CompileTarget.Library;
			folder3.Items.Add (projectLib3);
			
			DotNetProject projectLib4 = CreateProject (Util.Combine (dir, "nested-solution2", "nested-solution3", "library4"), "C#", "library4");
			projectLib4.References.Add (new ProjectReference (ReferenceType.Gac, "System"));
			projectLib4.Files.Add (new ProjectFile (Path.Combine (projectLib4.BaseDirectory, "MyClass.cs")));
			projectLib4.Files.Add (new ProjectFile (Path.Combine (projectLib4.BaseDirectory, "AssemblyInfo.cs")));
			projectLib4.CompileTarget = CompileTarget.Library;
			folder3.Items.Add (projectLib4);
			
			folder2.Items.Add (folder3);
			folder2.Items.Add (project2);
			
			string file = Path.Combine (dir, "TestSolution.sln");
			sol.FileName = file;
			
			project1.References.Add (new ProjectReference (ReferenceType.Gac, "System"));
			project1.References.Add (new ProjectReference (projectLib1));
			project1.References.Add (new ProjectReference (projectLib2));
			project1.References.Add (new ProjectReference (projectLib3));
			project1.References.Add (new ProjectReference (projectLib4));
			
			project2.References.Add (new ProjectReference (projectLib3));
			project2.References.Add (new ProjectReference (projectLib4));
			
			Assert.AreEqual (2, sol.Configurations.Count);
			Assert.AreEqual (6, scDebug.Configurations.Count);
			Assert.AreEqual (6, scRelease.Configurations.Count);
			
			return sol;
		}
		
		public static void CheckGenericItemProject (string fileFormat)
		{
			Solution sol = new Solution ();
			sol.FileFormat = Services.ProjectService.FileFormats.GetFileFormat (fileFormat);
			string dir = Util.CreateTmpDir ("generic-item-" + fileFormat);
			sol.FileName = Path.Combine (dir, "TestGenericItem");
			sol.Name = "TheItem";
			
			GenericItem it = new GenericItem ();
			it.SomeValue = "hi";
			
			sol.RootFolder.Items.Add (it);
			it.FileName = Path.Combine (dir, "TheItem");
			it.Name = "TheItem";
			
			sol.Save (Util.GetMonitor ());
			
			Solution sol2 = (Solution) Services.ProjectService.ReadWorkspaceItem (Util.GetMonitor (), sol.FileName);
			Assert.AreEqual (1, sol2.Items.Count);
			Assert.IsTrue (sol2.Items [0] is GenericItem);
			
			it = (GenericItem) sol2.Items [0];
			Assert.AreEqual ("hi", it.SomeValue);
		}
		
		public static void TestLoadSaveSolutionFolders (string fileFormat)
		{
			List<string> ids = new List<string> ();
			
			Solution sol = new Solution ();
			sol.FileFormat = Services.ProjectService.FileFormats.GetFileFormat (fileFormat);
			string dir = Util.CreateTmpDir ("solution-folders-" + fileFormat);
			sol.FileName = Path.Combine (dir, "TestSolutionFolders");
			sol.Name = "TheSolution";
			
			DotNetProject p1 = new DotNetProject ("C#");
			p1.FileName = Path.Combine (dir, "p1");
			sol.RootFolder.Items.Add (p1);
			string idp1 = p1.ItemId;
			Assert.IsFalse (string.IsNullOrEmpty (idp1));
			Assert.IsFalse (ids.Contains (idp1));
			ids.Add (idp1);

			SolutionFolder f1 = new SolutionFolder ();
			f1.Name = "f1";
			sol.RootFolder.Items.Add (f1);
			string idf1 = f1.ItemId;
			Assert.IsFalse (string.IsNullOrEmpty (idf1));
			Assert.IsFalse (ids.Contains (idf1));
			ids.Add (idf1);
			
			DotNetProject p2 = new DotNetProject ("C#");
			p2.FileName = Path.Combine (dir, "p2");
			f1.Items.Add (p2);
			string idp2 = p2.ItemId;
			Assert.IsFalse (string.IsNullOrEmpty (idp2));
			Assert.IsFalse (ids.Contains (idp2));
			ids.Add (idp2);

			SolutionFolder f2 = new SolutionFolder ();
			f2.Name = "f2";
			f1.Items.Add (f2);
			string idf2 = f2.ItemId;
			Assert.IsFalse (string.IsNullOrEmpty (idf2));
			Assert.IsFalse (ids.Contains (idf2));
			ids.Add (idf2);
			
			DotNetProject p3 = new DotNetProject ("C#");
			p3.FileName = Path.Combine (dir, "p3");
			f2.Items.Add (p3);
			string idp3 = p3.ItemId;
			Assert.IsFalse (string.IsNullOrEmpty (idp3));
			Assert.IsFalse (ids.Contains (idp3));
			ids.Add (idp3);
			
			DotNetProject p4 = new DotNetProject ("C#");
			p4.FileName = Path.Combine (dir, "p4");
			f2.Items.Add (p4);
			string idp4 = p4.ItemId;
			Assert.IsFalse (string.IsNullOrEmpty (idp4));
			Assert.IsFalse (ids.Contains (idp4));
			ids.Add (idp4);
			
			sol.Save (Util.GetMonitor ());
			
			Solution sol2 = (Solution) Services.ProjectService.ReadWorkspaceItem (Util.GetMonitor (), sol.FileName);
			Assert.AreEqual (4, sol2.Items.Count);
			Assert.AreEqual (2, sol2.RootFolder.Items.Count);
			Assert.AreEqual (typeof(DotNetProject), sol2.RootFolder.Items [0].GetType ());
			Assert.AreEqual (typeof(SolutionFolder), sol2.RootFolder.Items [1].GetType ());
			Assert.AreEqual ("p1", sol2.RootFolder.Items [0].Name);
			Assert.AreEqual ("f1", sol2.RootFolder.Items [1].Name);
			Assert.AreEqual (idp1, sol2.RootFolder.Items [0].ItemId, "idp1");
			Assert.AreEqual (idf1, sol2.RootFolder.Items [1].ItemId, "idf1");
			
			f1 = (SolutionFolder) sol2.RootFolder.Items [1];
			Assert.AreEqual (2, f1.Items.Count);
			Assert.AreEqual (typeof(DotNetProject), f1.Items [0].GetType ());
			Assert.AreEqual (typeof(SolutionFolder), f1.Items [1].GetType ());
			Assert.AreEqual ("p2", f1.Items [0].Name);
			Assert.AreEqual ("f2", f1.Items [1].Name);
			Assert.AreEqual (idp2, f1.Items [0].ItemId, "idp2");
			Assert.AreEqual (idf2, f1.Items [1].ItemId, "idf2");
			
			f2 = (SolutionFolder) f1.Items [1];
			Assert.AreEqual (2, f2.Items.Count);
			Assert.AreEqual (typeof(DotNetProject), f2.Items [0].GetType ());
			Assert.AreEqual (typeof(DotNetProject), f2.Items [1].GetType ());
			Assert.AreEqual ("p3", f2.Items [0].Name);
			Assert.AreEqual ("p4", f2.Items [1].Name);
			Assert.AreEqual (idp3, f2.Items [0].ItemId, "idp4");
			Assert.AreEqual (idp4, f2.Items [1].ItemId, "idp4");
		}
		
		public static void TestCreateLoadSaveConsoleProject (string fileFormat)
		{
			Solution sol = CreateConsoleSolution ("TestCreateLoadSaveConsoleProject");
			sol.FileFormat = Services.ProjectService.FileFormats.GetFileFormat (fileFormat);
			
			sol.Save (Util.GetMonitor ());
			string solFile = sol.FileName;
			
			sol = (Solution) Services.ProjectService.ReadWorkspaceItem (Util.GetMonitor (), solFile);
			CheckConsoleProject (sol);

			// Save over existing file
			sol.Save (Util.GetMonitor ());
			sol = (Solution) Services.ProjectService.ReadWorkspaceItem (Util.GetMonitor (), solFile);
			CheckConsoleProject (sol);
		}
		
		public static void TestLoadSaveResources (string fileFormat)
		{
			string solFile = Util.GetSampleProject ("resources-tester", "ResourcesTester.sln");
			Solution sol = (Solution) Services.ProjectService.ReadWorkspaceItem (Util.GetMonitor (), solFile);
			sol.FileFormat = Services.ProjectService.FileFormats.GetFileFormat (fileFormat);
			ProjectTests.CheckResourcesSolution (sol);
			
			sol.Save (Util.GetMonitor ());
			solFile = sol.FileName;
			
			sol = (Solution) Services.ProjectService.ReadWorkspaceItem (Util.GetMonitor (), solFile);
			ProjectTests.CheckResourcesSolution (sol);
			
			DotNetProject p = (DotNetProject) sol.Items [0];
			string f = Path.Combine (p.BaseDirectory, "Bitmap1.bmp");
			ProjectFile pf = p.Files.GetFile (f);
			pf.ResourceId = "SomeBitmap.bmp";
			sol.Save (Util.GetMonitor ());
			
			sol = (Solution) Services.ProjectService.ReadWorkspaceItem (Util.GetMonitor (), solFile);
			p = (DotNetProject) sol.Items [0];
			f = Path.Combine (p.BaseDirectory, "Bitmap1.bmp");
			pf = p.Files.GetFile (f);
			Assert.AreEqual ("SomeBitmap.bmp", pf.ResourceId);
		}
	}
	
	public class GenericItem: SolutionEntityItem
	{
		[ItemProperty]
		public string SomeValue;
		
		protected override void OnClean (IProgressMonitor monitor, string configuration)
		{
		}
		
		protected override ICompilerResult OnBuild (IProgressMonitor monitor, string configuration)
		{
			return null;
		}
		
		protected override void OnExecute (IProgressMonitor monitor, ExecutionContext context, string configuration)
		{
		}
		
		protected override bool OnGetNeedsBuilding (string configuration)
		{
			return false;
		}
		
		protected override void OnSetNeedsBuilding (bool val, string configuration)
		{
		}
		
	}
}
