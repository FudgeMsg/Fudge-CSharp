/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGamma.Fudge.Taxon;
using System.IO;
using OpenGamma.Fudge.Util;

namespace OpenGamma.Fudge
{
    /// <summary>
    /// The primary entry-point for code to interact with the rest of the fudge system.
    /// For performance reasons, there are many options that are passed around as parameters
    /// inside static methods for encoding and decoding, and many lightweight objects that
    /// ideally don't know of their configuration context.
    /// However, in a large application, it is often desirable to collect all configuration
    /// parameters in one location and inject options into it.
    /// <p/>
    /// <c>FudgeContext</c> allows application developers to have a single location
    /// to inject dependent parameters and instances, and make them available through
    /// simple method invocations. in addition, because it wraps all checked exceptions
    /// into instances of <see cref="FudgeRuntimeException"/>, it is the ideal way to use
    /// the fudge encoding system from within spring applications.
    /// <p/>
    /// While most applications will have a single instance of <c>FudgeContext</c>,
    /// some applications will have one instance per unit of encoding/decoding parameters.
    /// for example, if an application is consuming data from two messaging feeds, each
    /// of which reuses the same taxonomy id to represent a different
    /// <see cref="IFudgeTaxonomy"/>, it would configure two different instances of
    /// <c>FudgeContext</c>, one per feed.  
    /// </summary>
    public class FudgeContext
    {
        private ITaxonomyResolver taxonomyResolver;

        public ITaxonomyResolver TaxonomyResolver
        {
            get { return taxonomyResolver; }
            set { taxonomyResolver = value; }
        }

        public void Serialize(FudgeMsg msg, Stream s)
        {
            Serialize(msg, null, s);
        }

        public void Serialize(FudgeMsg msg, short? taxonomyId, Stream s)
        {
            IFudgeTaxonomy taxonomy = null;
            if ((TaxonomyResolver != null) && (taxonomyId != null))
            {
                taxonomy = TaxonomyResolver.ResolveTaxonomy(taxonomyId.Value);
            }
            BinaryWriter bw = new FudgeBinaryWriter(s);
            try
            {
                FudgeStreamEncoder.WriteMsg(bw, new FudgeMsgEnvelope(msg), taxonomy, taxonomyId ?? 0);
            }
            catch (IOException e)
            {
                throw new FudgeRuntimeException("Unable to write Fudge message to OutputStream", e);
            }
        }

        public byte[] ToByteArray(FudgeMsg msg)
        {
            MemoryStream stream = new MemoryStream();
            Serialize(msg, stream);
            return stream.ToArray();
        }  

        public FudgeMsgEnvelope Deserialize(Stream s)
        {
            BinaryReader br = new FudgeBinaryReader(s);
            FudgeMsgEnvelope envelope;
            try
            {
                envelope = FudgeStreamDecoder.ReadMsg(br, TaxonomyResolver);
            }
            catch (IOException e)
            {
                throw new FudgeRuntimeException("Unable to deserialize FudgeMsg from input stream", e);
            }
            return envelope;
        }

        public FudgeMsgEnvelope Deserialize(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes, false);
            return Deserialize(stream);
        }
  

    }
}
