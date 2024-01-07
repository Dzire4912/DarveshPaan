namespace TAN.DomainModels.Helpers
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class SkipAuditAttribute : Attribute
    {
        
    }

}
