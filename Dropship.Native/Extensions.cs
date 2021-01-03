using System;
using System.Reflection;
using System.Reflection.Metadata;

namespace Dropship.Native
{
    public static class Extensions
    {
        private static FieldInfo AssemblyDefinitionReader { get; } = typeof(AssemblyDefinition).GetField("_reader", BindingFlags.NonPublic | BindingFlags.Instance)!;

        internal static AssemblyName GetSafeAssemblyName(this AssemblyDefinition assembly)
        {
            return GetAssemblyName((MetadataReader) AssemblyDefinitionReader.GetValue(assembly)!, assembly.Name, assembly.Version, assembly.Culture, assembly.PublicKey, assembly.HashAlgorithm, assembly.Flags);
        }

        private static FieldInfo AssemblyReferenceReader { get; } = typeof(AssemblyReference).GetField("_reader", BindingFlags.NonPublic | BindingFlags.Instance)!;

        internal static AssemblyName GetSafeAssemblyName(this AssemblyReference assembly)
        {
            return GetAssemblyName((MetadataReader) AssemblyReferenceReader.GetValue(assembly)!, assembly.Name, assembly.Version, assembly.Culture, assembly.PublicKeyOrToken, AssemblyHashAlgorithm.None, assembly.Flags);
        }

        /// <summary>
        /// Workaround for unity mono being weird
        /// </summary>
        internal static AssemblyName GetAssemblyName(MetadataReader reader, StringHandle nameHandle, Version version, StringHandle cultureHandle, BlobHandle publicKeyOrTokenHandle, AssemblyHashAlgorithm assemblyHashAlgorithm, AssemblyFlags flags)
        {
            var name = reader.GetString(nameHandle);
            var cultureName = (!cultureHandle.IsNil) ? reader.GetString(cultureHandle) : null;
            var hashAlgorithm = (System.Configuration.Assemblies.AssemblyHashAlgorithm) assemblyHashAlgorithm;
            var publicKeyOrToken = !publicKeyOrTokenHandle.IsNil ? reader.GetBlobBytes(publicKeyOrTokenHandle) : null;

            var assemblyName = new AssemblyName(name);

            try
            {
                assemblyName.Version = version;
            }
            catch (NotImplementedException)
            {
            }

            try
            {
                assemblyName.CultureName = cultureName;
            }
            catch (NotImplementedException)
            {
            }

            try
            {
                assemblyName.HashAlgorithm = hashAlgorithm;
            }
            catch (NotImplementedException)
            {
            }

            try
            {
                assemblyName.Flags = GetAssemblyNameFlags(flags);
            }
            catch (NotImplementedException)
            {
            }

            try
            {
                assemblyName.ContentType = GetContentTypeFromAssemblyFlags(flags);
            }
            catch (NotImplementedException)
            {
            }

            var hasPublicKey = (flags & AssemblyFlags.PublicKey) != 0;
            if (hasPublicKey)
            {
                assemblyName.SetPublicKey(publicKeyOrToken);
            }
            else
            {
                assemblyName.SetPublicKeyToken(publicKeyOrToken);
            }

            return assemblyName;
        }

        private static AssemblyNameFlags GetAssemblyNameFlags(AssemblyFlags flags)
        {
            var assemblyNameFlags = AssemblyNameFlags.None;

            if ((flags & AssemblyFlags.PublicKey) != 0)
                assemblyNameFlags |= AssemblyNameFlags.PublicKey;

            if ((flags & AssemblyFlags.Retargetable) != 0)
                assemblyNameFlags |= AssemblyNameFlags.Retargetable;

            if ((flags & AssemblyFlags.EnableJitCompileTracking) != 0)
                assemblyNameFlags |= AssemblyNameFlags.EnableJITcompileTracking;

            if ((flags & AssemblyFlags.DisableJitCompileOptimizer) != 0)
                assemblyNameFlags |= AssemblyNameFlags.EnableJITcompileOptimizer;

            return assemblyNameFlags;
        }

        private static AssemblyContentType GetContentTypeFromAssemblyFlags(AssemblyFlags flags)
        {
            return (AssemblyContentType) (((int) flags & (int) AssemblyFlags.ContentTypeMask) >> 9);
        }
    }
}
