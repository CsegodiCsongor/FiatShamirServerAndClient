using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FiatShamirServer
{
    public class BigRND
    {
        public BigInteger p;
        public BigInteger q;
        public BigInteger n;

        public BigRND()
        {
            GenerateTwoRandomPrimes(new BigInteger(100000000000));
        }

        public static bool MillerRabinTest(BigInteger source, int certainty)
        {
            if (source == 2 || source == 3)
                return true;
            if (source < 2 || source % 2 == 0)
                return false;

            BigInteger d = source - 1;
            int s = 0;

            while (d % 2 == 0)
            {
                d /= 2;
                s += 1;
            }


            for (int i = 0; i < certainty; i++)
            {
                BigInteger a = GetRandomBigInt(2, source - 2);
                BigInteger x = BigInteger.ModPow(a, d, source);
                if (x == 1 || x == source - 1)
                    continue;

                for (int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, source);
                    if (x == 1)
                        return false;
                    if (x == source - 1)
                        break;
                }

                if (x != source - 1)
                    return false;
            }

            return true;
        }

        public void GenerateTwoRandomPrimes(BigInteger source)
        {
            List<BigInteger> Exists = new List<BigInteger>();
            BigInteger a;
            while (true)
            {
                a = BigRND.GetRandomBigInt(2, source - 2);
                while (Exists.Contains(a))
                {
                    a = BigRND.GetRandomBigInt(2, source - 2);
                }
                if (MillerRabinTest(a, 5))
                {
                    break;
                }
                Exists.Add(a);
            }
            p = a;
            Exists.Add(p);
            while (true)
            {
                a = BigRND.GetRandomBigInt(2, source - 2);
                while (Exists.Contains(a))
                {
                    a = BigRND.GetRandomBigInt(2, source - 2);
                }
                if (MillerRabinTest(a, 5))
                {
                    break;
                }
                Exists.Add(a);
            }
            q = a;
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
