using UnityEngine;

namespace TeddyToolKit.Core.Attribute
{

    /// <summary>
    /// This can be added to any string to make it render as the GameObjects with Tag dropdown in the inspector.
    /// Sealed makes it so this class cannot be inherited from, making it the final
    /// stage in the inheritance hierarchy.
    /// The standard is to have "Attribute" at the end of the class name, which can be ignored
    /// when adding it to anything: in this example, it would just be used as [Tag]
    /// This needs to be used in conjunction with the property drawer to visualise Editor.Core.Attribute.TagDrawer namespace
    /// </summary>
    public sealed class TagAttribute : PropertyAttribute
    {
        
    }
}