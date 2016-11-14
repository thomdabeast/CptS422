using System;
using System.Numerics;
using System.Text.RegularExpressions;

namespace CS422
{
    public class BigNum
    {
        BigInteger whole, floating;
		int floatingTensPlace;
        bool isUndefined;

        public BigNum(string value)
        {
            Initialize(value);
        }

        public BigNum(double value, bool useDoubleToString)
        {
            if((isUndefined = (double.IsInfinity(value) || double.IsNaN(value))))
            {
                // Do nothing
            }
            else if(useDoubleToString)
            {
                Initialize(value.ToString());
            }
            else
            {
				// Store the left hand side of the '.'
				whole = new BigInteger((int)value);

				// Get the EXACT value from its binary representation.
			 	floating = new BigInteger();
				double bit = value;
				bit -= (int)bit;

				// Get the amount of tenths
				for (int i = 0; i < 30; ++i)
				{
					if (bit * Math.Pow(10.0, i + 1) < 1)
					{
						floatingTensPlace++;
					}
					else
					{
						break;
					}
				}

				// Getting the exact value on the right hand side of the '.'. 20 bit precision.
				for (int i = 0; i < 20; i++)
				{
					// Get the next bit
					bit *= 2;

					// Check the 'bit'
					if ((int)bit == 1)
					{
						// Add it to our stored value.
						BigInteger temp = new BigInteger((Math.Pow(10, 30) * (1 / (Math.Pow(2, i + 1)))));
						floating += temp;
					}

					// Clear the whole number so we just have the decimal.
					bit -= (int)bit;
				}
            }
        }

        void Initialize(string value)
        {
            if(string.IsNullOrEmpty(value))
            {   
                throw new ArgumentException();
            }

            if(value.IndexOf('-') > 0 || value.Contains(" "))
            {
                throw new ArgumentException();
            }

            Regex reg = new Regex("[a-zA-Z]");
            if(reg.IsMatch(value))
            {
                throw new ArgumentException();
            }

			string[] nums = value.Split('.');

			// Should only be 1 '.'
			if (nums.Length > 2)
			{
				throw new ArgumentException();
			}

			whole = BigInteger.Parse(nums[0]);

			if (nums.Length == 1)
			{
				// No '.'
				floating = new BigInteger(0);
				return;
			}
			else 
			{
				// Determine tenths place
				foreach (var c in nums[1])
				{
					if (c == '0')
					{
						floatingTensPlace++;
					}
					else
					{
						break;
					}
				}

				floating = new BigInteger(double.Parse(nums[1]) * Math.Pow(10, 30 - floatingTensPlace - 1));
			}
		}

        public override string ToString()
        {
			if (isUndefined)
			{
				return "undefined";
			}

			if (whole == 0 && floating == 0)
			{
				// Just print 0
				return "0";
			}

			string f = floating.ToString();

			// Append 0s to make it look right
			for (int i = 0; i < floatingTensPlace; i++)
			{
				f = f.Insert(0, "0");
			}

			if (whole == 0)
			{
				return "." + f;
			}

			if (floating == 0)
			{
				return whole.ToString();
			}

			return whole + "." + f.TrimEnd('0');
        }

        public static bool IsToStringCorrect(double value) 
        {
			return new BigNum(value, true).ToString() == value.ToString();
        }

        public bool IsUndefined { get { return isUndefined; } }

        // Mathimatical operators
        public static BigNum operator+(BigNum lhs, BigNum rhs)
        {
			if (lhs.IsUndefined || rhs.IsUndefined)
			{
				return new BigNum(double.NaN.ToString());
			}

			BigInteger w = lhs.whole + rhs.whole;
			BigInteger f = lhs.floating + rhs.floating;

			if (f.ToString().Length > 30)
			{
				// If we got a whole number from adding the floating point values then add it to the whole int.
				int whole_num = (int)(f / new BigInteger(Math.Pow(10, 30)));
				w += whole_num;

				// Get rid of whole number
				f -= whole_num * new BigInteger(Math.Pow(10, 30));

				if (f == 0)
				{
					return new BigNum(w.ToString());
				}

				// Take off the whole number from the floating value and return it.
				return new BigNum(w + "." + f.ToString().Remove(0, f.ToString().Length - 30));
			}

			// Put zeros on final number
			string zeros = "";
			int min = (lhs.floatingTensPlace > rhs.floatingTensPlace) ? rhs.floatingTensPlace : lhs.floatingTensPlace;
			for (int i = 0; i < min; i++)
			{
				zeros += "0";
			}

			return new BigNum(w + "." + zeros + f);
        }

        public static BigNum operator-(BigNum lhs, BigNum rhs)
        {
			if (lhs.IsUndefined || rhs.IsUndefined)
			{
				return new BigNum(double.NaN.ToString());
			}

			BigInteger w = lhs.whole - rhs.whole;
			BigInteger f = lhs.floating - rhs.floating;

			if (f < 0)
			{
				w--;

				f = new BigInteger(Math.Pow(10, 30)) + f;
				return new BigNum(w + "." + f);
			}

			// Put zeros on final number
			string zeros = "";
			int min = (lhs.floatingTensPlace > rhs.floatingTensPlace) ? rhs.floatingTensPlace : lhs.floatingTensPlace;
			for (int i = 0; i < min; i++)
			{
				zeros += "0";
			}

			return new BigNum(w + "." + zeros + f);
        }

        public static BigNum operator*(BigNum lhs, BigNum rhs)
        {
			if (lhs.IsUndefined || rhs.IsUndefined)
			{
				return new BigNum(double.NaN.ToString());
			}

			BigInteger w = lhs.whole * rhs.whole;
			BigInteger f = lhs.floating * rhs.floating;
			double a = double.Parse(lhs.ToString().Substring(lhs.ToString().IndexOf('.')));
			BigInteger lf = new BigInteger(a) * lhs.whole;
			BigInteger rf = (rhs.floating / (int)Math.Pow(10, 30)) * lhs.whole;



			return new BigNum((w + (int)lf + (int)rf) + "." + f);
        }

        public static BigNum operator/(BigNum lhs, BigNum rhs)
        {
			if (lhs.IsUndefined || rhs.IsUndefined)
			{
				return new BigNum(double.NaN.ToString());
			}

			throw new NotImplementedException();
        }

        // Logical operators
        public static bool operator>(BigNum lhs, BigNum rhs)
        {
			if (lhs.IsUndefined || rhs.IsUndefined)
			{
				return false;
			}

			if (lhs.whole > rhs.whole)
			{
				return true;
			}

			if (lhs.whole == rhs.whole)
			{
				if (lhs.floating > rhs.floating)
				{
					return true;
				}
			}

			return false;
        }

        public static bool operator>=(BigNum lhs, BigNum rhs)
        {
			if (lhs.IsUndefined || rhs.IsUndefined)
			{
				return false;
			}

			if (lhs.whole > rhs.whole)
			{
				return true;
			}

			if (lhs.whole == rhs.whole)
			{
				if (lhs.floating >= rhs.floating)
				{
					return true;
				}
			}

			return false;
        }

        public static bool operator<(BigNum lhs, BigNum rhs)
        {
			if (lhs.IsUndefined || rhs.IsUndefined)
			{
				return false;
			}

			if (lhs.whole < rhs.whole)
			{
				return true;
			}

			if (lhs.whole == rhs.whole)
			{
				if (lhs.floating < rhs.floating)
				{
					return true;
				}
			}

			return false;
        }

        public static bool operator<=(BigNum lhs, BigNum rhs)
        {
			if (lhs.IsUndefined || rhs.IsUndefined)
			{
				return false;
			}

			if (lhs.whole < rhs.whole)
			{
				return true;
			}

			if (lhs.whole == rhs.whole)
			{
				if (lhs.floating <= rhs.floating)
				{
					return true;
				}
			}

			return false;
        }
    }
}
