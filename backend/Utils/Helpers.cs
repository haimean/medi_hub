using System.Reflection;

namespace Utils
{
    public static class Helpers
    {
        public static bool IsImageFile(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();

            if (extension != null)
            {
                string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                string fileExtension = extension.ToLower();

                foreach (string ext in imageExtensions)
                {
                    if (fileExtension == ext)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// apply property changed from ob1 to ob2
        /// </summary>
        /// <typeparam name="O1">request object</typeparam>
        /// <typeparam name="O2">model object</typeparam>
        /// <param name="object1"></param>
        /// <param name="object2"></param>
        /// <exception cref="ArgumentException"></exception>
        /// CreatedBy: PQ Huy (25.11.2024)
        public static void ApplyChange<SourceObj, DestinationObj>(SourceObj sourceObj, DestinationObj destinationObj, bool isSetNullValue = false)
        {

            var type1 = sourceObj.GetType();
            var type2 = destinationObj.GetType();

            var type1Propertys = type1.GetProperties();

            foreach (var type1Property in type1Propertys)
            {
                if (type1Property.Name.ToLower() == "id")
                {
                    continue;
                }

                var type2Property = type2.GetProperty(type1Property.Name);

                if (type2Property != null)
                {
                    var value1 = type1Property.GetValue(sourceObj);
                    var value2 = type2Property.GetValue(destinationObj);

                    if (!isSetNullValue)
                    {
                        if (value1 != null && value1 != value2)
                        {
                            type2Property.SetValue(destinationObj, value1);
                        }
                    }
                    else
                    {
                        if (value1 != value2)
                        {
                            type2Property.SetValue(destinationObj, value1);
                        }
                    }
                }


            }
        }

        /// <summary>
        /// Func help update entity by name
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// CreatedBy: PQ Huy (25.11.2024)
        public static void UpdateEntityField(object entity, string propertyName, object value)
        {
            if (propertyName.EndsWith("7") || propertyName.EndsWith("28"))
            {
                propertyName = propertyName.Substring(0, propertyName.Length - 1);
            }

            var property = entity.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
                         ?? entity.GetType().GetProperty(char.ToUpper(propertyName[0]) + propertyName.Substring(1), BindingFlags.Public | BindingFlags.Instance);

            if (property != null && property.CanWrite)
            {
                try
                {
                    var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    var convertedValue = Convert.ChangeType(value, targetType);

                    property.SetValue(entity, convertedValue);
                }
                catch (InvalidCastException)
                {
                    Console.WriteLine($"Cannot convert value '{value}' to type {property.PropertyType.Name} for property {propertyName}.");
                }
                catch (FormatException)
                {
                    Console.WriteLine($"Format of value '{value}' is invalid for type {property.PropertyType.Name}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to set property {propertyName}: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Property {propertyName} does not exist or is not writable.");
            }
        }
    }
}
