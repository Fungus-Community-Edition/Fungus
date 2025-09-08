namespace Amanita.VScripting
{
    [System.Serializable]
    [VariableInfo("", "String", typeof(string))]
    public class StringMuscariable : Muscariable<string>
    {
        public StringMuscariable() { }

        public static StringMuscariable operator +(StringMuscariable a, StringMuscariable b)
            => new StringMuscariable { Value = a.Value + b.Value };

        public static bool operator ==(StringMuscariable a, StringMuscariable b)
            => a.Value == b.Value;

        public static bool operator !=(StringMuscariable a, StringMuscariable b)
            => a.Value != b.Value;

        public override bool Equals(object obj)
        {
            var other = obj as StringMuscariable;
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

    }

}