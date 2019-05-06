using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace FiatShamirClient
{
    class FiatShamirHelepr
    {
        public BigInteger x;
        public BigInteger y;
        public BigInteger r;
        BigHelper bh;

        public void GenWitness(BigInteger n)
        {
            bh = new BigHelper();
            r = BigHelper.GetRandomBigInt(1, n-1);
            x = BigInteger.ModPow(r, 2, n);
        }

        public void GetResponse(byte e,BigInteger s,BigInteger n)
        {
            if(e==0)
            {
                y = r;
            }
            else
            {
                y = (r * s) % n;
            }
        }

        public bool AllOk(BigInteger v,byte e,BigInteger n)
        {
            if(y*y==(x*BigInteger.Pow(v,e))%n)
            {
                return true;
            }
            return false;
        }
    }
}
