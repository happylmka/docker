namespace cz.dvojak.k8s.EdgeOperator.Configuration.Options;

public class ProxyTemplateOption
{
    public const string PROXY_TEMPLATE = "ProxyTemplate";
    public string ImageName { get; set; } = null!;
    public string[] Args { get; set; } = null!;
    public int BasePort { get; set; }
    public string TcpCommandTemplate { get; set; } = null!;
    public string UdpCommandTemplate { get; set; } = null!;
}