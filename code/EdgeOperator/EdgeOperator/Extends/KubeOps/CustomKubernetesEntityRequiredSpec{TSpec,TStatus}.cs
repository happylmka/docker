namespace cz.dvojak.k8s.EdgeOperator.Extends.KubeOps;

// This is just to make spec in the CRD required (in json schema) see PR [#520](https://github.com/buehler/dotnet-operator-sdk/pull/520)
public abstract class CustomKubernetesEntityRequiredSpec<TSpec, TStatus> : CustomKubernetesEntityRequiredSpec<TSpec>
    where TSpec : new() where TStatus : new()
{
    public TStatus Status { get; set; } = new();
}