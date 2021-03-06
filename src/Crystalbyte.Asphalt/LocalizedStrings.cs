﻿#region Using directives

using Crystalbyte.Asphalt.Resources;

#endregion

namespace Crystalbyte.Asphalt {
    /// <summary>
    ///   Provides access to string resources.
    /// </summary>
    public sealed class LocalizedStrings {
        private static readonly AppResources AppResources = new AppResources();

        public AppResources LocalizedResources {
            get { return AppResources; }
        }
    }
}