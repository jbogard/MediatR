namespace MediatR
{
    using System;

    /// <summary>
    /// Represents a Void type, since Void is not a valid type in C#
    /// </summary>
    public sealed class Unit : IComparable
    {
        /// <summary>
        /// Default and only value of Unit type
        /// </summary>
        public static readonly Unit Value = new Unit();

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object obj)
        {
            return obj == null || obj is Unit;
        }

        int IComparable.CompareTo(object obj)
        {
            return 0;
        }
    }
}