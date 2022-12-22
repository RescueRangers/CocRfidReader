using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CocRfidReader.Model
{
    public class Coc : IEqualityComparer<Coc>
    {
        public int? PRODUKTIONSNR { get; set; }
        public string? ItemNumber { get; set; }
        public string? Name { get; set; }
        public string? ItemText { get; set; }
        public string? EPC { get; set; }
        public string AccountNumber { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj != null && obj is Coc coc)
                return Equals(this, coc);
            else return false;
        }

        public bool Equals(Coc? x, Coc? y)
        {
            if (x == null || y == null) return false;
            if (x.PRODUKTIONSNR == y.PRODUKTIONSNR)
                return true;
            return false;
        }

        public int GetHashCode([DisallowNull] Coc obj)
        {
            return obj == null ? 0 : obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            var hash = 17;
            unchecked
            {
                hash *= 23 + (PRODUKTIONSNR?.GetHashCode() ?? 0);
            }
            return hash;
        }

        public override string ToString()
        {
            return $"{PRODUKTIONSNR} | {Name} | {ItemNumber} {ItemText}";
        }
    }
}
