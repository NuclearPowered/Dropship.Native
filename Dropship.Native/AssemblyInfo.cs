using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Dropship.Native
{
    public class EasyCustomAttribute
    {
        public string Name { get; }
        public CustomAttributeValue<object>? Value { get; set; }

        public static EasyCustomAttribute? Read(MetadataReader reader, CustomAttribute attribute)
        {
            if (attribute.Constructor.Kind != HandleKind.MemberReference)
                return null;

            var constructor = reader.GetMemberReference((MemberReferenceHandle) attribute.Constructor);
            var typeReference = reader.GetTypeReference((TypeReferenceHandle) constructor.Parent);

            var typeName = reader.GetString(typeReference.Namespace) + "." + reader.GetString(typeReference.Name);

            // var resolutionScope = typeReference.ResolutionScope;
            // if (resolutionScope.Kind == HandleKind.AssemblyReference)
            // {
            //     var assemblyReference = reader.GetAssemblyReference((AssemblyReferenceHandle) resolutionScope);
            //     typeName += ", " + assemblyReference.GetSafeAssemblyName();
            // }

            CustomAttributeValue<object>? value = null;

            try
            {
                value = attribute.DecodeValue(new AssemblyInfo.DummyProvider());
            }
            catch (Exception)
            {
                // ignored
            }

            return new EasyCustomAttribute(typeName)
            {
                Value = value
            };
        }

        public EasyCustomAttribute(string name)
        {
            Name = name;
        }
    }

    public class EasyType
    {
        public string Name { get; }
        public List<EasyCustomAttribute> Attributes { get; } = new List<EasyCustomAttribute>();

        public EasyType(string name)
        {
            Name = name;
        }
    }

    public class AssemblyInfo
    {
        public AssemblyName Name { get; }
        public List<EasyCustomAttribute> Attributes { get; } = new List<EasyCustomAttribute>();
        public List<EasyType> Types { get; } = new List<EasyType>();

        public string Title { get; }
        public string Version { get; }

        public AssemblyInfo(Stream stream)
        {
            using var pe = new PEReader(stream);
            var reader = pe.GetMetadataReader();
            var assembly = reader.GetAssemblyDefinition();

            Name = assembly.GetSafeAssemblyName();

            foreach (var customAttribute in assembly.GetCustomAttributes().Select(reader.GetCustomAttribute))
            {
                var easyCustomAttribute = EasyCustomAttribute.Read(reader, customAttribute);
                if (easyCustomAttribute == null)
                    continue;

                Attributes.Add(easyCustomAttribute);
            }

            foreach (var typeDefinition in reader.TypeDefinitions.Select(reader.GetTypeDefinition))
            {
                var name = reader.GetString(typeDefinition.Name);
                var customAttributes = typeDefinition.GetCustomAttributes().Select(reader.GetCustomAttribute);

                var type = new EasyType(name);

                foreach (var customAttribute in customAttributes)
                {
                    var easyCustomAttribute = EasyCustomAttribute.Read(reader, customAttribute);
                    if (easyCustomAttribute == null)
                        continue;

                    type.Attributes.Add(easyCustomAttribute);
                }

                Types.Add(type);
            }

            var titleAttribute = GetCustomAttribute("System.Reflection.AssemblyTitleAttribute");
            Title = titleAttribute?.Value?.FixedArguments.Single().Value as string ?? Name.Name;

            var versionAttribute = GetCustomAttribute("System.Reflection.AssemblyInformationalVersionAttribute") ?? GetCustomAttribute("System.Reflection.AssemblyVersionAttribute");
            Version = versionAttribute?.Value?.FixedArguments.Single().Value as string ?? Name.Version.ToString(3);
        }

        public EasyCustomAttribute? GetCustomAttribute(string fullName)
        {
            return Attributes.FirstOrDefault(x => x.Name == fullName);
        }

        internal class DummyProvider : ICustomAttributeTypeProvider<object>
        {
            public object GetPrimitiveType(PrimitiveTypeCode typeCode)
            {
                return null!;
            }

            public object GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
            {
                return null!;
            }

            public object GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
            {
                return null!;
            }

            public object GetSZArrayType(object elementType)
            {
                return null!;
            }

            public object GetSystemType()
            {
                return null!;
            }

            public object GetTypeFromSerializedName(string name)
            {
                return null!;
            }

            public PrimitiveTypeCode GetUnderlyingEnumType(object type)
            {
                return PrimitiveTypeCode.String;
            }

            public bool IsSystemType(object type)
            {
                return false;
            }
        }
    }
}
