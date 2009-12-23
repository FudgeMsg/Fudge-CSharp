/* <!--
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
 * -->
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQToolkit;
using System.Linq.Expressions;

namespace Fudge.Linq
{
    /// <summary>
    /// ExpressionTreeStructureHasher computes a fast hash code based on the structure of the tree, ignoring any constant values, method names, etc.
    /// </summary>
    public class ExpressionTreeStructureHasher : ExpressionVisitor
    {
        private readonly Expression expression;
        private int currentHash;

        public ExpressionTreeStructureHasher(Expression e)
        {
            this.expression = e;
        }

        public int ComputeHash()
        {
            currentHash = 11;        // Starter value

            this.Visit(expression);

            return currentHash;
        }

        public static int ComputeHash(Expression e)
        {
            return new ExpressionTreeStructureHasher(e).ComputeHash();
        }

        protected override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                currentHash = currentHash * 31;
            }
            else
            {
                int nodeType = (int)exp.NodeType;
                int typeHash = exp.Type.GetHashCode();

                currentHash = currentHash * 31 + nodeType;
                currentHash = currentHash * 31 + typeHash;
            }

            return base.Visit(exp);
        }
    }
}
