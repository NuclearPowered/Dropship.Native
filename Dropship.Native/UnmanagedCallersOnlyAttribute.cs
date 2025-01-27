// ReSharper disable CheckNamespace

namespace System.Runtime.InteropServices
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class UnmanagedCallersOnlyAttribute : Attribute
    {
        public string? EntryPoint;
        public CallingConvention CallingConvention;
    }
}
