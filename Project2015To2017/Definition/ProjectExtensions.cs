using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Project2015To2017.Transforms;

namespace Project2015To2017.Definition
{
	public static class ProjectExtensions
	{
		public static IEnumerable<XElement> UnconditionalGroups(this Project project)
		{
			return project.PropertyGroups.Where(x => x.Attribute("Condition") == null);
		}

		public static IEnumerable<XElement> ConditionalGroups(this Project project)
		{
			return project.PropertyGroups.Where(x => x.Attribute("Condition") != null);
		}

		public static XElement PrimaryPropertyGroup(this Project project)
		{
			return project.UnconditionalGroups().First();
		}

		public static (IReadOnlyList<XElement> unconditional, IReadOnlyList<(string condition, XElement element)> conditional)
			PropertyAll(this Project project, string name)
		{
			var unconditional = new List<XElement>();
			var conditional = new List<(string condition, XElement element)>();
			foreach (var element in project.PropertyGroups.ElementsAnyNamespace(name))
			{
				if (!element.PropertyCondition(out var condition))
				{
					unconditional.Add(element);
					continue;
				}

				conditional.Add((condition, element));
			}

			return (unconditional, conditional);
		}

		public static XElement Property(this Project project, string name, bool tryConditional = false)
		{
			var (unconditional, conditional) = project.PropertyAll(name);
			return unconditional.LastOrDefault()
			       ?? (tryConditional ? conditional.Select(x => x.element).LastOrDefault() : null);
		}

		public static PropertyFindResult FindExistingElements(this Project project, params string[] names)
		{
			var result = new PropertyFindResult
			{
				OtherUnconditionalElements = new List<XElement>(),
				OtherConditionalElements = new List<XElement>()
			};

			foreach (var name in names)
			{
				var (unconditional, conditional) = project.PropertyAll(name);
				foreach (var child in unconditional)
				{
					Store(child, ref result.LastUnconditionalElement, result.OtherUnconditionalElements);
				}

				foreach (var (_, child) in conditional)
				{
					Store(child, ref result.LastConditionalElement, result.OtherConditionalElements);
				}
			}

			return result;

			void Store(XElement child, ref XElement lastElement, IList<XElement> others)
			{
				if (lastElement != null)
				{
					if (child.IsAfter(lastElement))
					{
						others.Add(lastElement);
						lastElement = child;
					}
					else
					{
						others.Add(child);
					}
				}
				else
				{
					lastElement = child;
				}
			}
		}

		public static IEnumerable<XElement> AllUnconditional(this PropertyFindResult self)
		{
			return self.LastUnconditionalElement != null
				? self.OtherUnconditionalElements.Concat(new[] {self.LastUnconditionalElement})
				: Array.Empty<XElement>();
		}

		public static IEnumerable<XElement> AllConditional(this PropertyFindResult self)
		{
			return self.LastConditionalElement != null
				? self.OtherConditionalElements.Concat(new[] {self.LastConditionalElement})
				: Array.Empty<XElement>();
		}

		public static IEnumerable<XElement> All(this PropertyFindResult self)
		{
			return self.LastElementIsConditional
				? self.AllConditional().Concat(self.AllUnconditional())
				: self.AllUnconditional().Concat(self.AllConditional());
		}

		public ref struct PropertyFindResult
		{
			public XElement LastUnconditionalElement;
			public XElement LastConditionalElement;

			public bool LastElementIsConditional => LastConditionalElement?.IsAfter(LastUnconditionalElement) ?? false;

			public bool LastElementIsUnconditional =>
				LastUnconditionalElement?.IsAfter(LastConditionalElement) ?? false;

			public IList<XElement> OtherUnconditionalElements;
			public IList<XElement> OtherConditionalElements;

			public bool FoundAny => LastConditionalElement != null || LastUnconditionalElement != null;

			public static implicit operator bool(PropertyFindResult self) => self.FoundAny;
		}

		public static void ReplacePropertiesWith(this Project project, XElement newElement, params string[] names)
		{
			var findResult = project.FindExistingElements(names);

			if (!findResult)
			{
				if (newElement != null)
				{
					project.PrimaryPropertyGroup().Add(newElement);
				}

				return;
			}

			XElement lastExisting = null;
			foreach (var element in findResult.All())
			{
				lastExisting?.Remove();
				lastExisting = element;
			}

			if (newElement == null)
			{
				lastExisting?.Remove();
				return;
			}

			if (lastExisting != null)
			{
				if (!lastExisting.PropertyCondition(out _))
				{
					lastExisting.ReplaceWith(newElement);
					return;
				}

				lastExisting.Remove();
			}

			project.PrimaryPropertyGroup().Add(newElement);
		}

		public static void SetProperty(this Project project, string elementName, string value)
		{
			XElement newElement = null;
			if (!string.IsNullOrWhiteSpace(value))
			{
				newElement = new XElement(elementName, value);
			}

			project.ReplacePropertiesWith(newElement, elementName);
		}

		public static DirectoryInfo TryFindBestRootDirectory(this Project project)
		{
			if (project == null) throw new ArgumentNullException(nameof(project));

			return project.Solution?.FilePath.Directory ?? project.FilePath.Directory;
		}
	}
}