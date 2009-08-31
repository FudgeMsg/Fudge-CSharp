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
        public static readonly FudgeFieldType<byte> ByteType = new FudgeFieldType<byte>(FudgeTypeDictionary.BYTE_TYPE_ID, false, 1);
        public static readonly FudgeFieldType<short> ShortType = new FudgeFieldType<short>(FudgeTypeDictionary.SHORT_TYPE_ID, false, 2);
        public static readonly FudgeFieldType<int> IntType = new FudgeFieldType<int>(FudgeTypeDictionary.INT_TYPE_ID, false, 4);
        public static readonly FudgeFieldType<long> LongType = new FudgeFieldType<long>(FudgeTypeDictionary.LONG_TYPE_ID, false, 8);
        public static readonly FudgeFieldType<float> FloatType = new FudgeFieldType<float>(FudgeTypeDictionary.FLOAT_TYPE_ID, false, 4);   // We use the name Float rather than Single to be consistent with Fudge-Java
        public static readonly FudgeFieldType<double> DoubleType = new FudgeFieldType<double>(FudgeTypeDictionary.DOUBLE_TYPE_ID, false, 8);
    }
}
