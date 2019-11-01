using System;

namespace LochNessBuilder
{
    /// <summary>
    /// Decorate any builder classes with this attribute, so that the BuilderRegistry can discover them unambiguously.
    /// </summary>
    public class BuilderAttribute : Attribute { }
}