using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Logger;

namespace ConfigurationProvider
{

    public interface IConfigurationParser<out T>
    {
        T Parse();
    }

    class Help
    {
        public static bool flg = false;
    }
    class Parser<T> : IConfigurationParser<T> where T : class
    {
        private readonly string jsonPath;
        private readonly string xmlPath = null;
        private readonly string xsdPath = null;
        private ManagerOfLogging managerOfLogging;

        private bool Validation(string xmlPath, string xsdPath)
        {
            try
            {
                var setting = new XmlReaderSettings();
                setting.ValidationType = ValidationType.Schema;
                setting.Schemas.Add(null, XmlReader.Create(xsdPath));

                var xmlReader = XmlReader.Create(xmlPath, setting);
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlReader);
                return true;
            }

            catch (Exception ex)
            {
                if (managerOfLogging.setuped == true)
                {
                    managerOfLogging.LoggerMessage("Error in XMLvalidation: ");
                }
                return false;
            }
        }

        public Parser(string path)
        {
            managerOfLogging = new ManagerOfLogging();

            this.jsonPath = path;

            this.xmlPath = path;
            if (File.Exists(Path.ChangeExtension(xmlPath, "xsd")))
            {
                xsdPath = Path.ChangeExtension(xmlPath, "xsd");
            }


        }

        public T Parse()
        {
            if (Help.flg == false)
            {
                using (var fileStream = new FileStream(jsonPath, FileMode.OpenOrCreate))
                {
                    using (var document = JsonDocument.Parse(fileStream))
                    {
                        var element = document.RootElement;

                        if (typeof(T).GetProperties().First().Name
                            != element.EnumerateObject().First().Name)
                        {
                            element = element.GetProperty(typeof(T).Name);
                        }
                        try
                        {
                            return JsonSerializer.Deserialize<T>(element.GetRawText());
                        }
                        catch (Exception ex)
                        {
                            if (managerOfLogging.setuped == true)
                            {
                                managerOfLogging.LoggerMessage("Error in json file: ");
                            }
                            return null;
                        }

                    }
                }
            }
            else
            {
                if (xsdPath != null && !Validation(xmlPath, xsdPath))
                {
                    return null;
                }

                try
                {
                    var xDocument = XDocument.Load(xmlPath);
                    var elements =
                        from element in xDocument.Elements(typeof(T).Name).DescendantsAndSelf()//потомков и себя самого
                        select element;

                    var xmlFormat = elements.First().ToString();
                    var xmlSerializer = new XmlSerializer(typeof(T));

                    using (TextReader textReader = new StringReader(xmlFormat))
                    {
                        return xmlSerializer.Deserialize(textReader) as T;
                    }
                }
                catch (Exception ex)
                {
                    if (managerOfLogging.setuped == true)
                    {
                        managerOfLogging.LoggerMessage("Error in XMLfile: ");
                    }
                    return null;
                }
            }
        }





    }



}