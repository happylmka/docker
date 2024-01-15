using KubeOps.Operator.Entities;
using KubeOps.Operator.Entities.Annotations;

namespace cz.dvojak.k8s.EdgeOperator.Extends.KubeOps;

// This is just to make spec in the CRD required (in json schema) see PR [#520](https://github.com/buehler/dotnet-operator-sdk/pull/520)
public abstract class CustomKubernetesEntityRequiredSpec<TSpec> : CustomKubernetesEntity<TSpec>
    where TSpec : new()
{
    [Required] public new TSpec Spec { get; set; } = new();
}