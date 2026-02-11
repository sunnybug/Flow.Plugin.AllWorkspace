using System;

namespace Flow.Launcher.Plugin.AllWorkspace.Shared
{
    public readonly struct PathString : IEquatable<PathString>
    {
        public readonly string Value;

        public PathString(string value) => Value = value ?? string.Empty;

        public override string ToString() => Value;

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

        public bool Equals(PathString other) => string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj)
        {
            if (obj is PathString ps) return Equals(ps);
            if (obj is string s) return string.Equals(Value, s, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        public static bool operator ==(PathString left, PathString right) => left.Equals(right);
        public static bool operator !=(PathString left, PathString right) => !left.Equals(right);
        public static implicit operator string(PathString h) => h.Value;
        public static implicit operator PathString(string s) => new(s ?? string.Empty);
    }
}
