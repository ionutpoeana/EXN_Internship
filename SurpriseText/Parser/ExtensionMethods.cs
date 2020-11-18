using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace ExtensionMethods
{
    public static class ExtensionMethods
    {
        public static bool IsTypeOf(this IList<(string Name, string Value)> vehicleProperties, IList<PropertyInfo> propertiesInfos )
        {
            if (vehicleProperties.Count != propertiesInfos.Count) return false;

            var matchedProperties = (
                from vehicleProp in vehicleProperties 
                    from infoProp in propertiesInfos 
                        where vehicleProp.Name.ToLower() == infoProp.Name.ToLower()
                        select vehicleProp).Count();

            return matchedProperties == vehicleProperties.Count;
        }

        public static T DeepClone<T>(this T entity)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream,entity);
                stream.Seek(0, SeekOrigin.Begin);
                return (T) formatter.Deserialize(stream);
            }
        }

    }

}
