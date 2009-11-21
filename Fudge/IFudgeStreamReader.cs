/*
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
using Fudge.Taxon;

namespace Fudge
{
    public interface IFudgeStreamReader
    {
        bool HasNext { get; }

        FudgeStreamElement MoveNext();

        FudgeStreamElement CurrentElement { get; }

        int ProcessingDirectives { get; }

        int SchemaVersion { get; }

        int TaxonomyId { get; }

        int EnvelopeSize { get; }

        FudgeFieldType FieldType { get; }

        int? FieldOrdinal { get; }

        string FieldName { get; }

        object FieldValue { get; }

        IFudgeTaxonomy Taxonomy { get; }
    }
}
