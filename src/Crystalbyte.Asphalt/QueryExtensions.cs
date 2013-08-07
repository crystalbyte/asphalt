#region Using directives

using System;
using System.Threading.Tasks;
using Microsoft.Phone.Maps.Services;

#endregion

namespace Crystalbyte.Asphalt {
    /// <summary>
    ///   See limitations on query usage.
    ///   http://stackoverflow.com/questions/17784234/exception-in-reversegeocodequery
    /// </summary>
    public static class QueryExtensions {
        public static Task<T> ExecuteAsync<T>(this Query<T> query) {
            var taskSource = new TaskCompletionSource<T>();

            EventHandler<QueryCompletedEventArgs<T>> handler = null;

            handler = (s, e) => {
                          query.QueryCompleted -= handler;

                          if (e.Cancelled)
                              taskSource.SetCanceled();
                          else if (e.Error != null)
                              taskSource.SetException(e.Error);
                          else
                              taskSource.SetResult(e.Result);
                      };

            query.QueryCompleted += handler;

            query.QueryAsync();

            return taskSource.Task;
        }
    }
}