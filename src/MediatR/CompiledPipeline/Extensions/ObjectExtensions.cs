using System;
using System.Reflection;

namespace MediatR.CompiledPipeline.Extensions;

public static class ObjectExtensions
{
    public static void CopyPropertiesTo<T>(
        this T source, 
        T destination)
            where T : class
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        // Iterate the Properties of the destination instance and  
        // populate them from their source counterparts  
        PropertyInfo[] destinationProperties = destination.GetType().GetProperties(); 
        foreach (PropertyInfo destinationPi in destinationProperties)
        {
            PropertyInfo sourcePi = source!.GetType().GetProperty(destinationPi.Name)!;
            if (Nullable.GetUnderlyingType(sourcePi.PropertyType) != null)
            {
                destinationPi.SetValue(destination, sourcePi.GetValue(source, null)!, null);
            }
            else
            {
                destinationPi.SetValue(destination, sourcePi.GetValue(source, null), null);
            }
        } 
    }
}