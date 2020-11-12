using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Contracts;
using ExtensionMethods;
using Microsoft.Extensions.Logging;

namespace SurpriseText
{
    public class XmlParser<T> where T : Vehicle
    {
        private readonly ILogger<Menu> _logger;
        private readonly IList<T> _entities;
        private readonly IList<Type> _entityTypes;

        public XmlParser(ILogger<Menu> logger)
        {
            _logger = logger;
            _entities = new List<T>();
            _entityTypes = Assembly.GetAssembly(typeof(T))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(T)))
                .ToList();
        }

        public new Type GetType()
        {
            return _entities[0].GetType();
        }

        private void AddEntity(IList<(string Name, string Value)> vehicleProperties)
        {
            foreach (var vehicleType in _entityTypes)
            {
                if (!vehicleProperties.IsTypeOf(vehicleType.GetProperties())) continue;

                var vehicle = MapEntity(vehicleProperties, vehicleType);
                if (vehicle == null) continue;

                _entities.Add(vehicle);
                break;
            }
        }
        private T MapEntity(IList<(string Name, string Value)> entityProperties, Type entityType)
        {
            var classProperties = entityType?.GetProperties();
            if (classProperties == null)
                return null;

            var vehicle = (T)(Activator.CreateInstance(entityType));
            try
            {
                foreach (var property in classProperties)
                {
                    var vehicleProperty = entityProperties.FirstOrDefault(p => string.Equals(p.Name, property.Name, StringComparison.CurrentCultureIgnoreCase));

                    if (vehicleProperty.Equals(default(ValueTuple<string, string>)))
                        return null;

                    property.SetValue(vehicle, Convert.ChangeType(vehicleProperty.Value, property.PropertyType));
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e,$"Exception occurred instance creation of a {entityType} object!");
                return null;
            }

            return vehicle;
        }

        public void ReadFile(string file)
        {
            if (_entities.Any()) _entities.Clear();

            var xmlReaderSettings = new XmlReaderSettings { IgnoreWhitespace = true };

            using (var reader = XmlReader.Create(file, xmlReaderSettings))
            {
                IList<(string Name, string Value)> properties = new List<(string Name, string Value)>();

                while (reader.Read())
                {
                    if (reader.Name != "Something" || reader.NodeType != XmlNodeType.Element) continue;

                    properties.Clear();

                    // read property name 
                    while (reader.Read() && reader.Name != "Something")
                    {
                        if (reader.NodeType != XmlNodeType.Element) continue;

                        var propertyName = reader.Name;
                        // read property value 
                        reader.Read();
                        var propertyValue = reader.Value;
                        reader.Read();
                        properties.Add((propertyName, propertyValue));
                    }
                    AddEntity(properties);
                }
            }
        }

        public IList<T> GetAllVehicles()
        {
            return _entities.ToList();
        }

        public static void WriteToFile(string filePath, IEnumerable<T> vehicles)
        {
            var settings = new XmlWriterSettings { Indent = true, IndentChars = "\t" };

            using (var xmlWriter = XmlWriter.Create(filePath, settings))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("ArrayOfSomething");

                foreach (var vehicle in vehicles)
                {
                    var vehicleProperties = vehicle.GetType().GetProperties();
                    xmlWriter.WriteStartElement("Something");
                    foreach (var property in vehicleProperties)
                    {
                        xmlWriter.WriteElementString(property.Name, property.GetValue(vehicle)?.ToString() ?? string.Empty);
                    }
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
            }
        }
    }
}
