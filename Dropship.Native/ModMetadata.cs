using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Dropship.Native
{
    public static class ModMetadataParser
    {
        public static ModMetadata Parse(Stream stream)
        {
            var assemblyInfo = new AssemblyInfo(stream);
            var plugin = assemblyInfo.Types.SingleOrDefault(x => x.Attributes.Any(a => a.Name == "BepInEx.BepInPlugin"));
            if (plugin == null)
            {
                throw new PluginNotFoundException(assemblyInfo.Name.FullName);
            }

            var attribute = plugin.Attributes.Single(a => a.Name == "BepInEx.BepInPlugin");
            var arguments = attribute.Value!.Value.FixedArguments.Select(x => (string) x.Value!).ToArray();

            return new ModMetadata(
                assemblyInfo.Name.Name,
                arguments[0],
                arguments[1] ?? assemblyInfo.Title,
                arguments[2] ?? assemblyInfo.Version,
                assemblyInfo.GetCustomAttribute("System.Reflection.AssemblyDescriptionAttribute")?.Value?.FixedArguments.Single().Value as string,
                assemblyInfo.GetCustomAttribute("System.Reflection.AssemblyCompanyAttribute")?.Value?.FixedArguments.Single().Value as string
            );
        }

        public class PluginNotFoundException : Exception
        {
            public PluginNotFoundException(string message) : base(message)
            {
            }
        }
    }

    internal static class NativeMethods
    {
        [UnmanagedCallersOnly(EntryPoint = "parse")]
        public static IntPtr Parse(IntPtr pathPtr)
        {
            try
            {
                var path = Marshal.PtrToStringAnsi(pathPtr);
                if (path == null || !File.Exists(path))
                {
                    throw new FileNotFoundException(path);
                }

                using var stream = File.OpenRead(path);
                return ModMetadataParser.Parse(stream).ToPointer();
            }
            catch (Exception e)
            {
                return new ModMetadata(e.ToString()).ToPointer();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public readonly struct ModMetadata
    {
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool success;

        [MarshalAs(UnmanagedType.LPStr)]
        public readonly string assemblyName;

        [MarshalAs(UnmanagedType.LPStr)]
        public readonly string id;

        [MarshalAs(UnmanagedType.LPStr)]
        public readonly string name;

        [MarshalAs(UnmanagedType.LPStr)]
        public readonly string version;

        [MarshalAs(UnmanagedType.LPStr)]
        public readonly string description;

        [MarshalAs(UnmanagedType.LPStr)]
        public readonly string authors;

        public ModMetadata(string assemblyName, string id, string name, string version, string? description, string? authors)
        {
            this.success = true;

            this.assemblyName = assemblyName;
            this.id = id;
            this.name = name;
            this.version = version;
            this.description = description ?? string.Empty;
            this.authors = authors ?? string.Empty;
        }

        public ModMetadata(string description)
        {
            this.success = false;
            this.description = description;

            this.assemblyName = string.Empty;
            this.id = string.Empty;
            this.name = string.Empty;
            this.version = string.Empty;
            this.authors = string.Empty;
        }

        public IntPtr ToPointer()
        {
            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(this));
            Marshal.StructureToPtr(this, ptr, false);
            return ptr;
        }
    }
}
