﻿// -----------------------------------------------------------------------
// Copyright © Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;

namespace System.Composition
{
    /// <summary>
    /// Applied to an import for ExportFactory{T}, this attribute marks the
    /// boundary of a sharing scope. The ExportLifetimeContext{T} instances
    /// returned from the factory will be boundaries for sharing of components that are bounded
    /// by the listed boundary names.
    /// </summary>
    /// <example>
    /// [Import, SharingBoundary("HttpRequest")]
    /// public ExportFactory&lt;HttpRequestHandler&gt; HandlerFactory { get; set; }
    /// </example>
    /// <seealso cref="SharedAttribute" />
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited=false)]
    [MetadataAttribute]
    public sealed class SharingBoundaryAttribute : Attribute
    {
        readonly string[] _sharingBoundaryNames;

        const string SharingBoundaryImportMetadataConstraintName = "SharingBoundaryNames";

        /// <summary>
        /// Construct a <see cref="SharingBoundaryAttribute"/> for the specified boundary names.
        /// </summary>
        /// <param name="sharingBoundaryNames">Boundaries implemented by the created ExportLifetimeContext{T}s.</param>
        public SharingBoundaryAttribute(params string[] sharingBoundaryNames)
        {
            if (sharingBoundaryNames == null) throw new ArgumentNullException("boundaries");

            _sharingBoundaryNames = sharingBoundaryNames;
        }

        /// <summary>
        /// Boundaries implemented by the created ExportLifetimeContext{T}s.
        /// </summary>
        public ReadOnlyCollection<string> SharingBoundaryNames { get { return new ReadOnlyCollection<string>(_sharingBoundaryNames); } }
    }
}
