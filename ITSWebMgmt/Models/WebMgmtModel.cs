using ITSWebMgmt.Caches;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Threading;

namespace ITSWebMgmt.Models
{
    public abstract class WebMgmtModel<T> where T : ADcache
    {
        public T ADcache;
        public SCCMcache SCCMcache;
        public string Path { get => ADcache.Path; }
        public virtual string adpath { get => ADcache.adpath; set { ADcache.adpath = value; } }
        public List<PropertyValueCollection> getAllProperties() => ADcache.getAllProperties();

        protected void LoadDataInbackground()
        {
            //Load data into ADcache in the background
            ThreadPool.QueueUserWorkItem(_ =>
            {
                ADcache.getGroups("memberOf");
                ADcache.getGroupsTransitive("memberOf");
                ADcache.getAllProperties();
            }, null);

            //Load data into SCCMcache in the background
            ThreadPool.QueueUserWorkItem(_ =>
            {
                SCCMcache.LoadAllIntoCache();
            }, null);
        }
    }
}
