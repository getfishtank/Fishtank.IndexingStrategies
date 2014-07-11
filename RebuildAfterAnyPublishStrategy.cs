using System;
using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.Maintenance.Strategies;
using Sitecore.Data;
using Sitecore.Diagnostics;

namespace Fishtank.IndexingStrategies
{
    public class RebuildAfterAnyPublishStrategy: IIndexUpdateStrategy
    {
        // Fields
        protected ISearchIndex index;

        protected const string ClassName = "Fishtank.IndexingStrategies.RebuildAfterAnyPublishStrategy";

        // Methods
        public RebuildAfterAnyPublishStrategy(string database)
        {
            Assert.IsNotNullOrEmpty(database, "database");
            this.Database = Factory.GetDatabase(database);
            Assert.IsNotNull(this.Database, string.Format("Database '{0}' was not found", database));
        }

        protected virtual void Handle()
        {
            OperationMonitor.Register(new Action(this.Run));
            OperationMonitor.Trigger();
        }

        public virtual void Initialize(ISearchIndex index)
        {
            Assert.IsNotNull(index, "index");
            CrawlingLog.Log.Info(string.Format("[Index={0}] Initializing {1}.", index.Name, ClassName), null);
            this.index = index;
            if (!Settings.EnableEventQueues)
            {
                CrawlingLog.Log.Fatal(string.Format("[Index={0}] Initialization of {1} failed because event queue is not enabled.", index.Name, ClassName), null);
            }
            else
            {
                EventHub.PublishEnd += (sender, args) => this.Handle();
            }
        }

        public virtual void Run()
        {
            CrawlingLog.Log.Info(string.Format("[Index={0}] {1} triggered.", this.index.Name, ClassName), null);
            if (this.Database == null)
            {
                CrawlingLog.Log.Fatal(string.Format("[Index={0}] OperationMonitor has invalid parameters. Index Update cancelled.", this.index.Name), null);
            }
            else
            {
                CrawlingLog.Log.Info(string.Format("[Index={0}] Full Rebuild.", this.index.Name), null);
                IndexCustodian.FullRebuild(this.index, true);
            }
        }

        // Properties
        public Database Database { get; protected set; }
    }
}
