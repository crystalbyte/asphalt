#region Using directives

using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Live;

#endregion

namespace Crystalbyte.Asphalt.Data {
    /// <summary>
    ///   Uploads exprorted data to the user's skydrive account.
    ///   See link for upload guidelines: http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh202955(v=vs.105).aspx
    /// </summary>
    [Export(typeof (IExportStrategy)), Shared]
    public sealed class SkyDriveExportStrategy : IExportStrategy {
        private const string SingleSignInScope = "wl.signin";
        private const string SkyDriveReadOnlyScope = "wl.skydrive";
        private const string SkyDriveUpdateScope = "wl.skydrive_update";

        private const string AsphaltClientId = "00000000480FCE7B";

        [Import]
        public Navigator Navigator { get; set; }

        [Import]
        public LocalStorage LocalStorage { get; set; }

        private static string CreateTimeStampedFilename(string extension) {
            var time = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            return string.Format("asphalt_export_{0}.{1}", time, extension);
        }

        #region Implementation of IExportStrategy

        public string FriendlyName {
            get { return "SkyDrive"; }
        }

        public async Task ExportAsync(string data, string extension, IProgressAware observer) {
            var name = CreateTimeStampedFilename(extension);
            await LocalStorage.SaveExportAsync(name, data);

            var auth = new LiveAuthClient(AsphaltClientId);
            await auth.LoginAsync(new[] {SingleSignInScope, SkyDriveReadOnlyScope, SkyDriveUpdateScope});

            var client = new LiveConnectClient(auth.Session);
            var token = new CancellationToken(false);

            await client.BackgroundUploadAsync("me/skydrive/my_documents",
                                               new Uri(string.Format(@"\shared\transfers\{0}", name), UriKind.Relative),
                                               OverwriteOption.Rename, token, new LiveProgressConverter(observer));

            await LocalStorage.DeleteExportAsync(name);
        }

        #endregion
    }
}