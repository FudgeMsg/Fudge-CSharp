using System;
using Xunit;

namespace OpenGamma.Fudge.Tests.Unit
{
    class FudgeTestUtils
    {
        public static void AssertAllFieldsMatch(FudgeMsg expectedMsg, FudgeMsg actualMsg)
        {
            var expectedIter = expectedMsg.GetAllFields().GetEnumerator();
            var actualIter = actualMsg.GetAllFields().GetEnumerator();
            while (expectedIter.MoveNext())
            {
                Assert.True(actualIter.MoveNext());
                IFudgeField expectedField = expectedIter.Current;
                IFudgeField actualField = actualIter.Current;

                Assert.Equal(expectedField.Name, actualField.Name);
                Assert.Equal(expectedField.Type, actualField.Type);
                Assert.Equal(expectedField.Ordinal, actualField.Ordinal);
                if (expectedField.Value.GetType().IsArray)
                {
                    Assert.Equal(expectedField.Value.GetType(), actualField.Value.GetType());
                    Assert.Equal(expectedField.Value, actualField.Value);       // XUnit will check all values in the arrays
                }
                else if (expectedField.Value is FudgeMsg)
                {
                    Assert.True(actualField.Value is FudgeMsg);
                    AssertAllFieldsMatch((FudgeMsg)expectedField.Value,
                        (FudgeMsg)actualField.Value);
                }
                else if (expectedField.Value is UnknownFudgeFieldValue)
                {
                    Assert.IsType<UnknownFudgeFieldValue>(actualField.Value);
                    UnknownFudgeFieldValue expectedValue = (UnknownFudgeFieldValue)expectedField.Value;
                    UnknownFudgeFieldValue actualValue = (UnknownFudgeFieldValue)actualField.Value;
                    Assert.Equal(expectedField.Type.TypeId, actualField.Type.TypeId);
                    Assert.Equal(expectedValue.Type.TypeId, actualField.Type.TypeId);
                    Assert.Equal(expectedValue.Contents, actualValue.Contents);
                }
                else
                {
                    Assert.Equal(expectedField.Value, actualField.Value);
                }
            }
            Assert.False(actualIter.MoveNext());
        }
    }
}
