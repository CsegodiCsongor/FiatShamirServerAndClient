using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FiatShamirClient
{
    class BigHelper
    {
        public BigInteger GenerateRandomE(BigInteger n)
        {
            List<BigInteger> Exists = new List<BigInteger>();
            BigInteger aux;
            while (true)
            {
                aux = BigHelper.GetRandomBigInt(2, n);
                while (Exists.Contains(aux))
                {
                    aux = BigHelper.GetRandomBigInt(2, n);
                }
                if (GCD(aux, n) == 1)
                {
                    break;
                }
                Exists.Add(aux);
            }
            return aux;
        }

        public BigInteger GCD(BigInteger e, BigInteger phi)
        {
            while (e != 0 && phi != 0)
            {
                if (e > phi)
                {
                    e %= phi;
                }
                else
                {
                    phi %= e;
                }
            }
            if (e == 0)
            {
                return phi;
            }
            else
            {
                return e;
            }
        }

        public static BigInteger GetRandomBigInt(BigInteger minVal, BigInteger maxVal)
        {
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[maxVal.ToByteArray().LongLength];
            BigInteger a;
            do
            {
                rng.GetBytes(bytes);
                a = new BigInteger(bytes);
            }
            while (a < minVal || a >= maxVal);
            return a;
        }
    }
}
