/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc. and other contributors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *     
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Fudge.Linq;
using IQToolkit;

namespace Fudge.Tests.Unit.Linq
{
    public class ExpressionTreeStructureHasherTest
    {
        [Fact]
        public void SameQueryStructureSameHash()
        {
            var data = new int[] {1, 3, 5, 6};

            var query1 = from entry in data.AsQueryable() where entry % 2 == 0 select entry * 4;
            var query2 = from entry in data.AsQueryable() where entry % 2 == 0 select entry * 4; 
            var hash1 = ExpressionTreeStructureHasher.ComputeHash(query1.Expression);
            var hash2 = ExpressionTreeStructureHasher.ComputeHash(query2.Expression);

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void DifferentQueryDifferetHash()
        {
            var data = new int[] { 1, 3, 5, 6 };

            var query1 = from entry in data.AsQueryable() where entry % 2 == 0 select entry * 4;
            var query2 = from entry in data.AsQueryable() where entry == 3 select entry * 4;
            var hash1 = ExpressionTreeStructureHasher.ComputeHash(query1.Expression);
            var hash2 = ExpressionTreeStructureHasher.ComputeHash(query2.Expression);

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void ConstantsNotRelevant()
        {
            var data = new int[] { 1, 3, 5, 6 };

            var query1 = from entry in data.AsQueryable() where entry % 2 == 0 select entry * 4;
            var query2 = from entry in data.AsQueryable() where entry % 3 == 2 select entry * 7;
            var hash1 = ExpressionTreeStructureHasher.ComputeHash(query1.Expression);
            var hash2 = ExpressionTreeStructureHasher.ComputeHash(query2.Expression);

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void ParameterValuesNotRelevant()
        {
            var data = new int[] { 1, 3, 5, 6 };

            int mod = 2;
            var query1 = from entry in data.AsQueryable() where entry % mod == 0 select entry * 4;
            mod = 3;
            var query2 = from entry in data.AsQueryable() where entry % mod == 0 select entry * 4;
            var hash1 = ExpressionTreeStructureHasher.ComputeHash(query1.Expression);
            var hash2 = ExpressionTreeStructureHasher.ComputeHash(query2.Expression);

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void CheckExpressionComparerBehaviour()
        {
            // This test is just to validate how IQToolkit.ExpressionComparer behaves
            var data = new int[] { 1, 3, 5, 6 };
            int mod = 2;
            var query1 = from entry in data.AsQueryable() where entry % mod == 0 select entry * 4;
            mod = 3;
            var query2 = from entry in data.AsQueryable() where entry % mod == 0 select entry * 4;
            var query3 = from entry in data.AsQueryable() where entry % mod == 0 select entry * 5;
            var query4 = from entry in data.AsQueryable() where entry % 2 == 0 select entry * 4;

            Assert.False(ExpressionComparer.AreEqual(query1.Expression, query2.Expression));            // Different parameter value
            Assert.True(ExpressionComparer.AreEqual(query1.Expression, query2.Expression, false));      // Different parameter value
            Assert.True(ExpressionComparer.AreEqual(query1.Expression, query3.Expression, false));      // Different constant
            Assert.False(ExpressionComparer.AreEqual(query1.Expression, query4.Expression, false));     // Constant versus parameter
        }
    }
}
