namespace cz.dvojak.k8s.EdgeOperator.Configuration.Options;

public class ValidatorOption
{
    public const string VALIDATOR = "Validator";

    public bool DeviceStrict { get; set; }
    public bool ConnectionStrict { get; set; }
}