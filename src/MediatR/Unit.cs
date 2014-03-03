namespace MediatR
{
    using System;

    public sealed class Unit : IComparable
    {
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