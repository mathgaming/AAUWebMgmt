using System.Collections.Generic;

namespace ITSWebMgmt.Caches
{
    public class GroupADCache : ADCache
    {
        public GroupADCache(string ADPath) : base(ADPath, new List<Property>
        {
            new Property("memberOf", typeof(object[])),
            new Property("member", typeof(object[])),
            new Property("description", typeof(string)),
            new Property("info", typeof(string)),
            new Property("name", typeof(string)),
            new Property("managedBy", typeof(string)),
            new Property("groupType", typeof(int)),
            new Property("distinguishedName", typeof(string)),
        }, null)
        { }
    }
}