#region Using directives

using System;
using Microsoft.Live;

#endregion

namespace Crystalbyte.Asphalt.Data {
    public sealed class LiveProgressConverter : IProgress<LiveOperationProgress> {
        private readonly IProgressAware _observer;

        public LiveProgressConverter(IProgressAware observer) {
            _observer = observer;
        }

        #region Implementation of IProgress<in LiveOperationProgress>

        public void Report(LiveOperationProgress value) {
            _observer.ReportProgress(value.ProgressPercentage);
        }

        #endregion
    }
}