using System;

namespace SourceGenerators.Infrastructure
{
    public static class SourceTypeBuilders
    {
        public static IClassBuilder Class() => new ClassBuilder();
    }

    public interface IClassBuilder
    {
        IClassBuilder AddMethod(IMethodBuilder method);
        IClassBuilder AddProperty(IPropertyBuilder property);
        IClassBuilder Interface(string name);
    }

    internal class ClassBuilder : IClassBuilder
    {
        internal ClassBuilder() { }

        public IClassBuilder AddMethod(IMethodBuilder method)
        {
            throw new NotImplementedException();
        }

        public IClassBuilder AddProperty(IPropertyBuilder property)
        {
            throw new NotImplementedException();
        }

        public IClassBuilder Interface(string name)
        {
            throw new NotImplementedException();
        }
    }

    public interface IMethodBuilder
    {

    }

    internal class MethodBuilder : IMethodBuilder
    {
        internal MethodBuilder() { }
    }

    public interface IPropertyBuilder
    {

    }

    internal class PropertyBuilder : IPropertyBuilder
    {
        internal PropertyBuilder() { }
    }

    public interface IInterfaceBuilder
    {

    }

    internal class InterfaceBuilder : IInterfaceBuilder
    {
        internal InterfaceBuilder() { }
    }
}
