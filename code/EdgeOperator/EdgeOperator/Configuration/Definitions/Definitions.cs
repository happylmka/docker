namespace cz.dvojak.k8s.EdgeOperator.Configuration.Definitions;

public static class Definitions
{
    public static class Api
    {
        public const string GROUP = "edge-operator.k8s.dvojak.cz";
        public const string API_VERSION = "v1";
    }

    public static class ExternalApi
    {
        public const string NAD_GROUP = "k8s.cni.cncf.io";
        public const string NAD_API_VERSION = "v1";
        public const string NAD_PLURAL_NAME = "network-attachment-definitions";
    }
}