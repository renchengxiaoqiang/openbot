using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Misc
{
    public class ConstIntArray : IEquatable<ConstIntArray>
    {
        private readonly int[] _arr;
        private int? _hashCode = null;
        public ConstIntArray Clone()
        {
            return base.MemberwiseClone() as ConstIntArray;
        }


        public ConstIntArray(int[] arr, bool calcHashCode = false)
        {
            this._arr = arr;
            if (calcHashCode)
            {
                GetHashCode();
            }
        }

        private bool IsArrayEqual(int[] ano)
        {
            if (_arr == null || ano == null)
            {
                return (_arr == null && ano == null);
            }
            bool rt = true;
            if (_arr.Length == ano.Length)
            {
                for (int i = 0; i < _arr.Length; i++)
                {
                    if (_arr[i] != ano[i])
                    {
                        rt = false;
                        break;
                    }
                }
            }
            else
            {
                rt = false;
            }

            return rt;
        }

        public override bool Equals(object obj)
        {
            return (obj is ConstIntArray) && Equals(obj as ConstIntArray);
        }

        public override int GetHashCode()
        {
            if (_hashCode == null)
            {
                _hashCode = CalcHashCode();
            }
            return _hashCode.Value;
        }

        private int? CalcHashCode()
        {
            if (_arr == null) return 0;

            if (_arr.Length == 0) return -1;

            int hashCode = _arr.Length;
            for (int i = 0; i < _arr.Length; i++)
            {
                hashCode ^= _arr[i];
            }
            return hashCode;
        }

        public bool Equals(ConstIntArray other)
        {
            if (other == null) return false;

            if (this == other) return true;

            if (other._hashCode == null || _hashCode == null || other.GetHashCode() == GetHashCode())
            {
                return IsArrayEqual(other._arr);
            }
            return false;
        }
    }
}
