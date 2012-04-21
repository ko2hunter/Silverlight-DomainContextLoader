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
using System.Collections.Generic;
using System.Linq;

namespace DomainContextLoader.Framework.QueryLoader
{
    /// <summary>
    /// Class for wrapping DomainContext EntityQueries so that multiple queries can be done asyncronously or syncronously 
    /// and notifying the listeners that all the queries have completed.
    /// </summary>
    public class MultiSyncQueryLoader
    {
        /// <summary>
        /// Occurs when the Domain Context Queries are completed, whether asyncronously or syncronously.
        /// </summary>
        public event Action Completed;

        /// <summary>
        /// container field for the DomainContextQueries that are needed to run
        /// </summary>
        private List<DomainContextQuery> _queries = new List<DomainContextQuery>();

        /// <summary>
        /// The default domain context used when the domain context is not 
        /// specified by the method
        /// </summary>
        private DomainContext _defaultDomainContext = null;

        /// <summary>
        /// The default load behaviour used when the load behaviour is not
        /// specified by the method.
        /// </summary>
        private LoadBehavior _defaultLoadBehaviour  = LoadBehavior.RefreshCurrent;

        /// <summary>
        /// Gets or sets the default domain context.
        /// </summary>
        /// <value>
        /// The default domain context.
        /// </value>
        public DomainContext DefaultDomainContext
        {
            get
            {
                return _defaultDomainContext;
            }
            set
            {
                _defaultDomainContext = value;
            }
        }

        /// <summary>
        /// Gets or sets the default load behaviour.
        /// </summary>
        /// <value>
        /// The default load behaviour.
        /// </value>
        public LoadBehavior DefaultLoadBehaviour
        {
            get
            {
                return _defaultLoadBehaviour;
            }
            set
            {
                _defaultLoadBehaviour = value;
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSyncQueryLoader"/> class.
        /// </summary>
        public MultiSyncQueryLoader()
        { }

        /// <summary>
        /// Initializes a new instance of the MultiSyncQueryLoader class and specifies
        /// the default load behaviour and domain context.
        /// </summary>
        /// <param name="defaultDomainContext"> The default domain context. </param>
        /// <param name="defaultLoadBehaviour"> The default load behaviour. </param>
        public MultiSyncQueryLoader(DomainContext defaultDomainContext, LoadBehavior defaultLoadBehaviour)
        {
            _defaultDomainContext = defaultDomainContext;
            _defaultLoadBehaviour = defaultLoadBehaviour;
        }

        /// <summary>
        /// Initializes a new instance of the MultiSyncQueryLoader class and sets the
        /// domain context to use for all queries.  
        /// </summary>
        /// <remarks>
        /// By default this uses the RefreshCurrent Load Behaviour
        /// </remarks>
        /// <param name="defaultDomainContext"> The default domain context. </param>
        public MultiSyncQueryLoader(DomainContext defaultDomainContext)
        {
            _defaultDomainContext = defaultDomainContext;
        }

        /// <summary>
        /// Adds the DomainContext and required EntityQuery
        /// </summary>
        /// <typeparam name="T">The Entity type for the DomainContext load</typeparam>
        /// <param name="context">The DomainContext to against which the query will be run.</param>
        /// <param name="query">The EntityQuery that will be run against the DomainContext.</param>
        /// <param name="loadBehavior">The load behavior of the LoadOperation</param>
        /// <param name="callback">The callback for the LoadOperation.</param>
        /// <param name="userStateParam">The user state param for this loadoperation.</param>
        public void AddQuery<T>(DomainContext context, EntityQuery<T> query, LoadBehavior loadBehavior, Action<LoadOperation<T>> callback, object userStateParam)
            where T : Entity
        {
            var domainQuery = new DomainContextQuery<T>(context, query, loadBehavior, callback, userStateParam);
            _queries.Add(domainQuery);
        }

        /// <summary>
        /// Adds the DomainContext and required EntityQuery using the default domain context
        /// </summary>
        /// <typeparam name="T">The Entity type for the DomainContext load</typeparam>
        /// <param name="query">The EntityQuery that will be run against the DomainContext.</param>
        /// <param name="loadBehavior">The load behavior of the LoadOperation</param>
        /// <param name="callback">The callback for the LoadOperation.</param>
        /// <param name="userStateParam">The user state param for this loadoperation.</param>
        public void AddQuery<T>(EntityQuery<T> query, LoadBehavior loadBehavior, Action<LoadOperation<T>> callback, object userStateParam)
            where T : Entity
        {
            if (_defaultDomainContext == null)
            {
                throw new System.NullReferenceException("DomainContext");
            }

            var domainQuery = new DomainContextQuery<T>(DefaultDomainContext, query, loadBehavior, callback, userStateParam);
            _queries.Add(domainQuery);
        }

        /// <summary>
        /// Adds the DomainContext and required EntityQuery using the default domain context
        /// </summary>
        /// <typeparam name="T">The Entity type for the DomainContext load</typeparam>
        /// <param name="query">The EntityQuery that will be run against the DomainContext.</param>
        /// <param name="callback">The callback for the LoadOperation.</param>
        /// <param name="userStateParam">The user state param for this loadoperation.</param>
        public void AddQuery<T>(EntityQuery<T> query, Action<LoadOperation<T>> callback, object userStateParam)
            where T : Entity
        {
            if (_defaultDomainContext == null)
            {
                throw new System.NullReferenceException("DomainContext");
            }

            var domainQuery = new DomainContextQuery<T>(DefaultDomainContext, query, DefaultLoadBehaviour, callback, userStateParam);
            _queries.Add(domainQuery);
        }
        
        /// <summary>
        /// Adds the DomainContext and required EntityQuery using the default domain context
        /// </summary>
        /// <typeparam name="T">The Entity type for the DomainContext load</typeparam>
        /// <param name="query">The EntityQuery that will be run against the DomainContext.</param>
        /// <param name="callback">The callback for the LoadOperation.</param>
        public void AddQuery<T>(EntityQuery<T> query, Action<LoadOperation<T>> callback)
            where T : Entity
        {
            this.AddQuery(query, callback, null);
        }

        /// <summary>
        /// Runs the load operations asyncronously in the order they were added.
        /// They will progress only as the previous completes.
        /// </summary>
        public void RunSync()
        {
            SyncCallback();
        }

        /// <summary>
        /// Runs all the queries asyncronously.
        /// They progress simultaneously
        /// </summary>
        public void RunAsync()
        {
            if (_queries.Count <= 0)
            {
                OnCompleted();
            }

            _queries.ForEach(a => a.Run(AsyncCallback));
        }

        /// <summary>
        /// Callback for the Async load operations.
        /// </summary>
        /// <param name="asyncQueryObject">The DomainContextQuery that is currently returning.</param>
        private void AsyncCallback(object asyncQueryObject)
        {
            if (asyncQueryObject == null)
            {
                throw new Exception("Async Query Object cannot be null in an Async Callback");
            }

            if (_queries.FirstOrDefault(a=>a.Complete == false) == null)
            {
                OnCompleted();
            }
        }

        /// <summary>
        /// Syncronous callback for running the queries in order.
        /// </summary>
        private void SyncCallback()
        {
            DomainContextQuery query = _queries.FirstOrDefault(a => a.Complete == false);
            if (query == null)
            {
                OnCompleted();
                return;
            }

            query.Run(SyncCallback);
        }

        /// <summary>
        /// Called when the _queries list is empty.
        /// </summary>
        private void OnCompleted()
        {
            if (Completed != null)
            {
                Completed();
            }
        }

        /// <summary>
        /// Clears the list of queries that are to be run against the DomainContexts
        /// </summary>
        public void Clear()
        {
            _queries.Clear();
        }
    }
}
