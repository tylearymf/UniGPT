using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

internal class Inspector
{
    public Inspector()
    {
    }

    public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        if (assembly == null) throw new ArgumentNullException("assembly");

        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null);
        }
    }

    /// <summary>
    /// Get all types of a specific type/interface in an assembly
    /// </summary>
    /// <param name="assembly">Assembly to check</param>
    /// <param name="type">Type (can be Interface type)</param>
    /// <returns></returns>
    public static IEnumerable<Type> GetTypesWithInterface(Assembly assembly, Type type)
    {
        return GetLoadableTypes(assembly).Where(type.IsAssignableFrom).ToList();
    }

    public static IEnumerable<Type> GetTypesWithInterface(Type type)
    {
        var types = new List<Type>();
        foreach (var assembly in GetAllAssemblies())
            types.AddRange(GetTypesWithInterface(assembly, type));

        return types;
    }

    public static IEnumerable<Type> GetAllTypesWithStaticPropertiesForAssembly(Assembly assembly)
    {
        var result = new List<Type>();
        if (assembly.IsDynamic)
            return result;

        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            var message = $"Could not load types for assembly (some types in this assembly might refer to assemblies that are not referenced): {assembly.FullName}\n";
            foreach (var except in e.LoaderExceptions)
                message += "\n   -- Loader exception: " + except.Message;

            Debug.LogWarning(message);

            types = e.Types.Where(t => t != null).ToArray();
        }

        foreach (Type t in types.Where(t =>
            t != null &&
            !t.IsInterface &&
            !t.IsAbstract &&
            !t.IsEnum &&
            !t.ContainsGenericParameters))
        {
            if (!t.IsClass && !t.IsValueType) continue;

            var staticProperties = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (!staticProperties.Any()) continue;

            result.Add(t);
        }

        return result;
    }

    public static IEnumerable<Type> GetAllTypesWithStaticPropertiesForAssemblyNamespace(Assembly assembly, string ns)
    {
        return GetAllTypesWithStaticPropertiesForAssembly(assembly).Where(t => t.Namespace == ns);
    }

    public static IEnumerable<Assembly> GetAllAssemblies()
    {
        AppDomain app = AppDomain.CurrentDomain;
        var allAssemblies = app.GetAssemblies();

        return allAssemblies;
    }

    // Get list of all relevant assemblies
    // Currently not taking all assemblies because it caused an issue when setting up
    // the code analyser. Need to investigate.
    public static IEnumerable<Assembly> GetRelevantAssemblies(bool includeSystem = false)
    {
        AppDomain app = AppDomain.CurrentDomain;
        var allAssemblies = app.GetAssemblies();

        // Note: Jetbrains assembly loading throws exception. Needs to investigate
        var assemblies = allAssemblies
            .Where(assembly => assembly.FullName.ToLower().Contains("unity"))
            .OrderBy(assembly => assembly.FullName).ToList();

        // Add system assemblies at the end (until I have assembly/namespace filtering)
        if (includeSystem)
            assemblies.AddRange(allAssemblies.Where(assembly => assembly.FullName.Contains("System")));

        return assemblies;
    }

    public static IEnumerable<Assembly> GetReferencableAssemblies()
    {
        return GetAllAssemblies().Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location));
    }

    public static IEnumerable<string> GetAllNamespaces(Assembly assembly)
    {
        return GetLoadableTypes(assembly)
            .Select(t => t.Namespace)
            .Distinct()
            .OrderBy(ns => ns);
    }
}
