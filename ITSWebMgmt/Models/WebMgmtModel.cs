using ITSWebMgmt.Caches;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Threading;

namespace ITSWebMgmt.Models
{
    public abstract class WebMgmtModel<T> where T : ADCache
    {
        public T ADCache;
        public SCCMCache SCCMCache;
        public string Path { get => ADCache.Path; }
        public virtual string ADPath { get => ADCache.ADPath; set { ADCache.ADPath = value; } }
        public List<PropertyValueCollection> GetAllProperties() => ADCache.GetAllProperties();

        protected void LoadDataInbackground()
        {
            //Load data into ADCache in the background
            ThreadPool.QueueUserWorkItem(_ =>
            {
                ADCache.GetGroups("memberOf");
                ADCache.GetGroupsTransitive("memberOf");
                ADCache.GetAllProperties();
            }, null);

            //Load data into SCCMCache in the background
            ThreadPool.QueueUserWorkItem(_ =>
            {
                SCCMCache.LoadAllIntoCache();
            }, null);
        }
    }
}
