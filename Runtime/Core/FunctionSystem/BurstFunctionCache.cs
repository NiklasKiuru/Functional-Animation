using System;
using System.Collections.Generic;
using Unity.Burst;
using System.Reflection;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    /// <summary>
    /// Runtime cache for burst compiled easing function pointers
    /// </summary>
    public class BurstFunctionCache
    {
        // Runtime assembly cache
        private static Dictionary<EF.EasingFunctionDelegate, FunctionPointer<EF.EasingFunctionDelegate>> s_cache;

        // Cache for functions serialized in the editor or from saved assets
        private static Dictionary<uint, FunctionPointer<EF.EasingFunctionDelegate>> s_serializedCache;

        private static List<FunctionAlias> s_definitions;

        /// <summary>
        /// Loads the runtime cache
        /// </summary>
        internal static void Load()
        {
            s_cache = new();
            s_serializedCache = new();
            s_definitions = new List<FunctionAlias>();

            // Load defaults from EF
            foreach (var efunc in typeof(EF).GetRuntimeMethods())
            {
                var efAttr = efunc.GetCustomAttribute<EFunctionAttribute>();
                if(efAttr != null)
                    CachePointer(efAttr, efunc);
            }
                

            // Load runtime user assemblies
            foreach(var assembly in AppDomain.CurrentDomain.GetUserCreatedAssemblies())
            {
                foreach(var type in assembly.DefinedTypes)
                {
                    // Require burstcompile attribute in the type def
                    if(type.GetCustomAttribute<BurstCompileAttribute>() != null)
                    {
                        foreach(var method in type.GetRuntimeMethods())
                        {
                            var efAttr = method.GetCustomAttribute<EFunctionAttribute>();
                            if (efAttr != null && method.GetCustomAttribute<BurstCompileAttribute>() != null && method.IsStatic)
                            {
                                CachePointer(efAttr, method);
                            }
                        }
                    }
                }
            }
#if UNITY_EDITOR
            Debug.Log($"[EF] Loaded runtime function cache with {s_cache.Count} functions");
#endif
        }

        /// <summary>
        /// Gets all cached function pointer aliases
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<FunctionAlias> GetDefinitions() 
        { 
            if(s_definitions == null) 
                Load();
            return s_definitions;
        }

        private static void CachePointer(EFunctionAttribute attr, MethodInfo info)
        {
            if (info.Name.Contains("$BurstManaged"))
                return;
            try
            {
                var name = info.Name;
                if (!string.IsNullOrEmpty(attr.Name))
                    name = attr.Name;
                var del = (EF.EasingFunctionDelegate)info.CreateDelegate(typeof(EF.EasingFunctionDelegate));
                var fPointer = BurstCompiler.CompileFunctionPointer(del);
                var alias = new FunctionAlias(name);
                s_serializedCache.Add(alias.Hash, fPointer);
                s_cache.Add(del, fPointer);
                s_definitions.Add(alias);
                //Debug.Log($"Name: {alias.Value} Hash: {alias.Hash}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EF] Unable to cache function {info.Name}. {ex}");
            }
        }

        /// <summary>
        /// Gets a precompiled function pointer from cache in runtime. Does not work in editor
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public static FunctionPointer<EF.EasingFunctionDelegate> GetCachedPointer(EF.EasingFunctionDelegate func)
        {
            if(func == null)
                throw new ArgumentNullException(nameof(func));
            try
            {
                return s_cache[func];
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
                // Prevents null ref pointer in process jobs
                return s_cache[EF.Linear];
            }
        }

        /// <summary>
        /// Gets a precompiled function pointer from cache with a specified alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static FunctionPointer<EF.EasingFunctionDelegate> GetCachedPointer(FunctionAlias alias)
        {
            if (s_serializedCache == null)
                Load();
            try
            {
                return s_serializedCache[alias.Hash];
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                // Prevents null ref pointer in process jobs
                return s_cache[EF.Linear];
            }
        }

        /// <summary>
        /// Gets a cached function pointer with the shortcut enum
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public static FunctionPointer<EF.EasingFunctionDelegate> GetCachedPointer(Function func)
        {
            if (s_serializedCache == null)
                Load();
            return s_serializedCache[(uint)func];
        }
    }

    internal static class AssemblyExtensions
    {
        private static readonly HashSet<string> s_internalAssemblyNames = new()
        {
            "Bee.BeeDriver",
            "ExCSS.Unity",
            "Mono.Security",
            "mscorlib",
            "netstandard",
            "Newtonsoft.Json",
            "nunit.framework",
            "ReportGeneratorMerged",
            "Unrelated",
            "SyntaxTree.VisualStudio.Unity.Bridge",
            "SyntaxTree.VisualStudio.Unity.Messaging",
            "FunctionalAnimation",
#if !UNITY_INCLUDE_TESTS
            "FunctionalAnimation.Tests"
#endif
        };

        /// <summary>
        /// Gets all the used assemblies from the user and users used packages
        /// </summary>
        /// <param name="appDomain"></param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetUserCreatedAssemblies(this AppDomain appDomain)
        {
            foreach (var assembly in appDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }

                var assemblyName = assembly.GetName().Name;
                if (assemblyName.StartsWith("System") ||
                   assemblyName.StartsWith("Unity") ||
                   assemblyName.StartsWith("UnityEditor") ||
                   assemblyName.StartsWith("UnityEngine") ||
                   s_internalAssemblyNames.Contains(assemblyName))
                {
                    continue;
                }

                yield return assembly;
            }
        }
    }
}
