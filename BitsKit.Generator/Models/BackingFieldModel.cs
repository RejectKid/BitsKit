using Microsoft.CodeAnalysis;

namespace BitsKit.Generator.Models
{
    internal record BackingFieldModel
    {
        public readonly string Name;
        public readonly string TypeString;
        public readonly int FixedSize;
        public readonly bool IsReadOnly;
        
        public readonly BackingFieldType Type;

        public BackingFieldModel(IFieldSymbol fieldSymbol, BackingFieldType type)
        {
            Name = fieldSymbol.Name;
            TypeString = fieldSymbol.Type.ToDisplayString();
            FixedSize = fieldSymbol.FixedSize;
            IsReadOnly = fieldSymbol.IsReadOnly;
            
            Type = type;
        }
    }
}