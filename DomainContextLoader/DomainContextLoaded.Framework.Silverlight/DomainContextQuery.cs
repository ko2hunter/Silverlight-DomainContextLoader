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
    internal abstract class DomainContextQuery
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DomainContextQuery&lt;T&gt;"/> is complete.
        /// </summary>
        /// <value><c>true</c> if complete; otherwise, <c>false</c>.</value>
        public bool Complete { get; protected set; }

        /// <summary>
        /// Gets or sets the DomainContext that is to be queried.
        /// </summary>
        /// <value>The context.</value>
        public DomainContext Context { get; protected set; }

        /// <summary>
        /// Runs the current DomainContextQuery that does not require an AsyncQueryObject
        /// </summary>
        /// <param name="completedCallback">The completed callback.</param>
        public abstract void Run(Action completedCallback);

        /// <summary>
        /// Runs the current DomainContextQuery that contains an AsyncQueryObject
        /// </summary>
        /// <param name="completedCallback">The completed callback.</param>
        public abstract void Run(Action<object> completedCallback);
    }
}
