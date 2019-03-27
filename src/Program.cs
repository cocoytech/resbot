using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using McMaster.Extensions.CommandLineUtils;

namespace resbot
{
    class Program
    {
		[Option(Template = "-r --resxPath", Description = "The path to the resx file.", ValueName = "/Resources/Strings.resx")]
		static string ResxPath { get; set; }

		[Option(Template = "-gr --generateResponses", Description = "Generate a Responses.cs file for use with bot template manager.")]
		static bool GenerateResponsesFile {get; set; }

		[Option(Template = "-gd --generateDesigner", Description = "Generate a Strings.Designer.cs file for use with bot template manager.")]
		static bool GenerateDesignerFile {get; set; }


		[Option(Template="-dp --dialogsPath", Description="The path to the folder containing all your dialogs.", ValueName="/Dialogs")]
		static string DialogsPath { get; set; }

		[Option(Template="-n --namespace", Description="The root namespace for the bot.", ValueName="MyCompany.Bot")]
		static string Namespace { get; set; }

		[Option(Template="-dn --dialogName", Description="The name of the dialog.", ValueName="MakeReservation")]
		static string DialogName { get; set; }

		public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

		void OnExecute()
		{
			try
			{
				GenerateResources();
			}
			catch (ArgumentNullException e)
			{
				Console.WriteLine($"Please supply a value for {e.ParamName}");
			}
			catch (Exception e)
			{
				Console.WriteLine($"Something bad happened.\n\n{e.GetBaseException().Message}");
			}
		}

		static void GenerateResources()
		{
			var resourcePath = ResxPath ?? Path.Combine(DialogsPath, $"{DialogName}/Resources/{DialogName}Strings.resx");
			var resources = GetResources(resourcePath);

			if(!string.IsNullOrEmpty(ResxPath))
			{
				if(!File.Exists(ResxPath))
					throw new FileNotFoundException(ResxPath);

				var designerData = GenerateDesigner(resources);
				var filePath = Path.Combine(Path.GetDirectoryName(ResxPath), $"{Path.GetFileNameWithoutExtension(ResxPath)}.Designer.cs");
				File.WriteAllText(filePath, designerData);
				Console.WriteLine($"File created: {filePath}");
				Console.WriteLine(designerData);

				return;
			}

			if (string.IsNullOrWhiteSpace(DialogsPath))
				throw new ArgumentNullException(DialogsPath);

			if (string.IsNullOrWhiteSpace(Namespace))
				throw new ArgumentNullException(Namespace);

			if (string.IsNullOrWhiteSpace(DialogName))
				throw new ArgumentNullException(DialogName);

			if(GenerateDesignerFile)
			{
				Console.WriteLine("\r\n");
				var designerData = GenerateDesigner(resources);
				var filePath = Path.Combine(DialogsPath, $"{DialogName}/Resources/{DialogName}Strings.Designer.cs");
				File.WriteAllText(filePath, designerData);
				Console.WriteLine($"File created: {filePath}");
				Console.WriteLine(designerData);
			}

			if(GenerateResponsesFile)
			{
				Console.WriteLine("\r\n");
				var responsesData = GenerateBotResponses(resources);
				var filePath = Path.Combine(DialogsPath, $"{DialogName}/{DialogName}Responses.cs");
				File.WriteAllText(filePath, responsesData);
				Console.WriteLine($"File created: {filePath}");
				Console.WriteLine(responsesData);
			}

			Console.WriteLine("\r\nComplete");
		}

		static string GenerateDesigner(Dictionary<string, string> resources)
		{
			Console.WriteLine($"Generating designer backing file");

			var designerTemplateData = ReadFromEmbeddedResource("Templates.ResourceDesigner");
			var resourcePropertyTemplateData = ReadFromEmbeddedResource("Templates.ResourceDesignerProperty");

			var properties = new StringBuilder();
			foreach(var resource in resources)
			{
				var prop = resourcePropertyTemplateData.Replace("{key}", resource.Key).Replace("{value}", resource.Value);
				properties.AppendLine(prop);
				properties.AppendLine();
			}

			var output = designerTemplateData.Replace("{namespace}", $"{Namespace}.Dialogs.{DialogName}");
			output = output.Replace("{class}", DialogName);
			output = output.Replace("{properties}", properties.ToString().Trim());

			return output;
		}

		static string GenerateBotResponses(Dictionary<string, string> resources, bool generateTemplate = true)
		{
			Console.WriteLine($"Generating bot responses file");

			var responsesTemplateData = ReadFromEmbeddedResource("Templates.BotResponses");
			var responseLanguageTemplateData = ReadFromEmbeddedResource("Templates.BotResponsesLanguageTemplate");

			var responseIds = new StringBuilder();
			var languageTemplates = new StringBuilder();
			foreach (var resource in resources)
			{
				var isPrompt = resource.Key.Contains("prompt", StringComparison.InvariantCultureIgnoreCase);
				var keyProper = ToProperCase(resource.Key);
				var inputType = isPrompt ? "ExpectingInput" : "AcceptingInput";

				var lt = responseLanguageTemplateData.Replace("{key}", resource.Key).Replace("{value}", resource.Value);
				lt = lt.Replace("{keyProper}", keyProper).Replace("{inputType}", inputType);
				lt = lt.Replace("{class}", $"{DialogName}Strings");
				languageTemplates.AppendLine($"{lt},");

				var prop = $"\t\t\tpublic const string {keyProper} = nameof({keyProper});";
				responseIds.AppendLine(prop);
			}

			var output = responsesTemplateData.Replace("{namespace}", $"{Namespace}.Dialogs.{DialogName}");
			output = output.Replace("{class}", DialogName);
			output = output.Replace("{responseIds}", responseIds.ToString().Trim());
			output = output.Replace("{languageTemplates}", languageTemplates.ToString().Trim().TrimEnd(','));

			return output;
		}

		#region Helpers

		static Dictionary<string, string> GetResources(string resourcePath)
		{
			if (string.IsNullOrEmpty(resourcePath))
				throw new ArgumentNullException(nameof(resourcePath));

			Console.WriteLine($"Reading resx file at located at {resourcePath}");

			var doc = new XmlDocument();
			doc.Load(resourcePath);

			var resources = new Dictionary<string, string>();
			var nodes = doc.DocumentElement.SelectNodes("data");

			foreach (var node in nodes)
			{
				var xmlNode = node as XmlNode;
				resources.Add(xmlNode.Attributes["name"].Value, xmlNode.ChildNodes.Cast<XmlNode>().Single(n => n.Name == "value").InnerXml);
			}

			return resources;
		}


		static string ReadFromEmbeddedResource(string resourcePath)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		static string ToProperCase(string input)
		{
			var output = new List<char>();
			var capNext = false;

			for(int i = 0; i < input.Length; i++)
			{
				var c = input[i];

				if(i == 0)
				{
					capNext = true;
				}
				else if(c == '_')
				{
					capNext = true;
					continue;
				}

				if(capNext)
				{
					output.Add(char.ToUpper(c));
					capNext = false;
				}
				else
				{
					output.Add(char.ToLower(c));
				}
			}

			return new string(output.ToArray());
		}

		#endregion
    }
}
