using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGamma.Fudge.Types
{
    /// <summary>
    ///  A collection of all the simple fixed-width field types that represent
    ///  primitive values.
    ///  Because these are fast-pathed inside the encoder/decoder sequence,
    ///  there's no point in breaking them out to other classes.
    /// </summary>
    public static class PrimitiveFieldTypes
    {
        public static readonly FudgeFieldType<bool> BooleanType = new FudgeFieldType<bool>(FudgeTypeDictionary.BOOLEAN_TYPE_ID, false, 1);
        public static readonly FudgeFieldType<sbyte> SByteType = new FudgeFieldType<sbyte>(FudgeTypeDictionary.SBYTE_TYPE_ID, false, 1, (sbyte i, ref FudgeFieldType t) => MinimizeIntegers(i, ref t));
        public static readonly FudgeFieldType<short> ShortType = new FudgeFieldType<short>(FudgeTypeDictionary.SHORT_TYPE_ID, false, 2, (short i, ref FudgeFieldType t) => MinimizeIntegers(i, ref t));
        public static readonly FudgeFieldType<int> IntType = new FudgeFieldType<int>(FudgeTypeDictionary.INT_TYPE_ID, false, 4, (int i, ref FudgeFieldType t) => MinimizeIntegers(i, ref t));
        public static readonly FudgeFieldType<long> LongType = new FudgeFieldType<long>(FudgeTypeDictionary.LONG_TYPE_ID, false, 8, (long i, ref FudgeFieldType t) => MinimizeIntegers(i, ref t));
        public static readonly FudgeFieldType<float> FloatType = new FudgeFieldType<float>(FudgeTypeDictionary.FLOAT_TYPE_ID, false, 4);
        public static readonly FudgeFieldType<double> DoubleType = new FudgeFieldType<double>(FudgeTypeDictionary.DOUBLE_TYPE_ID, false, 8);

        #region Minimizations
        private static object MinimizeIntegers(long valueAsLong, ref FudgeFieldType type)
        {
            object value = valueAsLong;
            if ((valueAsLong >= sbyte.MinValue) && (valueAsLong <= sbyte.MaxValue))
            {
                value = (sbyte)valueAsLong;
                type = PrimitiveFieldTypes.SByteType;
            }
            else if ((valueAsLong >= short.MinValue) && (valueAsLong <= short.MaxValue))
            {
                value = (short)valueAsLong;
                type = PrimitiveFieldTypes.ShortType;
            }
            else if ((valueAsLong >= int.MinValue) && (valueAsLong <= int.MaxValue))
            {
                value = (int)valueAsLong;
                type = PrimitiveFieldTypes.IntType;
            }
            return value;
        }

        // TODO t0rx 2009-10-07 -- Need to get clarification on whether false should minimise to Indicator.  Currently Fudge-Java does not so matching that behaviour.
        //private static object MinimizeBoolean(bool value, ref FudgeFieldType type)
        //{
        //    if (!value)
        //    {
        //        type = IndicatorFieldType.Instance;
        //        return IndicatorType.Instance;
        //    }

        //    return value;
        //}
        #endregion
    }
}
