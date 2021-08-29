using System;

namespace LaDeak.JsonMergePatch.Abstractions;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public class PatchableAttribute : Attribute
{
}