using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace EasyCom.CustomStr
{
    public class CustomStrManager
    {
        private static string CommandListFileName = "CustomString.json";
        private static string CommandListFilePath = Path.Combine(App.DataPath, CommandListFileName);

        public List<CustomStrTab> CustomStrTabList { get; } = new List<CustomStrTab>();
        private List<CustomStrData> CustomStrDisplayList = new List<CustomStrData>();

        private MainWindow mainWindow = null;
        private JObject CommandsJson = null;
        public Action OnLoadFinish { get; set; } = null;
        public CustomStrManager(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

        }
        public Task LoadCustomStrAsync()
        {
            Task task = Task.Run(LoadFromFile);
            return task;
        }
        public Task SaveCustomStrAsync()
        {
            Task task = Task.Run(SaveToFile);
            return task;
        }

        /*
                {
                  "CustomStrings": [
                    {
                      "Name": "New Tab",
                      "Description": "",
                      "Index": 0,
                      "List": [
                        {
                          "Name": "Example",
                          "Description": "",
                          "Command": "Welcome to use Easycom!"
                        }
                      ]
                    }
                  ]
                } 
                */

        private void LoadFromFile()
        {
            
            if (!File.Exists(CommandListFilePath))
            {
                CreateExampleCommandFile();
            }

            try
            {
                using (StreamReader sr = File.OpenText(CommandListFilePath))
                {

                    CommandsJson = JObject.Parse(sr.ReadToEnd());
                    JToken TabList = null;
                    TabList = CommandsJson.Property("CustomStrings", StringComparison.InvariantCulture)?.Value;
                    if (TabList != null&& TabList is JArray)
                    {
                        foreach (JObject tab in TabList)
                        {
                            string tabName = (string)tab.Property("Name",StringComparison.InvariantCulture)?.Value;
                            string Description = (string)tab.Property("Description", StringComparison.InvariantCulture)?.Value;
                            JToken CustomStrings = tab.Property("StrList", StringComparison.InvariantCulture)?.Value;
                            
                            if (tabName != null && Description != null && CustomStrings is JArray)
                            {
                                CustomStrTab newTab = new CustomStrTab(tabName);
                                newTab.Description = Description;
                                CustomStrTabList.Add(newTab);
                                foreach (JObject cs in (JArray)CustomStrings)
                                {

                                    string csName = (string)cs.Property("Name",StringComparison.InvariantCulture)?.Value;
                                    string csDescription = (string)cs.Property("Description", StringComparison.InvariantCulture)?.Value;
                                    string csStr= (string)cs.Property("Text", StringComparison.InvariantCulture)?.Value;
                                    if (csName != null && csDescription != null && csStr != null) ;
                                        newTab.StrList.Add(new CustomStrData(csName, csDescription, csStr));
                                }
                            }
                        }
                        
                        OnLoadFinish?.Invoke();
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                CreateExampleCommandFile();
            }
        }

        public void SaveToFile()
        {
            try
            {
                using (StreamWriter sr = File.CreateText(CommandListFilePath))
                {
                    JObject parent = new JObject();
                    JArray tabArray = JArray.FromObject(CustomStrTabList);
                    parent.Add(new JProperty("CustomStrings", tabArray));
                    sr.WriteLine(parent.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        public void GetCommandsListByName(string name)
        {

        }

        private void CreateExampleCommandFile()
        {

            Console.WriteLine("Create New Config File");
            if (File.Exists(CommandListFilePath))
            {
                File.Delete(CommandListFilePath);
            }
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(CommandListFilePath))
            {

                JObject parent = new JObject();
                JArray TabArray = new JArray();
                JObject NewTab = new JObject();
                JArray StrList = new JArray();
                parent.Add(new JProperty("CustomStrings", TabArray));

                NewTab.Add(new JProperty("Name", "New Tab"));
                NewTab.Add(new JProperty("Description", ""));
                //NewTab.Add(new JProperty("Index", 0));
                NewTab.Add(new JProperty("StrList", StrList));
                TabArray.Add(NewTab);

                CustomStrData example = new CustomStrData("Example", "No", "Welcome to use Easycom!");
                JObject newItem = JObject.FromObject(example);
                StrList.Add(newItem);

                sw.WriteLine(parent.ToString());
                sw.Close();
            }
        }
    }
}
