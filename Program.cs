﻿using Microsoft.Extensions.Configuration;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace TfsSdkConsoleApp
{
    internal class Program
    {
        #region Inner Types

        public class Step
        {
            public string Action { get; set; }
            public string ExpectedResult { get; set; }
        }

        public class TestCase
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public List<Step> Steps { get; set; }
        }

        public class TestCaseSource
        {
            public string Folder { get; set; }

            public int[] WorkItems { get; set; }
        }

        #endregion Inner Types

        static void Main(string[] args)
        {
            // Carregar configurações do appsettings.json
            var config = LoadConfiguration();

            var rootDirectory = config["OutputDirectory"];

            Uri tfsUri = new Uri(config["TFSUri"]);
            TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(tfsUri);
            tpc.EnsureAuthenticated();

            WorkItemStore workItemStore = tpc.GetService<WorkItemStore>();

            if (!Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }

            var sources = LoadTestSources();

            foreach (var source in sources)
            {
                var allTestCases = new List<TestCase>();

                var testCaseIds = source.WorkItems;

                var outputDirectory = rootDirectory + "/" + source.Folder;

                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                foreach (int testCaseId in testCaseIds)
                {
                    try
                    {
                        var workItem = workItemStore.GetWorkItem(testCaseId);

                        if (workItem.Type.Name == "Test Case")
                        {
                            var title = workItem.Title;
                            var stepsXml = workItem.Fields["Microsoft.VSTS.TCM.Steps"].Value as string;
                            var testCaseSteps = ConvertStepsXmlToList(stepsXml, workItemStore);

                            var testCaseData = new TestCase
                            {
                                Id = workItem.Id,
                                Title = title,
                                Steps = testCaseSteps
                            };

                            allTestCases.Add(testCaseData);
                        }
                        else
                        {
                            Console.WriteLine($"O Work Item {testCaseId} não é um Test Case.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro Work Item {testCaseId}: {ex.Message}");
                    }
                }

                var jsonContent = JsonConvert.SerializeObject(allTestCases, Formatting.Indented);
                var filePath = Path.Combine(outputDirectory, "testcases.json");

                File.WriteAllText(filePath, jsonContent);

                Console.WriteLine($"Arquivo JSON com todos os Test Cases salvo com sucesso: {filePath}");
            }

            Console.ReadLine();
        }

        private static IConfiguration LoadConfiguration()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            return configBuilder.Build();
        }

        private static List<TestCaseSource> LoadTestSources()
        {
            var sources = new List<TestCaseSource>();

            var filePath = "test-cases-source.json";
            var jsonContent = File.ReadAllText(filePath);

            sources = JsonConvert.DeserializeObject<List<TestCaseSource>>(jsonContent);

            return sources;
        }

        private static int[] LoadTestCaseIdsFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Arquivo {filePath} não encontrado!");
                return Array.Empty<int>();
            }

            var lines = File.ReadAllLines(filePath);
            return Array.ConvertAll(lines, int.Parse);
        }

        private static List<Step> ConvertStepsXmlToList(string stepsXml, WorkItemStore workItemStore)
        {
            var stepsList = new List<Step>();

            if (string.IsNullOrEmpty(stepsXml))
                return stepsList;

            XElement stepsElement = XElement.Parse(stepsXml);

            foreach (var node in stepsElement.Elements())
            {
                if (node.Name == "step")
                {
                    ProcessStepNode(node, stepsList);
                }
                else if (node.Name == "compref")
                {
                    ProcessComprefNode(node, workItemStore, stepsList);
                }
            }

            return stepsList;
        }

        private static void ProcessStepNode(XElement stepNode, List<Step> stepsList)
        {
            var action = stepNode.Elements("parameterizedString").FirstOrDefault()?.Value ?? "Sem ação definida";
            var expectedResult = stepNode.Elements("parameterizedString").Skip(1).FirstOrDefault()?.Value ?? "";

            action = System.Net.WebUtility.HtmlDecode(action).Trim();
            expectedResult = System.Net.WebUtility.HtmlDecode(expectedResult).Trim();

            stepsList.Add(new Step
            {
                Action = RemoveHtmlTags(action),
                ExpectedResult = RemoveHtmlTags(expectedResult)
            });
        }

        private static void ProcessComprefNode(XElement comprefNode, WorkItemStore workItemStore, List<Step> stepsList)
        {
            var sharedStepRefId = comprefNode.Attribute("ref")?.Value;

            if (!string.IsNullOrEmpty(sharedStepRefId))
            {
                var sharedStepId = int.Parse(sharedStepRefId);
                var sharedStepWorkItem = workItemStore.GetWorkItem(sharedStepId);
                var stepsXml = sharedStepWorkItem.Fields["Microsoft.VSTS.TCM.Steps"].Value as string;

                if (sharedStepWorkItem.Type.Name == "Shared Steps")
                {
                    string sharedStepTitle = sharedStepWorkItem.Title;

                    stepsList.Add(new Step
                    {
                        Action = RemoveHtmlTags(sharedStepTitle)
                    });

                    stepsList.AddRange(ConvertStepsXmlToList(stepsXml, workItemStore));
                }
            }

            foreach (var node in comprefNode.Elements())
            {
                if (node.Name == "step")
                {
                    ProcessStepNode(node, stepsList);
                }
                else if (node.Name == "compref")
                {
                    ProcessComprefNode(node, workItemStore, stepsList);
                }
            }
        }

        private static string RemoveHtmlTags(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return Regex.Replace(input, "<.*?>", string.Empty).Trim();
        }
    }
}