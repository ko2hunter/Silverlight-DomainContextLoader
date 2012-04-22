using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ServiceModel.DomainServices.Client;

namespace DomainContextLoader.Framework.QueryLoader
{
    /// <summary>
    /// Used for the AsyncQueryLoader to run queries against the specified DomainContext.
    /// </summary>
    /// <typeparam name="T">The Entity Type for the EntityQuery to Run</typeparam>
    internal class DomainContextQuery<T> : DomainContextQuery
        where T : Entity
    {
        /// <summary>
        /// Gets or sets the EntityQuery that is to be run agains the DomainContext
        /// </summary>
        /// <value>The query.</value>
        public EntityQuery<T> Query { get; private set; }

        /// <summary>
        /// Gets or sets the load behaviour for the DomainContext load.
        /// </summary>
        /// <value>The load behaviour.</value>
        LoadBehavior LoadBehaviour { get; set; }

        /// <summary>
        /// Gets or sets the LoadOperation Callback
        /// </summary>
        /// <value>The callback.</value>
        Action<LoadOperation<T>> Callback { get; set; }

        /// <summary>
        /// Gets or sets the user state parameters for the LoadOperation
        /// </summary>
        /// <value>The user state param.</value>
        object UserStateParam { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainContextQuery&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="context">The DomainContext.</param>
        /// <param name="query">The EntityQuery.</param>
        /// <param name="loadBehaviour">The LoadBehaviour.</param>
        /// <param name="callback">The Load Operation Callback.</param>
        /// <param name="userStateParam">The user state param.</param>
        public DomainContextQuery(DomainContext context, EntityQuery<T> query, LoadBehavior loadBehaviour, Action<LoadOperation<T>> callback, object userStateParam)
        {
            Context = context;
            Query = query;
            LoadBehaviour = loadBehaviour;
            Callback = callback;
            UserStateParam = userStateParam;
        }

        /// <summary>
        /// Runs the current DomainContextQuery that does not require an AsyncQueryObject
        /// </summary>
        /// <param name="completedCallback">The completed callback.</param>
        public override void Run(Action completedCallback)
        {
            Run(new Action<object>(obj => { completedCallback.Invoke(); }));
        }

        /// <summary>
        /// Field for storing the completed callback for the Run method
        /// </summary>
        private Action<object> _completedCallback;

        /// <summary>
        /// Runs the current DomainContextQuery that contains an AsyncQueryObject
        /// </summary>
        /// <param name="completedCallback">The completed callback.</param>
        public override void Run(Action<object> completedCallback)
        {
            _completedCallback = completedCallback;

            Complete = false;
            Running = true;
            // Query the DomainContext with the parameters specified by this current DomainContextQuery.
            // Use our own LoadOperation method callback, so that the completedCallback action can be notified upon completion
            Context.Load(Query, LoadBehaviour, OnQueryLoadCompleted, UserStateParam);
        }

        /// <summary>
        /// Called when [query load completed].
        /// </summary>
        /// <param name="loadOperation">The load operation.</param>
        private void OnQueryLoadCompleted(LoadOperation<T> loadOperation)
        {
            Complete = true;

            // If there is a callback specified, call this with the loadOperation
            if (Callback != null)
            {
                Callback.Invoke(loadOperation);
            }

            // If the completed callback is not null, let completedCallback action know we are done.
            if (_completedCallback != null)
            {
                _completedCallback.Invoke(this);
            }

            Running = false;
        }
    }
}
